using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;
using Polly.Retry;
using PhishGuard.Backend.Content;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Utilities;

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

        // URL PÚBLICA base do endpoint de rastreamento, gravada nos links dos e-mails.
        // Resolvida da configuração (AppSettings:PublicApiBaseUrl); default localhost p/ dev.
        // Precisa ser um endereço acessível PELO ALVO — senão o clique nunca chega ao backend.
        private readonly string _baseTrackingUrl;

        public CampaignDispatchService(
            AppDbContext context,
            ILogger<CampaignDispatchService> logger,
            ISmtpCredentialProtector senhaProtector,
            ISmtpClientFactory smtpClientFactory,
            IConfiguration configuration)
            : this(context, logger, senhaProtector, smtpClientFactory,
                   BackoffExponencialComJitter, ResolverThrottle(configuration))
        {
            var apiBase = configuration["AppSettings:PublicApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(apiBase))
                apiBase = "http://localhost:5000";
            _baseTrackingUrl = $"{apiBase}/api/tracking";
        }

        // Pausa entre envios (anti-spam) configurável em AppSettings:SmtpThrottleMs. Default
        // ENXUTO de 300ms (antes 1000ms): para lotes pequenos, a soma dessas pausas era o que
        // mantinha a campanha "Processando" por muito tempo antes de virar "Em Andamento".
        // 300ms ≈ 3 envios/s — abaixo dos limites de provedores como o Gmail, e ~3x mais rápido.
        private static TimeSpan ResolverThrottle(IConfiguration configuration)
        {
            var ms = configuration.GetValue<int?>("AppSettings:SmtpThrottleMs") ?? 300;
            return TimeSpan.FromMilliseconds(Math.Max(0, ms));
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
            // Default para o caminho de testes (ctor interno). O ctor público de produção
            // sobrescreve com a URL pública resolvida da configuração.
            _baseTrackingUrl = "http://localhost:5000/api/tracking";
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
                    // MÁSCARA DE REMETENTE: o NOME de exibição é a marca parodiada do cenário
                    // (ex.: "Microsft 365", "NetsFlix"), mas o ENDEREÇO real é SEMPRE a conta SMTP
                    // autenticada do tenant (ex.: phishguard.tcc@gmail.com). Provedores como o
                    // Gmail reescrevem/recusam um From que não seja a conta autenticada — usar o
                    // RemetenteEmail fictício (no-reply@microsft365.com) quebraria a entrega. Assim
                    // o cabeçalho fica: "Microsft 365" <phishguard.tcc@gmail.com>.
                    message.From.Add(new MailboxAddress(campaign.Template.RemetenteNome, smtpConfig.Usuario));
                    message.To.Add(new MailboxAddress(target.Nome, target.Email));
                    message.Subject = campaign.Template.Assunto;

                    var linkClique = $"{_baseTrackingUrl}/click/{campaign.Id}/{target.Id}";
                    var linkPixel = $"{_baseTrackingUrl}/open/{campaign.Id}/{target.Id}";

                    // TIMEZONE: todos os horários dos e-mails são carimbados no fuso de Brasília
                    // (America/Sao_Paulo, UTC-3) a partir de UtcNow — ver HorarioBrasilia. Sem
                    // isso, no contêiner (relógio UTC) os textos saíam 3h adiantados.

                    // Expiração DINÂMICA do link (isca bho MAX): data/hora de ENVIO + 2h,
                    // formatada no padrão "MMM dd, yyyy às hh:mm tt" (ex.: "Jul 22, 2026 às 04:30 PM").
                    var expiraEm = HorarioBrasilia.Converter(DateTime.UtcNow.AddHours(2));
                    var dataExpiracao = expiraEm.ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"))
                        + " às " + expiraEm.ToString("hh:mm tt", CultureInfo.GetCultureInfo("en-US"));

                    // Data/hora do "acesso detectado" (isca Microsft 365): momento
                    // do ENVIO, padrão pt-BR "dd/MM/yyyy às HH:mm (BRT)".
                    var agora = HorarioBrasilia.Agora();
                    var dataAcesso = agora.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                        + " às " + agora.ToString("HH:mm", CultureInfo.InvariantCulture) + " (BRT)";

                    var corpoPersonalizado = corpoBase
                        .Replace("{{NOME}}", target.Nome)
                        .Replace("{{LINK_PHISHING}}", linkClique)
                        .Replace("{{LINK}}", linkClique)
                        .Replace("{{DATA_EXPIRACAO}}", dataExpiracao)
                        .Replace("{{DATA_ACESSO}}", dataAcesso);
                    corpoPersonalizado += $"<img src='{linkPixel}' width='1' height='1' style='display:none;' />";

                    // LOGOS INLINE (CID): o Gmail não renderiza SVG/data-URI, mas exibe
                    // anexos inline por padrão. Para cada token de logo referenciado por
                    // "cid:<token>" no corpo, anexa o PNG embutido como LinkedResource com o
                    // Content-ID correspondente. Só anexa o que o corpo realmente referencia.
                    var builder = new BodyBuilder { HtmlBody = corpoPersonalizado };
                    foreach (var token in EmailLogoCatalog.Tokens)
                    {
                        if (corpoPersonalizado.Contains($"cid:{token}", StringComparison.OrdinalIgnoreCase)
                            && EmailLogoCatalog.TryGetLogo(token, out var logoFile, out var logoBytes))
                        {
                            var linked = builder.LinkedResources.Add(logoFile, logoBytes, new ContentType("image", "png"));
                            linked.ContentId = token;
                        }
                    }

                    message.Body = builder.ToMessageBody();

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
