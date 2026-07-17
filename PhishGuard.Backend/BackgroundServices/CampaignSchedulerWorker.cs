using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;

// Expõe o método de ciclo (internal) para o projeto de testes validar a regra de
// elegibilidade do worker (apenas campanhas "Agendada" com DataInicio já atingida).
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PhishGuard.Tests")]

namespace PhishGuard.Backend.BackgroundServices
{
    /// <summary>
    /// Serviço hospedado que orquestra o disparo AUTOMÁTICO das campanhas agendadas.
    /// A cada ciclo, busca campanhas cujo horário de início já chegou (DataInicio &lt;=
    /// agora) e que ainda não foram disparadas, e aciona o disparo em lote via
    /// <see cref="ICampaignDispatchService"/>.
    /// </summary>
    public class CampaignSchedulerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CampaignSchedulerWorker> _logger;

        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(1);

        public CampaignSchedulerWorker(IServiceScopeFactory scopeFactory, ILogger<CampaignSchedulerWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CampaignSchedulerWorker iniciado. Ciclo a cada {Intervalo}.", Intervalo);

            using var timer = new PeriodicTimer(Intervalo);
            do
            {
                try
                {
                    await ProcessarCampanhasElegiveisAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Encerramento normal do host — não é erro.
                }
                catch (Exception ex)
                {
                    // Um erro no ciclo NÃO pode derrubar o worker; loga e aguarda o próximo tick.
                    _logger.LogError(ex, "Erro inesperado no ciclo do CampaignSchedulerWorker.");
                }
            }
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));
        }

        internal async Task ProcessarCampanhasElegiveisAsync(CancellationToken stoppingToken)
        {
            // Escopo dedicado por ciclo: AppDbContext e o serviço de disparo são scoped;
            // resolvê-los de um escopo próprio evita injetar scoped no singleton do worker
            // e conflitos de concorrência entre ciclos (PASSO 2).
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dispatcher = scope.ServiceProvider.GetRequiredService<ICampaignDispatchService>();

            var agora = DateTime.UtcNow;

            await DispararAgendadasAsync(context, dispatcher, agora, stoppingToken);
            await FinalizarEncerradasAsync(context, agora, stoppingToken);
        }

        // (1) DISPARO: processa campanhas "Agendada" cujo horário de início chegou E
        // campanhas "Processando" (claim já feito, mas o lote ainda não terminou — típico
        // de uma RETOMADA após queda do container). Campanhas em "Rascunho" são ignoradas.
        //
        // Claim atômico: antes de disparar, a "Agendada" é movida para "Processando" e
        // PERSISTIDA. Se o processo cair no meio do lote, a campanha permanece
        // "Processando" e é reprocessada aqui, com o disparo pulando (idempotência) quem
        // já recebeu. Uma "Agendada" que caísse antes do claim não seria reenviada em
        // duplicidade, apenas reprocessada do zero de forma idempotente.
        // NOTA (scale-out): com múltiplas instâncias do worker, este claim load→save deve
        // virar um UPDATE condicional com token de concorrência (xmin/rowversion) para ser
        // atômico entre processos. Com uma única instância (arquitetura atual) já é seguro.
        //
        // IgnoreQueryFilters: o worker é um processo de sistema, sem contexto de tenant.
        // Opera sobre todas as campanhas e cada disparo é escopado pelo TenantId próprio
        // da campanha (o filtro global por tenant zeraria o resultado aqui).
        private async Task DispararAgendadasAsync(
            AppDbContext context, ICampaignDispatchService dispatcher, DateTime agora, CancellationToken stoppingToken)
        {
            var elegiveis = await context.Campaigns
                .IgnoreQueryFilters()
                .Include(c => c.Template)
                .Include(c => c.Targets)
                .Where(c => (c.Status == CampaignStatus.Agendada && c.DataInicio <= agora)
                            || c.Status == CampaignStatus.Processando)
                .ToListAsync(stoppingToken);

            if (elegiveis.Count == 0)
                return;

            _logger.LogInformation("{Total} campanha(s) elegível(is) para disparo/retomada.", elegiveis.Count);

            foreach (var campanha in elegiveis)
            {
                // Isolamento por campanha: a falha no disparo de UMA campanha não
                // interrompe as demais nem derruba o serviço.
                try
                {
                    if (!campanha.Targets.Any())
                    {
                        _logger.LogWarning("Campanha {Id} sem alvos associados; ignorada neste ciclo.", campanha.Id);
                        continue;
                    }

                    // Claim ATÔMICO: reivindica a campanha antes de enviar (persistido). O
                    // token de concorrência 'xmin' (ver AppDbContext) garante que, com
                    // múltiplas instâncias do worker, apenas UMA vença o claim — a perdedora
                    // recebe DbUpdateConcurrencyException e pula a campanha neste ciclo,
                    // evitando disparo duplicado do lote.
                    if (campanha.Status == CampaignStatus.Agendada)
                    {
                        campanha.Status = CampaignStatus.Processando;
                        try
                        {
                            await context.SaveChangesAsync(stoppingToken);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            _logger.LogInformation(
                                "Campanha {Id} já reivindicada por outra instância; ignorada neste ciclo.",
                                campanha.Id);
                            continue;
                        }
                    }

                    // Log de INÍCIO: marca no timeline o momento em que o claim já foi feito e
                    // o disparo (I/O de SMTP) vai começar. Se a próxima linha do log for uma
                    // falha classificada, fica claro que o erro é no ENVIO, não no claim/fila.
                    _logger.LogInformation(
                        "Iniciando disparo da campanha {Id} ({Total} alvo(s)).",
                        campanha.Id, campanha.Targets.Count);

                    await dispatcher.DispatchAsync(campanha, stoppingToken);
                    _logger.LogInformation("Campanha {Id} disparada automaticamente.", campanha.Id);
                }
                catch (OperationCanceledException)
                {
                    throw; // propaga o cancelamento do host.
                }
                catch (Exception ex)
                {
                    // LOG ROBUSTO: classifica a causa provável para isolar, em produção, se a
                    // falha foi de AUTENTICAÇÃO SMTP, de CONEXÃO, de PROTOCOLO/TLS ou de
                    // CONFIGURAÇÃO/FILA (SMTP ausente, template/alvos faltando) — sem precisar
                    // decifrar a stack trace crua. A campanha permanece em 'Processando' e é
                    // reprocessada no próximo ciclo de forma idempotente; as demais seguem.
                    var categoria = ClassificarFalhaDisparo(ex);
                    _logger.LogError(ex,
                        "Falha ao disparar a campanha {Id}. Categoria provável: {Categoria}. A campanha " +
                        "permanece em '{Status}' e será reprocessada no próximo ciclo; as demais seguem normalmente.",
                        campanha.Id, categoria, campanha.Status);
                }
            }
        }

        // Classifica a causa PROVÁVEL de uma falha de disparo para o log operacional. Os
        // tipos são totalmente qualificados de propósito: 'AuthenticationException' existe em
        // MailKit.Security E em System.Security.Authentication, e o disparo pode lançar
        // 'InvalidOperationException' (config/fila) — qualificar evita ambiguidade e imports.
        // Só a conexão/autenticação SMTP inicial e a resolução de config/fila propagam até
        // aqui; falhas por-alvo já são tratadas e registradas dentro do CampaignDispatchService.
        internal static string ClassificarFalhaDisparo(Exception ex) => ex switch
        {
            MailKit.Security.AuthenticationException =>
                "AUTENTICAÇÃO SMTP — usuário/senha do tenant inválidos ou expirados",
            MailKit.Security.SslHandshakeException =>
                "TLS/SSL — falha no handshake StartTls (porta ou certificado do servidor SMTP)",
            MailKit.Net.Smtp.SmtpProtocolException =>
                "PROTOCOLO SMTP — erro no diálogo/handshake com o servidor",
            MailKit.Net.Smtp.SmtpCommandException =>
                "COMANDO SMTP — servidor rejeitou um comando (remetente/relay/porta)",
            System.Net.Sockets.SocketException =>
                "CONEXÃO — host/porta SMTP inacessível (rede/firewall)",
            InvalidOperationException =>
                "CONFIGURAÇÃO/FILA — SMTP não configurado para o tenant, ou template/alvos ausentes",
            _ =>
                "NÃO CLASSIFICADA — ver stack trace",
        };

        // (2) FINALIZAÇÃO: encerra automaticamente a janela de coleta das campanhas
        // "Em Andamento" cuja Data de Encerramento da Coleta (DataFim) já passou,
        // transicionando-as para "Finalizada". Campanhas sem DataFim (null) permanecem
        // ativas indefinidamente até serem encerradas por outra via.
        private async Task FinalizarEncerradasAsync(AppDbContext context, DateTime agora, CancellationToken stoppingToken)
        {
            var aFinalizar = await context.Campaigns
                .IgnoreQueryFilters()
                .Where(c => c.Status == CampaignStatus.EmAndamento && c.DataFim != null && c.DataFim <= agora)
                .ToListAsync(stoppingToken);

            if (aFinalizar.Count == 0)
                return;

            foreach (var campanha in aFinalizar)
                campanha.Status = CampaignStatus.Finalizada;

            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("{Total} campanha(s) finalizada(s) (janela de coleta encerrada).", aFinalizar.Count);
        }
    }
}
