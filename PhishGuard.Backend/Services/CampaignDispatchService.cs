using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;
using Polly.Retry;
using PhishGuard.Backend.Content;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services
{
    public interface ICampaignDispatchService
    {
        /// <summary>
        /// Realiza o disparo em lote dos e-mails de uma campanha e move seu status
        /// para "Em Andamento". O SMTP é resolvido pelo TenantId da PRÓPRIA campanha
        /// (não pelo ITenantProvider), para funcionar tanto no contexto de requisição
        /// quanto no worker de agendamento em segundo plano (sem HttpContext).
        /// </summary>
        Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default);
    }

    public class CampaignDispatchService : ICampaignDispatchService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CampaignDispatchService> _logger;
        private readonly ISmtpCredentialProtector _senhaProtector;
        private readonly ISmtpClientFactory _smtpClientFactory;

        // Seams de tempo (injetáveis) para manter o comportamento de PRODUÇÃO idêntico e,
        // ao mesmo tempo, permitir que os testes zerem esperas: o backoff do retry e a
        // pausa de throttle entre envios. Em produção o ctor público injeta os defaults.
        private readonly Func<int, TimeSpan> _backoffProvider;
        private readonly TimeSpan _throttleEntreEnvios;

        private const string BaseTrackingUrl = "http://localhost:5000/api/tracking";

        public CampaignDispatchService(
            AppDbContext context,
            ILogger<CampaignDispatchService> logger,
            ISmtpCredentialProtector senhaProtector,
            ISmtpClientFactory smtpClientFactory)
            : this(context, logger, senhaProtector, smtpClientFactory,
                   BackoffExponencialComJitter, TimeSpan.FromSeconds(1))
        {
        }

        // Ctor INTERNO (visível aos testes via InternalsVisibleTo). Não é usado pela DI —
        // o container só enxerga o construtor público — então não há ambiguidade de resolução.
        internal CampaignDispatchService(
            AppDbContext context,
            ILogger<CampaignDispatchService> logger,
            ISmtpCredentialProtector senhaProtector,
            ISmtpClientFactory smtpClientFactory,
            Func<int, TimeSpan> backoffProvider,
            TimeSpan throttleEntreEnvios)
        {
            _context = context;
            _logger = logger;
            _senhaProtector = senhaProtector;
            _smtpClientFactory = smtpClientFactory;
            _backoffProvider = backoffProvider;
            _throttleEntreEnvios = throttleEntreEnvios;
        }

        // Backoff exponencial 2s→4s→8s + jitter de até 1s (descorrelaciona reconexões).
        private static TimeSpan BackoffExponencialComJitter(int tentativa)
            => TimeSpan.FromSeconds(Math.Pow(2, tentativa))
               + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));

        public async Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            // Garante que Template e Targets estejam carregados (o worker já os inclui;
            // este fallback cobre chamadas que passem a campanha sem navegações).
            if (campaign.Template == null)
                await _context.Entry(campaign).Reference(c => c.Template).LoadAsync(cancellationToken);
            if (campaign.Targets == null || !campaign.Targets.Any())
                await _context.Entry(campaign).Collection(c => c.Targets).LoadAsync(cancellationToken);

            if (campaign.Template == null)
                throw new InvalidOperationException("Template de e-mail da campanha não encontrado.");
            if (campaign.Targets == null || !campaign.Targets.Any())
                throw new InvalidOperationException("A campanha não possui alvos selecionados.");

            // SMTP é por-tenant. IgnoreQueryFilters + filtro explícito pelo TenantId da
            // campanha para funcionar sem contexto de tenant (worker em segundo plano).
            var smtpConfig = await _context.SmtpConfigs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.TenantId == campaign.TenantId, cancellationToken);

            if (smtpConfig == null)
                throw new InvalidOperationException("Configuração SMTP não encontrada para o tenant da campanha.");

            // IDEMPOTÊNCIA: quem já recebeu (log de Envio) é pulado. Carrega o conjunto de
            // alvos já enviados desta campanha ANTES do loop — assim uma retomada após
            // queda do worker/container não reenvia para quem já recebeu.
            var jaEnviados = (await _context.SimulationsLogs
                    .IgnoreQueryFilters()
                    .Where(l => l.CampaignId == campaign.Id && l.Acao == SimulationActions.Envio)
                    .Select(l => l.TargetId)
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            var pendentes = campaign.Targets.Where(t => !jaEnviados.Contains(t.Id)).ToList();
            if (pendentes.Count == 0)
            {
                _logger.LogInformation(
                    "Campanha {CampaignId}: todos os {Total} alvos já haviam sido enviados; nada a fazer.",
                    campaign.Id, campaign.Targets.Count);
                campaign.Status = CampaignStatus.EmAndamento;
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            // Senha decifrada apenas no momento do uso (proteção em repouso).
            var senhaSmtp = _senhaProtector.Unprotect(smtpConfig.Senha);

            using var client = _smtpClientFactory.Create();

            // RESILIÊNCIA (Polly): toda operação SMTP (conectar/autenticar/enviar) roda sob
            // retry com backoff exponencial 2s→4s→8s + jitter de até 1s. O jitter evita o
            // "thundering herd" (vários workers/alvos reconectando no mesmo instante e
            // sobrecarregando o servidor). OperationCanceledException NÃO é retentada —
            // shutdown/cancelamento deve propagar imediatamente.
            var smtpRetryPolicy = BuildSmtpRetryPolicy();

            // Estabelece (ou restabelece) a sessão SMTP. Idempotente: um SendAsync que derruba
            // o socket pode ser seguido por uma reconexão transparente antes do próximo envio.
            async Task GarantirConexaoAsync(CancellationToken ct)
            {
                if (!client.IsConnected)
                    await client.ConnectAsync(smtpConfig.Host, smtpConfig.Porta, SecureSocketOptions.StartTls, ct);
                if (!client.IsAuthenticated)
                    await client.AuthenticateAsync(smtpConfig.Usuario, senhaSmtp, ct);
            }

            // Conexão inicial sob retry. Se falhar DEFINITIVAMENTE, a exceção propaga: sem
            // sessão SMTP não há o que disparar, e o caller (worker) trata por-campanha.
            await smtpRetryPolicy.ExecuteAsync(GarantirConexaoAsync, cancellationToken);

            // Persiste apenas o IDENTIFICADOR da isca em CorpoHtml; resolve para o HTML real.
            var corpoBase = OfficialBaitCatalog.ResolveHtml(campaign.Template.CorpoHtml);

            foreach (var target in pendentes)
            {
                // Resiliência: uma falha definitiva num alvo (e-mail inválido, indisponibilidade
                // do SMTP após todos os retries) não pode derrubar o disparo inteiro. Registra
                // o alvo como "Falha" e segue para o próximo destinatário.
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(campaign.Template.RemetenteNome, campaign.Template.RemetenteEmail));
                    message.To.Add(new MailboxAddress(target.Nome, target.Email));
                    message.Subject = campaign.Template.Assunto;

                    var linkClique = $"{BaseTrackingUrl}/click/{campaign.Id}/{target.Id}";
                    var linkPixel = $"{BaseTrackingUrl}/open/{campaign.Id}/{target.Id}";

                    var corpoPersonalizado = corpoBase
                        .Replace("{{NOME}}", target.Nome)
                        .Replace("{{LINK_PHISHING}}", linkClique)
                        .Replace("{{LINK}}", linkClique);
                    corpoPersonalizado += $"<img src='{linkPixel}' width='1' height='1' style='display:none;' />";

                    message.Body = new BodyBuilder { HtmlBody = corpoPersonalizado }.ToMessageBody();

                    // Envio sob retry. Antes de cada tentativa reforça a sessão SMTP (um envio
                    // que falhou pode ter derrubado o socket) e então envia.
                    await smtpRetryPolicy.ExecuteAsync(async ct =>
                    {
                        await GarantirConexaoAsync(ct);
                        await client.SendAsync(message, ct);
                    }, cancellationToken);

                    // Marca o Envio IMEDIATAMENTE após o sucesso e persiste, para que uma
                    // queda logo em seguida não faça o alvo ser reenviado na retomada.
                    _context.SimulationsLogs.Add(new SimulationLog
                    {
                        Id = Guid.NewGuid(),
                        TenantId = campaign.TenantId,
                        CampaignId = campaign.Id,
                        TargetId = target.Id,
                        Acao = SimulationActions.Envio,
                        DataHora = DateTime.UtcNow,
                        IpOrigem = "SISTEMA"
                    });
                    await _context.SaveChangesAsync(cancellationToken);

                    // THROTTLING: pausa para não ser banido por spam pelo provedor SMTP.
                    if (_throttleEntreEnvios > TimeSpan.Zero)
                        await Task.Delay(_throttleEntreEnvios, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Falha DEFINITIVA (retries esgotados) para ESTE alvo. Loga e persiste um
                    // log "Falha" para rastreabilidade, sem interromper o restante do lote.
                    _logger.LogError(ex,
                        "Falha definitiva ao enviar e-mail para {Email} na campanha {CampaignId} após todos os retries.",
                        target.Email, campaign.Id);

                    _context.SimulationsLogs.Add(new SimulationLog
                    {
                        Id = Guid.NewGuid(),
                        TenantId = campaign.TenantId,
                        CampaignId = campaign.Id,
                        TargetId = target.Id,
                        Acao = SimulationActions.Falha,
                        DataHora = DateTime.UtcNow,
                        IpOrigem = "SISTEMA"
                    });

                    // O SaveChanges do log de Falha é isolado: um erro ao persistir o próprio
                    // log não pode, por sua vez, derrubar o processamento dos demais alvos.
                    try
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception logEx) when (logEx is not OperationCanceledException)
                    {
                        _logger.LogError(logEx,
                            "Não foi possível persistir o log de Falha do alvo {TargetId} (campanha {CampaignId}).",
                            target.Id, campaign.Id);
                    }
                }
            }

            await client.DisconnectAsync(true, cancellationToken);

            // Lote 100% processado: sai de "Processando" para "Em Andamento" (coleta).
            campaign.Status = CampaignStatus.EmAndamento;
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Política de retry para operações SMTP: 3 tentativas, backoff exponencial
        /// (2s → 4s → 8s) + jitter aleatório de até 1s por tentativa. O jitter descorrelaciona
        /// reconexões simultâneas, evitando picos de carga contra o servidor SMTP.
        /// <see cref="OperationCanceledException"/> é deixada propagar (shutdown/cancelamento).
        /// </summary>
        private AsyncRetryPolicy BuildSmtpRetryPolicy()
        {
            const int maxTentativas = 3;

            return Policy
                .Handle<Exception>(ex => ex is not OperationCanceledException)
                .WaitAndRetryAsync(
                    retryCount: maxTentativas,
                    sleepDurationProvider: _backoffProvider, // produção: 2/4/8s + jitter; testes: zero
                    onRetry: (ex, delay, tentativa, _) =>
                        _logger.LogWarning(ex,
                            "Falha SMTP transitória (tentativa {Tentativa}/{Max}). Novo retry em {Delay:g}.",
                            tentativa, maxTentativas, delay));
        }
    }
}
