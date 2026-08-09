using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MimeKit;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services.Delivery
{
    public sealed class ProviderApiEmailSender : IEmailSender
    {
        private const int MaxRetries = 3;
        private const string SesPath = "/v2/email/outbound-emails";

        private readonly HttpClient _httpClient;
        private readonly IEmailSecretProtector _secretProtector;
        private readonly ILogger<ProviderApiEmailSender> _logger;
        private readonly Func<int, TimeSpan> _backoffProvider;
        private readonly Func<DateTimeOffset> _utcNow;

        public EmailProviderType ProviderType => EmailProviderType.ProviderApi;

        public ProviderApiEmailSender(
            HttpClient httpClient,
            IEmailSecretProtector secretProtector,
            ILogger<ProviderApiEmailSender> logger)
            : this(httpClient, secretProtector, logger, BackoffWithJitter, () => DateTimeOffset.UtcNow)
        {
        }

        internal ProviderApiEmailSender(
            HttpClient httpClient,
            IEmailSecretProtector secretProtector,
            ILogger<ProviderApiEmailSender> logger,
            Func<int, TimeSpan> backoffProvider,
            Func<DateTimeOffset> utcNow)
        {
            _httpClient = httpClient;
            _secretProtector = secretProtector;
            _logger = logger;
            _backoffProvider = backoffProvider;
            _utcNow = utcNow;
        }

        public async Task<EmailSendResult> SendAsync(
            OutboundEmailMessage message,
            SmtpConfig configuration,
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(configuration.ApiProvider))
                return EmailSendResult.Fail("EMAIL_PROVIDER_UNSUPPORTED", "O provedor de e-mail selecionado não é suportado.");

            string apiSecret;
            try
            {
                apiSecret = _secretProtector.UnprotectSecret(
                    configuration.TenantId,
                    EmailProviderType.ProviderApi,
                    EmailSecretType.ApiKey,
                    configuration.EncryptedApiKey);
            }
            catch (SmtpOperationalException ex)
            {
                return EmailSendResult.Fail(ex.Code, ex.Message);
            }

            if (string.IsNullOrWhiteSpace(apiSecret))
                return EmailSendResult.Fail("API_KEY_REQUIRED", "A credencial do provedor de e-mail não está configurada.");
            if (apiSecret.Any(char.IsControl))
                return EmailSendResult.Fail("API_KEY_INVALID", "A credencial do provedor contém caracteres inválidos.");

            for (var attempt = 0; attempt <= MaxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using var request = BuildRequest(
                        configuration,
                        message,
                        apiSecret,
                        idempotencyKey,
                        _utcNow());
                    using var response = await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);

                    if (response.IsSuccessStatusCode)
                        return EmailSendResult.Ok(await ReadProviderMessageIdAsync(
                            configuration.ApiProvider, response, cancellationToken));

                    if (attempt < MaxRetries && IsTransient(response.StatusCode))
                    {
                        await DelayBeforeRetryAsync(attempt + 1, cancellationToken);
                        continue;
                    }

                    var providerError = await ReadSafeProviderErrorAsync(response, cancellationToken);
                    _logger.LogWarning(
                        "Provedor de e-mail {Provider} recusou o envio com HTTP {StatusCode}. C\u00f3digo do provedor: {ProviderErrorCode}. Request ID: {ProviderRequestId}.",
                        configuration.ApiProvider,
                        (int)response.StatusCode,
                        providerError.Code,
                        providerError.RequestId);
                    return FailureForStatus(response.StatusCode);
                }
                catch (HttpRequestException ex) when (attempt < MaxRetries)
                {
                    _logger.LogWarning(
                        "Falha transitória de rede no provedor {Provider}; tentativa {Attempt}/{MaxAttempts}.",
                        configuration.ApiProvider,
                        attempt + 1,
                        MaxRetries + 1);
                    _logger.LogDebug(ex, "Detalhe da falha transitória do provedor de e-mail.");
                    await DelayBeforeRetryAsync(attempt + 1, cancellationToken);
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested && attempt < MaxRetries)
                {
                    _logger.LogWarning(
                        "Timeout transitório no provedor {Provider}; tentativa {Attempt}/{MaxAttempts}.",
                        configuration.ApiProvider,
                        attempt + 1,
                        MaxRetries + 1);
                    _logger.LogDebug(ex, "Detalhe do timeout transitório do provedor de e-mail.");
                    await DelayBeforeRetryAsync(attempt + 1, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Falha definitiva ao comunicar com o provedor {Provider}.", configuration.ApiProvider);
                    return EmailSendResult.Fail(
                        "PROVIDER_API_EXCEPTION",
                        "Não foi possível comunicar com a API do provedor de e-mail.");
                }
            }

            return EmailSendResult.Fail(
                "PROVIDER_API_UNAVAILABLE",
                "A API do provedor de e-mail permaneceu indisponível após as retentativas.");
        }

        public async Task<EmailProviderTestResult> TestAsync(
            SmtpConfig configuration,
            string destinationEmail,
            CancellationToken cancellationToken)
        {
            var testMessage = new OutboundEmailMessage
            {
                FromName = string.IsNullOrWhiteSpace(configuration.SenderName)
                    ? "PhishGuard (Teste API)"
                    : configuration.SenderName,
                FromEmail = configuration.SenderEmail,
                ToName = destinationEmail,
                ToEmail = destinationEmail,
                Subject = "Teste de Conexão API HTTPS - PhishGuard",
                HtmlBody = "<p>Teste de envio via API HTTPS do PhishGuard concluído com sucesso.</p>"
            };

            var result = await SendAsync(
                testMessage,
                configuration,
                $"test/{Guid.NewGuid():N}",
                cancellationToken);

            return new EmailProviderTestResult
            {
                Success = result.Success,
                ErrorCode = result.ErrorCode,
                Message = result.Success
                    ? configuration.ApiProvider == ApiProviderName.MailtrapSandbox
                        ? $"Mensagem de teste capturada no Mailtrap Sandbox para o destinatário simulado {destinationEmail}."
                        : $"Teste concluído com sucesso via {configuration.ApiProvider} para {destinationEmail}."
                    : result.ErrorMessage ?? "Falha ao enviar e-mail de teste via API."
            };
        }

        private HttpRequestMessage BuildRequest(
            SmtpConfig configuration,
            OutboundEmailMessage message,
            string apiSecret,
            string idempotencyKey,
            DateTimeOffset now)
        {
            var senderEmail = configuration.SenderEmail;
            var senderName = string.IsNullOrWhiteSpace(configuration.SenderName)
                ? message.FromName
                : configuration.SenderName;

            if (configuration.ApiProvider == ApiProviderName.AwsSes)
                return BuildAwsSesRequest(configuration, message, senderEmail, senderName, apiSecret, idempotencyKey, now);

            if (configuration.ApiProvider == ApiProviderName.MailtrapSandbox)
            {
                if (!EmailProviderPolicy.TryParseMailtrapSandboxId(
                    configuration.ApiAccountIdentifier,
                    out var sandboxId))
                    throw new InvalidOperationException("Mailtrap Sandbox ID inválido.");

                var sandboxPayload = BuildProviderPayload(
                    configuration.ApiProvider,
                    message,
                    senderEmail,
                    senderName,
                    idempotencyKey);
                var sandboxRequest = JsonRequest(
                    $"https://sandbox.api.mailtrap.io/api/send/{sandboxId}",
                    sandboxPayload);
                sandboxRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiSecret);
                return sandboxRequest;
            }

            var endpoint = configuration.ApiProvider switch
            {
                ApiProviderName.Postmark => "https://api.postmarkapp.com/email",
                ApiProviderName.Brevo => "https://api.brevo.com/v3/smtp/email",
                ApiProviderName.SendGrid => "https://api.sendgrid.com/v3/mail/send",
                _ => throw new InvalidOperationException("Provedor HTTP fora da allow-list.")
            };

            var payload = BuildProviderPayload(
                configuration.ApiProvider,
                message,
                senderEmail,
                senderName,
                idempotencyKey);
            var request = JsonRequest(endpoint, payload);

            switch (configuration.ApiProvider)
            {
                case ApiProviderName.Postmark:
                    request.Headers.Add("X-Postmark-Server-Token", apiSecret);
                    break;
                case ApiProviderName.Brevo:
                    request.Headers.Add("api-key", apiSecret);
                    break;
                case ApiProviderName.SendGrid:
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiSecret);
                    break;
            }

            return request;
        }

        private static object BuildProviderPayload(
            ApiProviderName provider,
            OutboundEmailMessage message,
            string senderEmail,
            string senderName,
            string idempotencyKey)
            => provider switch
            {
                ApiProviderName.Postmark => new
                {
                    From = FormatMailbox(senderName, senderEmail),
                    To = FormatMailbox(message.ToName, message.ToEmail),
                    message.Subject,
                    HtmlBody = message.HtmlBody,
                    Headers = new[] { new { Name = "X-PhishGuard-Idempotency-Key", Value = idempotencyKey } },
                    Metadata = new Dictionary<string, string> { ["phishguard-idempotency-key"] = idempotencyKey },
                    Attachments = message.InlineAttachments.Select(attachment => new
                    {
                        Name = attachment.FileName,
                        Content = Convert.ToBase64String(attachment.Bytes),
                        ContentType = attachment.MediaType,
                        ContentID = $"cid:{attachment.ContentId}"
                    })
                },
                ApiProviderName.Brevo => new
                {
                    sender = new { name = senderName, email = senderEmail },
                    to = new[] { new { email = message.ToEmail, name = message.ToName } },
                    message.Subject,
                    htmlContent = InlineAsDataUris(message),
                    headers = new Dictionary<string, string> { ["Idempotency-Key"] = idempotencyKey }
                },
                ApiProviderName.SendGrid => new
                {
                    personalizations = new[]
                    {
                        new
                        {
                            to = new[] { new { email = message.ToEmail, name = message.ToName } },
                            subject = message.Subject,
                            custom_args = new Dictionary<string, string>
                            {
                                ["phishguard_idempotency_key"] = idempotencyKey
                            }
                        }
                    },
                    from = new { email = senderEmail, name = senderName },
                    content = new[] { new { type = "text/html", value = message.HtmlBody } },
                    attachments = message.InlineAttachments.Select(attachment => new
                    {
                        content = Convert.ToBase64String(attachment.Bytes),
                        type = attachment.MediaType,
                        filename = attachment.FileName,
                        disposition = "inline",
                        content_id = attachment.ContentId
                    })
                },
                ApiProviderName.MailtrapSandbox => new
                {
                    from = new { email = senderEmail, name = senderName },
                    to = new[] { new { email = message.ToEmail, name = message.ToName } },
                    subject = message.Subject,
                    html = message.HtmlBody,
                    attachments = message.InlineAttachments.Select(attachment => new
                    {
                        content = Convert.ToBase64String(attachment.Bytes),
                        type = attachment.MediaType,
                        filename = attachment.FileName,
                        disposition = "inline",
                        content_id = attachment.ContentId
                    })
                },
                _ => throw new InvalidOperationException("Provedor HTTP fora da allow-list.")
            };

        private HttpRequestMessage BuildAwsSesRequest(
            SmtpConfig configuration,
            OutboundEmailMessage message,
            string senderEmail,
            string senderName,
            string secretAccessKey,
            string idempotencyKey,
            DateTimeOffset now)
        {
            var region = configuration.ApiRegion.Trim().ToLowerInvariant();
            if (!EmailProviderPolicy.IsSupportedAwsRegion(region))
                throw new InvalidOperationException("Região AWS SES fora da allow-list.");
            if (string.IsNullOrWhiteSpace(configuration.ApiAccountIdentifier))
                throw new InvalidOperationException("AWS Access Key ID não configurado.");
            if (!configuration.ApiAccountIdentifier.All(char.IsLetterOrDigit))
                throw new InvalidOperationException("AWS Access Key ID inválido.");

            var mime = BuildMimeMessage(message, senderEmail, senderName, idempotencyKey);
            using var stream = new MemoryStream();
            mime.WriteTo(stream);

            var payload = JsonSerializer.Serialize(new
            {
                FromEmailAddress = FormatMailbox(senderName, senderEmail),
                Destination = new { ToAddresses = new[] { message.ToEmail } },
                Content = new { Raw = new { Data = Convert.ToBase64String(stream.ToArray()) } },
                EmailTags = new[] { new { Name = "phishguard-idempotency", Value = Sha256Hex(idempotencyKey)[..32] } }
            });

            var endpoint = new Uri($"https://email.{region}.amazonaws.com{SesPath}");
            var request = JsonRequest(endpoint.ToString(), payload, alreadySerialized: true);
            SignAwsRequest(
                request,
                payload,
                configuration.ApiAccountIdentifier.Trim(),
                secretAccessKey,
                region,
                now);
            return request;
        }

        private static MimeMessage BuildMimeMessage(
            OutboundEmailMessage outbound,
            string senderEmail,
            string senderName,
            string idempotencyKey)
        {
            var mime = new MimeMessage();
            mime.From.Add(new MailboxAddress(senderName, senderEmail));
            mime.To.Add(new MailboxAddress(outbound.ToName, outbound.ToEmail));
            mime.Subject = outbound.Subject;
            mime.Headers.Add("X-PhishGuard-Idempotency-Key", idempotencyKey);

            var body = new BodyBuilder { HtmlBody = outbound.HtmlBody };
            foreach (var attachment in outbound.InlineAttachments)
            {
                var linked = body.LinkedResources.Add(
                    attachment.FileName,
                    attachment.Bytes,
                    ContentType.Parse(attachment.MediaType));
                linked.ContentId = attachment.ContentId;
            }

            mime.Body = body.ToMessageBody();
            return mime;
        }

        private static void SignAwsRequest(
            HttpRequestMessage request,
            string payload,
            string accessKeyId,
            string secretAccessKey,
            string region,
            DateTimeOffset now)
        {
            const string service = "ses";
            var utc = now.UtcDateTime;
            var amzDate = utc.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
            var dateStamp = utc.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var payloadHash = Sha256Hex(payload);
            var host = request.RequestUri!.Host;
            const string signedHeaders = "content-type;host;x-amz-content-sha256;x-amz-date";
            var canonicalHeaders =
                $"content-type:application/json\nhost:{host}\nx-amz-content-sha256:{payloadHash}\nx-amz-date:{amzDate}\n";
            var canonicalRequest =
                $"POST\n{SesPath}\n\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";
            var credentialScope = $"{dateStamp}/{region}/{service}/aws4_request";
            var stringToSign =
                $"AWS4-HMAC-SHA256\n{amzDate}\n{credentialScope}\n{Sha256Hex(canonicalRequest)}";

            var dateKey = Hmac(Encoding.UTF8.GetBytes("AWS4" + secretAccessKey), dateStamp);
            var regionKey = Hmac(dateKey, region);
            var serviceKey = Hmac(regionKey, service);
            var signingKey = Hmac(serviceKey, "aws4_request");
            var signature = Convert.ToHexString(Hmac(signingKey, stringToSign)).ToLowerInvariant();

            request.Headers.TryAddWithoutValidation("X-Amz-Date", amzDate);
            request.Headers.TryAddWithoutValidation("X-Amz-Content-Sha256", payloadHash);
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                $"AWS4-HMAC-SHA256 Credential={accessKeyId}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}");
        }

        private static HttpRequestMessage JsonRequest(string endpoint, object payload, bool alreadySerialized = false)
        {
            var json = alreadySerialized ? (string)payload : JsonSerializer.Serialize(payload);
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
        }

        private static string InlineAsDataUris(OutboundEmailMessage message)
        {
            var html = message.HtmlBody;
            foreach (var attachment in message.InlineAttachments)
            {
                html = html.Replace(
                    $"cid:{attachment.ContentId}",
                    $"data:{attachment.MediaType};base64,{Convert.ToBase64String(attachment.Bytes)}",
                    StringComparison.OrdinalIgnoreCase);
            }
            return html;
        }

        private static string FormatMailbox(string name, string email)
            => string.IsNullOrWhiteSpace(name) ? email : $"{name} <{email}>";

        private static bool IsTransient(HttpStatusCode statusCode)
            => statusCode == HttpStatusCode.RequestTimeout
                || statusCode == HttpStatusCode.TooManyRequests
                || (int)statusCode >= 500;

        private static EmailSendResult FailureForStatus(HttpStatusCode statusCode)
            => statusCode switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => EmailSendResult.Fail(
                    "PROVIDER_AUTH_FAILED",
                    "O provedor recusou a credencial ou o remetente configurado."),
                HttpStatusCode.TooManyRequests => EmailSendResult.Fail(
                    "PROVIDER_RATE_LIMITED",
                    "O provedor limitou temporariamente os envios. Tente novamente mais tarde."),
                _ when (int)statusCode >= 500 => EmailSendResult.Fail(
                    "PROVIDER_API_UNAVAILABLE",
                    "A API do provedor de e-mail está temporariamente indisponível."),
                _ => EmailSendResult.Fail(
                    $"PROVIDER_HTTP_{(int)statusCode}",
                    "O provedor rejeitou a mensagem. Verifique remetente, domínio e configuração da conta.")
            };

        private static async Task<string?> ReadProviderMessageIdAsync(
            ApiProviderName provider,
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (provider == ApiProviderName.SendGrid
                && response.Headers.TryGetValues("X-Message-Id", out var values))
                return values.FirstOrDefault();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                using var document = JsonDocument.Parse(json);
                var propertyName = provider switch
                {
                    ApiProviderName.Postmark => "MessageID",
                    ApiProviderName.Brevo => "messageId",
                    ApiProviderName.AwsSes => "MessageId",
                    _ => string.Empty
                };
                if (provider == ApiProviderName.MailtrapSandbox
                    && document.RootElement.TryGetProperty("message_ids", out var messageIds)
                    && messageIds.ValueKind == JsonValueKind.Array
                    && messageIds.GetArrayLength() > 0)
                    return messageIds[0].GetString();

                return propertyName.Length > 0
                    && document.RootElement.TryGetProperty(propertyName, out var value)
                    ? value.GetString()
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static async Task<(string Code, string? RequestId)> ReadSafeProviderErrorAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            var requestId = response.Headers.TryGetValues("x-amzn-requestid", out var requestIds)
                ? requestIds.FirstOrDefault()
                : null;
            var headerErrorCode = response.Headers.TryGetValues("x-amzn-errortype", out var errorTypes)
                ? errorTypes.FirstOrDefault()
                : null;

            if (!string.IsNullOrWhiteSpace(headerErrorCode))
                return (headerErrorCode, requestId);

            // C\u00f3digos s\u00e3o metadados p\u00fablicos do provedor; a mensagem textual pode conter
            // detalhes operacionais e, por isso, n\u00e3o \u00e9 persistida nem exibida ao usu\u00e1rio.
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
                return ("UNKNOWN", requestId);

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                var code = root.TryGetProperty("Code", out var codeProperty)
                    ? codeProperty.GetString()
                    : root.TryGetProperty("code", out var lowerCodeProperty)
                        ? lowerCodeProperty.GetString()
                    : root.TryGetProperty("__type", out var typeProperty)
                        ? typeProperty.GetString()
                        : null;
                return (string.IsNullOrWhiteSpace(code) ? "UNKNOWN" : code, requestId);
            }
            catch (JsonException)
            {
                return ("UNKNOWN", requestId);
            }
        }

        private async Task DelayBeforeRetryAsync(int retryNumber, CancellationToken cancellationToken)
        {
            var delay = _backoffProvider(retryNumber);
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken);
        }

        private static TimeSpan BackoffWithJitter(int retryNumber)
            => TimeSpan.FromSeconds(Math.Pow(2, retryNumber))
                + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));

        private static string Sha256Hex(string value)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

        private static byte[] Hmac(byte[] key, string value)
            => HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(value));
    }
}
