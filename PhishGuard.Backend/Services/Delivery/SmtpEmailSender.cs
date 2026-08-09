using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;
using Polly.Retry;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services.Delivery
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly ISmtpClientFactory _smtpClientFactory;
        private readonly ISmtpCredentialProtector _credentialProtector;
        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly SecureSocketOptions _secureSocketOptions;
        private readonly Func<int, TimeSpan> _backoffProvider;
        private readonly TimeSpan _operationTimeout;

        public EmailProviderType ProviderType => EmailProviderType.Smtp;

        public SmtpEmailSender(
            ISmtpClientFactory smtpClientFactory,
            ISmtpCredentialProtector credentialProtector,
            IConfiguration configuration,
            ILogger<SmtpEmailSender> logger)
            : this(smtpClientFactory, credentialProtector, configuration, logger, BackoffExponencialComJitter)
        {
        }

        public SmtpEmailSender(
            ISmtpClientFactory smtpClientFactory,
            ISmtpCredentialProtector credentialProtector,
            IConfiguration configuration,
            ILogger<SmtpEmailSender> logger,
            Func<int, TimeSpan> backoffProvider)
        {
            _smtpClientFactory = smtpClientFactory;
            _credentialProtector = credentialProtector;
            _logger = logger;
            _secureSocketOptions = ResolverSecureSocket(configuration);
            _backoffProvider = backoffProvider;
            var timeoutSeconds = configuration.GetValue<int?>("AppSettings:SmtpOperationTimeoutSeconds") ?? 15;
            _operationTimeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 5, 60));
        }

        private static SecureSocketOptions ResolverSecureSocket(IConfiguration configuration)
        {
            var raw = configuration["AppSettings:SmtpSecureSocketOptions"];
            return Enum.TryParse<SecureSocketOptions>(raw, ignoreCase: true, out var opt)
                ? opt
                : SecureSocketOptions.StartTls;
        }

        private static TimeSpan BackoffExponencialComJitter(int tentativa)
            => TimeSpan.FromSeconds(Math.Pow(2, tentativa))
               + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));

        private AsyncRetryPolicy BuildSmtpRetryPolicy()
        {
            const int maxTentativas = 3;

            return Policy
                .Handle<Exception>(ex => ex is not OperationCanceledException
                    and not SmtpOperationalException
                    and not MailKit.Security.AuthenticationException)
                .WaitAndRetryAsync(
                    retryCount: maxTentativas,
                    sleepDurationProvider: _backoffProvider,
                    onRetry: (ex, delay, tentativa, _) =>
                        _logger.LogWarning(ex,
                            "Falha SMTP transitória (tentativa {Tentativa}/{Max}). Novo retry em {Delay:g}.",
                            tentativa, maxTentativas, delay));
        }

        public async Task<EmailSendResult> SendAsync(
            OutboundEmailMessage outbound,
            SmtpConfig config,
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            try
            {
                var senhaSmtp = UnprotectPassword(config);
                using var client = _smtpClientFactory.Create();
                var retryPolicy = BuildSmtpRetryPolicy();

                async Task GarantirConexaoAsync(CancellationToken ct)
                {
                    if (!client.IsConnected)
                        await ExecuteWithTimeoutAsync(
                            token => client.ConnectAsync(config.Host, config.Porta, _secureSocketOptions, token), ct);
                    if (!client.IsAuthenticated)
                        await ExecuteWithTimeoutAsync(
                            token => client.AuthenticateAsync(config.Usuario, senhaSmtp, token), ct);
                }

                await retryPolicy.ExecuteAsync(GarantirConexaoAsync, cancellationToken);

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(outbound.FromName, config.Usuario));
                mimeMessage.To.Add(new MailboxAddress(outbound.ToName, outbound.ToEmail));
                mimeMessage.Subject = outbound.Subject;
                mimeMessage.Headers.Add("X-PhishGuard-Idempotency-Key", idempotencyKey);

                var builder = new BodyBuilder { HtmlBody = outbound.HtmlBody };
                foreach (var inline in outbound.InlineAttachments)
                {
                    var linked = builder.LinkedResources.Add(inline.FileName, inline.Bytes, ContentType.Parse(inline.MediaType));
                    linked.ContentId = inline.ContentId;
                }
                mimeMessage.Body = builder.ToMessageBody();

                await retryPolicy.ExecuteAsync(async ct =>
                {
                    await GarantirConexaoAsync(ct);
                    await ExecuteWithTimeoutAsync(token => client.SendAsync(mimeMessage, token), ct);
                }, cancellationToken);

                try
                {
                    await ExecuteWithTimeoutAsync(token => client.DisconnectAsync(true, token), cancellationToken);
                }
                catch
                {
                    // Ignora erro no disconnect apos envio com sucesso
                }

                return EmailSendResult.Ok();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erro no envio via SmtpEmailSender para {ToEmail}", outbound.ToEmail);
                var falha = SmtpOperationalPolicy.Classify(ex);
                return EmailSendResult.Fail(falha.Code, falha.Message);
            }
        }

        public async Task<EmailProviderTestResult> TestAsync(
            SmtpConfig config,
            string destinationEmail,
            CancellationToken cancellationToken)
        {
            try
            {
                var senha = UnprotectPassword(config);
                using var client = _smtpClientFactory.Create();

                await ExecuteWithTimeoutAsync(
                    token => client.ConnectAsync(config.Host, config.Porta, _secureSocketOptions, token), cancellationToken);
                await ExecuteWithTimeoutAsync(
                    token => client.AuthenticateAsync(config.Usuario, senha, token), cancellationToken);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("PhishGuard (Teste)", config.Usuario));
                message.To.Add(new MailboxAddress(destinationEmail, destinationEmail));
                message.Subject = "Teste de Conexão SMTP - PhishGuard";
                message.Body = new TextPart("plain")
                {
                    Text = $"Teste de envio do PhishGuard. A conexão com o servidor SMTP {config.Host} foi concluída com sucesso."
                };

                await ExecuteWithTimeoutAsync(token => client.SendAsync(message, token), cancellationToken);
                await ExecuteWithTimeoutAsync(token => client.DisconnectAsync(true, token), cancellationToken);

                return new EmailProviderTestResult
                {
                    Success = true,
                    Message = $"Conexão e envio de teste realizados com sucesso via SMTP para {destinationEmail}!"
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var falha = SmtpOperationalPolicy.Classify(ex);
                return new EmailProviderTestResult
                {
                    Success = false,
                    ErrorCode = falha.Code,
                    Message = falha.Message
                };
            }
        }

        private string UnprotectPassword(SmtpConfig config)
            => _credentialProtector is IEmailSecretProtector contextual
                ? contextual.UnprotectSecret(
                    config.TenantId,
                    EmailProviderType.Smtp,
                    EmailSecretType.SmtpPassword,
                    config.Senha)
                : _credentialProtector.Unprotect(config.Senha);

        private async Task ExecuteWithTimeoutAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_operationTimeout);

            try
            {
                await operation(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"A operação SMTP excedeu o limite de {_operationTimeout.TotalSeconds:0} segundos.");
            }
        }
    }
}
