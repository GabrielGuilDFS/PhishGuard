using System;
using System.Collections.Generic;
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
using PhishGuard.Backend.Security;

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
        private readonly ITrackingTokenService _trackingTokenService;

        // Seams de tempo (injetáveis) para manter o comportamento de PRODUÇÃO idêntico e,
        // ao mesmo tempo, permitir que os testes zerem esperas: o backoff do retry e a
        // pausa de throttle entre envios. Em produção o ctor público injeta os defaults.
        private readonly Func<int, TimeSpan> _backoffProvider;
        private readonly TimeSpan _throttleEntreEnvios;

        // URL PÚBLICA base do endpoint de rastreamento, gravada nos links dos e-mails.
        // Resolvida da configuração (AppSettings:PublicApiBaseUrl); default localhost p/ dev.
        // Precisa ser um endereço acessível PELO ALVO — senão o clique nunca chega ao backend.
        private readonly string _baseTrackingUrl;

        // Modo de segurança do socket SMTP. Default de PRODUÇÃO: StartTls (upgrade TLS na
        // 587). Configurável (AppSettings:SmtpSecureSocketOptions) só para HOMOLOGAÇÃO —
        // ex.: "StartTlsWhenAvailable" para falar com o Mailpit (SMTP em texto puro na 1025),
        // sem afetar o disparo real (Gmail continua anunciando STARTTLS e sendo criptografado).
        private readonly SecureSocketOptions _secureSocketOptions;

        // ALLOWLIST DE DESTINO (§2.1d): quando NÃO vazia, o disparo só envia para e-mails
        // cujo domínio esteja na lista (AppSettings:OutboundEmailAllowedDomains). Rede de
        // segurança de HOMOLOGAÇÃO: garante que nenhum e-mail de teste escape para uma caixa
        // real, MESMO que o SMTP seja trocado por engano. Vazia (produção) = sem restrição.
        private readonly IReadOnlyCollection<string> _dominiosPermitidos;
        private readonly bool _smtpTransportEnabled;
        private readonly string? _smtpTransportDisabledReason;
        private readonly TimeSpan _smtpOperationTimeout;

        public CampaignDispatchService(
            AppDbContext context,
            ILogger<CampaignDispatchService> logger,
            ISmtpCredentialProtector senhaProtector,
            ISmtpClientFactory smtpClientFactory,
            ITrackingTokenService trackingTokenService,
            IConfiguration configuration)
            : this(context, logger, senhaProtector, smtpClientFactory,
                   BackoffExponencialComJitter, ResolverThrottle(configuration))
        {
            var apiBase = configuration["AppSettings:PublicApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(apiBase))
                apiBase = "http://localhost:5000";
            _baseTrackingUrl = $"{apiBase}/api/tracking";
            _trackingTokenService = trackingTokenService;
            _secureSocketOptions = ResolverSecureSocket(configuration);
            _dominiosPermitidos = ResolverDominiosPermitidos(configuration);
            _smtpTransportEnabled = SmtpOperationalPolicy.IsTransportEnabled(configuration);
            _smtpTransportDisabledReason = SmtpOperationalPolicy.GetTransportDisabledReason(configuration);
            var timeoutSeconds = configuration.GetValue<int?>("AppSettings:SmtpOperationTimeoutSeconds") ?? 15;
            _smtpOperationTimeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 5, 60));
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
            _trackingTokenService = new TrackingTokenService(
                TimeProvider.System,
                "segredo-de-testes-para-tracking-com-pelo-menos-sessenta-e-quatro-bytes-123456789",
                TimeSpan.FromDays(90));
            _backoffProvider = backoffProvider;
            _throttleEntreEnvios = throttleEntreEnvios;
            // Defaults para o caminho de testes (ctor interno). O ctor público de produção
            // sobrescreve com os valores resolvidos da configuração.
            _baseTrackingUrl = "http://localhost:5000/api/tracking";
            _secureSocketOptions = SecureSocketOptions.StartTls;
            _dominiosPermitidos = Array.Empty<string>();
            _smtpTransportEnabled = true;
            _smtpTransportDisabledReason = null;
            _smtpOperationTimeout = TimeSpan.FromSeconds(15);
        }

        // Resolve o modo de socket SMTP da config. Aceita os nomes do enum SecureSocketOptions
        // (StartTls, StartTlsWhenAvailable, SslOnConnect, Auto, None). Default: StartTls.
        private static SecureSocketOptions ResolverSecureSocket(IConfiguration configuration)
        {
            var raw = configuration["AppSettings:SmtpSecureSocketOptions"];
            return Enum.TryParse<SecureSocketOptions>(raw, ignoreCase: true, out var opt)
                ? opt
                : SecureSocketOptions.StartTls;
        }

        // Lê a allowlist de domínios de destino (separados por ',' ou ';'). Normaliza para
        // minúsculas e SEM o '@' inicial. Vazio/ausente => sem restrição (produção).
        private static IReadOnlyCollection<string> ResolverDominiosPermitidos(IConfiguration configuration)
        {
            var raw = configuration["AppSettings:OutboundEmailAllowedDomains"];
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<string>();

            return raw
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(d => d.TrimStart('@').ToLowerInvariant())
                .Where(d => d.Length > 0)
                .Distinct()
                .ToArray();
        }

        // Aplica a allowlist a um endereço de destino. Lista vazia => tudo permitido.
        private bool DestinoPermitido(string email)
        {
            if (_dominiosPermitidos.Count == 0)
                return true;

            var at = email.LastIndexOf('@');
            if (at < 0 || at == email.Length - 1)
                return false; // e-mail sem domínio: bloqueia por segurança.

            var dominio = email[(at + 1)..].ToLowerInvariant();
            return _dominiosPermitidos.Contains(dominio);
        }

        // Backoff exponencial 2s→4s→8s + jitter de até 1s (descorrelaciona reconexões).
        private static TimeSpan BackoffExponencialComJitter(int tentativa)
            => TimeSpan.FromSeconds(Math.Pow(2, tentativa))
               + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));

        public async Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            if (!_smtpTransportEnabled)
                throw new SmtpOperationalException(
                    SmtpOperationalPolicy.TransportUnavailableCode,
                    _smtpTransportDisabledReason ?? "O transporte SMTP está indisponível neste ambiente.");

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

            if (!SmtpOperationalPolicy.IsConfigured(smtpConfig))
                throw new SmtpOperationalException(
                    SmtpOperationalPolicy.NotConfiguredCode,
                    "Configuração SMTP não encontrada ou incompleta para o tenant da campanha.");

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
            var senhaSmtp = _senhaProtector.Unprotect(smtpConfig!.Senha);

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
                    await ExecutarComTimeoutAsync(
                        token => client.ConnectAsync(smtpConfig.Host, smtpConfig.Porta, _secureSocketOptions, token),
                        ct);
                if (!client.IsAuthenticated)
                    await ExecutarComTimeoutAsync(
                        token => client.AuthenticateAsync(smtpConfig.Usuario, senhaSmtp, token),
                        ct);
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
                // ALLOWLIST (§2.1d): em homologação, recusa qualquer destino fora dos
                // domínios permitidos ANTES de tocar no SMTP — nenhum e-mail de teste
                // vaza para uma caixa real. Registra "Falha" (rastreável) e segue o lote.
                if (!DestinoPermitido(target.Email))
                {
                    _logger.LogWarning(
                        "Alvo {Email} bloqueado pela allowlist de domínios de destino (campanha {CampaignId}). E-mail NÃO enviado.",
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
                    await _context.SaveChangesAsync(cancellationToken);
                    continue;
                }

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

                    var trackingToken = Uri.EscapeDataString(_trackingTokenService.Create(campaign.Id, target.Id));
                    var linkClique = $"{_baseTrackingUrl}/click/{campaign.Id}/{target.Id}?k={trackingToken}";
                    var linkPixel = $"{_baseTrackingUrl}/open/{campaign.Id}/{target.Id}?k={trackingToken}";

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
                    // PIXEL DE ABERTURA: NÃO use display:none/visibility:hidden. Muitos clientes
                    // deixam de baixar imagens removidas do layout e, nesse caso, o endpoint nunca
                    // é chamado. O pixel transparente continua imperceptível e acessível, mas é um
                    // elemento renderizável de 1x1 que o cliente pode requisitar.
                    corpoPersonalizado += $"<img src=\"{linkPixel}\" width=\"1\" height=\"1\" style=\"display:block !important; width:1px !important; height:1px !important; max-width:1px !important; max-height:1px !important; border:0 !important; margin:0 !important; padding:0 !important; opacity:0.01 !important; overflow:hidden !important;\" alt=\"\" aria-hidden=\"true\" />";

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
                        await ExecutarComTimeoutAsync(token => client.SendAsync(message, token), ct);
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

            try
            {
                await ExecutarComTimeoutAsync(token => client.DisconnectAsync(true, token), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // O lote já foi persistido por alvo. Uma falha ao encerrar a sessão não
                // transforma entregas concluídas em falha nem provoca reprocessamento.
                _logger.LogWarning(ex,
                    "Não foi possível encerrar normalmente a sessão SMTP da campanha {CampaignId}.",
                    campaign.Id);
            }

            // Lote 100% processado: sai de "Processando" para "Em Andamento" (coleta).
            campaign.Status = CampaignStatus.EmAndamento;
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Política de retry para operações SMTP: 3 retries (4 tentativas no total), backoff exponencial
        /// (2s → 4s → 8s) + jitter aleatório de até 1s por tentativa. O jitter descorrelaciona
        /// reconexões simultâneas, evitando picos de carga contra o servidor SMTP.
        /// <see cref="OperationCanceledException"/> é deixada propagar (shutdown/cancelamento).
        /// </summary>
        private AsyncRetryPolicy BuildSmtpRetryPolicy()
        {
            const int maxTentativas = 3;

            return Policy
                // Credencial/configuração inválida não é transitória: repetir apenas aumenta
                // o tempo em "Processando" e pode acelerar bloqueios no provedor.
                .Handle<Exception>(ex => ex is not OperationCanceledException
                    and not SmtpOperationalException
                    and not MailKit.Security.AuthenticationException)
                .WaitAndRetryAsync(
                    retryCount: maxTentativas,
                    sleepDurationProvider: _backoffProvider, // produção: 2/4/8s + jitter; testes: zero
                    onRetry: (ex, delay, tentativa, _) =>
                        _logger.LogWarning(ex,
                            "Falha SMTP transitória (tentativa {Tentativa}/{Max}). Novo retry em {Delay:g}.",
                            tentativa, maxTentativas, delay));
        }

        private async Task ExecutarComTimeoutAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_smtpOperationTimeout);

            try
            {
                await operation(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"A operação SMTP excedeu o limite de {_smtpOperationTimeout.TotalSeconds:0} segundos.");
            }
        }
    }
}
