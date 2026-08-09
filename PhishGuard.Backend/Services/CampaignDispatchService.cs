using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Backend.Services
{
    public interface ICampaignDispatchService
    {
        Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default);
    }

    public class CampaignDispatchService : ICampaignDispatchService
    {
        private static readonly TimeSpan DeliveryLease = TimeSpan.FromMinutes(5);

        private sealed class LegacySecretProtectorAdapter : IEmailSecretProtector
        {
            private readonly ISmtpCredentialProtector _inner;

            public LegacySecretProtectorAdapter(ISmtpCredentialProtector inner) => _inner = inner;

            public string Protect(string? plaintext) => _inner.Protect(plaintext);
            public string Unprotect(string? stored) => _inner.Unprotect(stored);
            public string ProtectSecret(Guid tenantId, EmailProviderType providerType, EmailSecretType secretType, string? plaintext)
                => _inner.Protect(plaintext);
            public string UnprotectSecret(Guid tenantId, EmailProviderType providerType, EmailSecretType secretType, string? stored)
                => _inner.Unprotect(stored);
        }

        private readonly AppDbContext _context;
        private readonly ILogger<CampaignDispatchService> _logger;
        private readonly IEmailMessageComposer _composer;
        private readonly IEmailSenderResolver _senderResolver;
        private readonly TimeSpan _throttleBetweenSends;
        private readonly IReadOnlyCollection<string> _allowedDomains;
        private readonly bool _smtpTransportEnabled;
        private readonly string? _smtpTransportDisabledReason;

        public CampaignDispatchService(
            AppDbContext context,
            ILogger<CampaignDispatchService> logger,
            ISmtpCredentialProtector credentialProtector,
            ISmtpClientFactory smtpClientFactory,
            ITrackingTokenService trackingTokenService,
            IEmailMessageComposer composer,
            IEmailSenderResolver senderResolver,
            IConfiguration configuration)
            : this(
                context,
                logger,
                composer,
                senderResolver,
                ResolverThrottle(configuration),
                ResolverDominiosPermitidos(configuration),
                SmtpOperationalPolicy.IsTransportEnabled(configuration),
                SmtpOperationalPolicy.GetTransportDisabledReason(configuration))
        {
        }

        // Compatibilidade para os testes e consumidores que ainda constroem o serviço com
        // as abstrações SMTP antigas. O caminho de produção usa o construtor acima via DI.
        public CampaignDispatchService(
            AppDbContext context,
            ILogger<CampaignDispatchService> logger,
            ISmtpCredentialProtector credentialProtector,
            ISmtpClientFactory smtpClientFactory,
            ITrackingTokenService trackingTokenService,
            IConfiguration configuration)
        {
            var contextual = credentialProtector as IEmailSecretProtector
                ?? new LegacySecretProtectorAdapter(credentialProtector);
            var composer = new EmailMessageComposer(
                trackingTokenService,
                configuration["AppSettings:PublicApiBaseUrl"] ?? "http://localhost:5000");
            var resolver = new EmailSenderResolver(new IEmailSender[]
            {
                new SmtpEmailSender(
                    smtpClientFactory,
                    credentialProtector,
                    configuration,
                    NullLogger<SmtpEmailSender>.Instance),
                new ProviderApiEmailSender(
                    new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(20) },
                    contextual,
                    NullLogger<ProviderApiEmailSender>.Instance)
            });

            _context = context;
            _logger = logger;
            _composer = composer;
            _senderResolver = resolver;
            _throttleBetweenSends = ResolverThrottle(configuration);
            _allowedDomains = ResolverDominiosPermitidos(configuration);
            _smtpTransportEnabled = SmtpOperationalPolicy.IsTransportEnabled(configuration);
            _smtpTransportDisabledReason = SmtpOperationalPolicy.GetTransportDisabledReason(configuration);
        }

        internal CampaignDispatchService(
            AppDbContext context,
            ILogger<CampaignDispatchService> logger,
            ISmtpCredentialProtector credentialProtector,
            ISmtpClientFactory smtpClientFactory,
            Func<int, TimeSpan> backoffProvider,
            TimeSpan throttleEntreEnvios)
        {
            var tracking = new TrackingTokenService(
                TimeProvider.System,
                "segredo-de-testes-para-tracking-com-pelo-menos-sessenta-e-quatro-bytes-123456789",
                TimeSpan.FromDays(90));
            var configuration = new ConfigurationBuilder().Build();
            var contextual = credentialProtector as IEmailSecretProtector
                ?? new LegacySecretProtectorAdapter(credentialProtector);

            _context = context;
            _logger = logger;
            _composer = new EmailMessageComposer(tracking, "http://localhost:5000");
            _senderResolver = new EmailSenderResolver(new IEmailSender[]
            {
                new SmtpEmailSender(
                    smtpClientFactory,
                    credentialProtector,
                    configuration,
                    NullLogger<SmtpEmailSender>.Instance,
                    backoffProvider),
                new ProviderApiEmailSender(
                    new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(20) },
                    contextual,
                    NullLogger<ProviderApiEmailSender>.Instance)
            });
            _throttleBetweenSends = throttleEntreEnvios;
            _allowedDomains = Array.Empty<string>();
            _smtpTransportEnabled = true;
            _smtpTransportDisabledReason = null;
        }

        private CampaignDispatchService(
            AppDbContext context,
            ILogger<CampaignDispatchService> logger,
            IEmailMessageComposer composer,
            IEmailSenderResolver senderResolver,
            TimeSpan throttleBetweenSends,
            IReadOnlyCollection<string> allowedDomains,
            bool smtpTransportEnabled,
            string? smtpTransportDisabledReason)
        {
            _context = context;
            _logger = logger;
            _composer = composer;
            _senderResolver = senderResolver;
            _throttleBetweenSends = throttleBetweenSends;
            _allowedDomains = allowedDomains;
            _smtpTransportEnabled = smtpTransportEnabled;
            _smtpTransportDisabledReason = smtpTransportDisabledReason;
        }

        public async Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            if (campaign.Template == null)
                await _context.Entry(campaign).Reference(item => item.Template).LoadAsync(cancellationToken);
            if (campaign.Targets == null || !campaign.Targets.Any())
                await _context.Entry(campaign).Collection(item => item.Targets).LoadAsync(cancellationToken);

            if (campaign.Template == null)
                throw new InvalidOperationException("Template de e-mail da campanha não encontrado.");
            if (campaign.Targets == null || campaign.Targets.Count == 0)
                throw new InvalidOperationException("A campanha não possui alvos selecionados.");

            var deliveryConfig = await _context.SmtpConfigs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(config => config.TenantId == campaign.TenantId, cancellationToken);

            if (!SmtpOperationalPolicy.IsConfigured(deliveryConfig))
                throw new SmtpOperationalException(
                    SmtpOperationalPolicy.NotConfiguredCode,
                    "Configuração de entrega de e-mail não encontrada ou incompleta para o tenant da campanha.");

            if (deliveryConfig!.ProviderType == EmailProviderType.Smtp && !_smtpTransportEnabled)
                throw new SmtpOperationalException(
                    SmtpOperationalPolicy.TransportUnavailableCode,
                    _smtpTransportDisabledReason
                        ?? "O transporte SMTP está indisponível neste ambiente. Configure um provedor por API HTTPS.");

            var sender = _senderResolver.Resolve(deliveryConfig);
            var sentTargetIds = (await _context.SimulationsLogs
                    .IgnoreQueryFilters()
                    .Where(log => log.CampaignId == campaign.Id && log.Acao == SimulationActions.Envio)
                    .Select(log => log.TargetId)
                    .ToListAsync(cancellationToken))
                .ToHashSet();
            var deliveries = (await _context.CampaignDeliveries
                    .IgnoreQueryFilters()
                    .Where(delivery => delivery.CampaignId == campaign.Id)
                    .ToListAsync(cancellationToken))
                .ToDictionary(delivery => delivery.TargetId);

            var senderEmail = deliveryConfig.ProviderType == EmailProviderType.Smtp
                ? deliveryConfig.Usuario
                : deliveryConfig.SenderEmail;
            var hasActiveLease = false;

            foreach (var target in campaign.Targets.Where(target => !sentTargetIds.Contains(target.Id)))
            {
                var idempotencyKey = $"campaign/{campaign.Id:N}/target/{target.Id:N}";
                deliveries.TryGetValue(target.Id, out var existingDelivery);
                var delivery = await TryClaimDeliveryAsync(
                    campaign,
                    target,
                    idempotencyKey,
                    existingDelivery,
                    cancellationToken);

                if (delivery == null)
                {
                    hasActiveLease = true;
                    continue;
                }

                if (!DestinoPermitido(target.Email))
                {
                    _logger.LogWarning(
                        "Alvo {TargetId} bloqueado pela allow-list de destino na campanha {CampaignId}.",
                        target.Id,
                        campaign.Id);
                    await MarkFailedAsync(
                        delivery,
                        campaign,
                        target,
                        "DESTINATION_NOT_ALLOWED",
                        cancellationToken);
                    continue;
                }

                try
                {
                    var message = _composer.Compose(
                        campaign,
                        target,
                        senderEmail,
                        deliveryConfig.SenderName);
                    var result = await sender.SendAsync(
                        message,
                        deliveryConfig,
                        idempotencyKey,
                        cancellationToken);

                    if (result.Success)
                    {
                        delivery.Status = CampaignDeliveryStatus.Sent;
                        delivery.ConcurrencyToken = Guid.NewGuid();
                        delivery.SentAtUtc = DateTime.UtcNow;
                        delivery.LeaseExpiresAtUtc = null;
                        delivery.ProviderMessageId = Truncate(result.ProviderMessageId, 200);
                        delivery.LastErrorCode = null;
                        await RecordLogOnceAsync(campaign, target, SimulationActions.Envio, cancellationToken);

                        if (_throttleBetweenSends > TimeSpan.Zero)
                            await Task.Delay(_throttleBetweenSends, cancellationToken);
                    }
                    else
                    {
                        _logger.LogError(
                            "Falha no transporte de e-mail do alvo {TargetId}, campanha {CampaignId}: {Code}.",
                            target.Id,
                            campaign.Id,
                            result.ErrorCode);
                        await MarkFailedAsync(
                            delivery,
                            campaign,
                            target,
                            result.ErrorCode ?? "EMAIL_SEND_FAILED",
                            cancellationToken);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(
                        ex,
                        "Falha definitiva no alvo {TargetId}, campanha {CampaignId}.",
                        target.Id,
                        campaign.Id);
                    await MarkFailedAsync(
                        delivery,
                        campaign,
                        target,
                        "EMAIL_SEND_EXCEPTION",
                        cancellationToken);
                }
            }

            if (!hasActiveLease)
                campaign.Status = CampaignStatus.EmAndamento;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<CampaignDelivery?> TryClaimDeliveryAsync(
            Campaign campaign,
            Target target,
            string idempotencyKey,
            CampaignDelivery? delivery,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var isNew = delivery == null;

            if (delivery?.Status == CampaignDeliveryStatus.Sent)
                return null;
            if (delivery?.Status == CampaignDeliveryStatus.Processing
                && delivery.LeaseExpiresAtUtc > now)
                return null;

            if (isNew)
            {
                delivery = new CampaignDelivery
                {
                    Id = Guid.NewGuid(),
                    TenantId = campaign.TenantId,
                    CampaignId = campaign.Id,
                    TargetId = target.Id,
                    IdempotencyKey = idempotencyKey
                };
                _context.CampaignDeliveries.Add(delivery);
            }

            delivery!.Status = CampaignDeliveryStatus.Processing;
            delivery.AttemptCount++;
            delivery.ConcurrencyToken = Guid.NewGuid();
            delivery.LastAttemptAtUtc = now;
            delivery.LeaseExpiresAtUtc = now.Add(DeliveryLease);
            delivery.LastErrorCode = null;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return delivery;
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.Entry(delivery).State = EntityState.Detached;
                return null;
            }
            catch (DbUpdateException) when (isNew)
            {
                _context.Entry(delivery).State = EntityState.Detached;
                var winnerExists = await _context.CampaignDeliveries
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .AnyAsync(
                        item => item.CampaignId == campaign.Id && item.TargetId == target.Id,
                        cancellationToken);
                if (winnerExists) return null;
                throw;
            }
        }

        private async Task MarkFailedAsync(
            CampaignDelivery delivery,
            Campaign campaign,
            Target target,
            string errorCode,
            CancellationToken cancellationToken)
        {
            delivery.Status = CampaignDeliveryStatus.Failed;
            delivery.ConcurrencyToken = Guid.NewGuid();
            delivery.LeaseExpiresAtUtc = null;
            delivery.LastErrorCode = Truncate(errorCode, 100);
            await RecordLogOnceAsync(campaign, target, SimulationActions.Falha, cancellationToken);
        }

        private async Task RecordLogOnceAsync(
            Campaign campaign,
            Target target,
            string action,
            CancellationToken cancellationToken)
        {
            var existing = await _context.SimulationsLogs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    log => log.CampaignId == campaign.Id
                        && log.TargetId == target.Id
                        && log.Acao == action,
                    cancellationToken);

            if (existing == null)
            {
                _context.SimulationsLogs.Add(new SimulationLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = campaign.TenantId,
                    CampaignId = campaign.Id,
                    TargetId = target.Id,
                    Acao = action,
                    DataHora = DateTime.UtcNow,
                    IpOrigem = "SISTEMA"
                });
            }
            else if (action == SimulationActions.Falha)
            {
                existing.DataHora = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private static string? Truncate(string? value, int maxLength)
            => value is null || value.Length <= maxLength ? value : value[..maxLength];

        private static TimeSpan ResolverThrottle(IConfiguration configuration)
        {
            var milliseconds = configuration.GetValue<int?>("AppSettings:SmtpThrottleMs") ?? 300;
            return TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));
        }

        private static IReadOnlyCollection<string> ResolverDominiosPermitidos(IConfiguration configuration)
        {
            var raw = configuration["AppSettings:OutboundEmailAllowedDomains"];
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();

            return raw
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(domain => domain.TrimStart('@').ToLowerInvariant())
                .Where(domain => domain.Length > 0)
                .Distinct()
                .ToArray();
        }

        private bool DestinoPermitido(string email)
        {
            if (_allowedDomains.Count == 0) return true;
            var at = email.LastIndexOf('@');
            if (at < 0 || at == email.Length - 1) return false;
            return _allowedDomains.Contains(email[(at + 1)..].ToLowerInvariant());
        }
    }
}
