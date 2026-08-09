using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Tests.Services.Delivery;

public sealed class ProviderApiEmailSenderTests
{
    private sealed class Protector : IEmailSecretProtector
    {
        public string Protect(string? plaintext) => plaintext ?? string.Empty;
        public string Unprotect(string? stored) => stored ?? string.Empty;
        public string ProtectSecret(Guid tenantId, EmailProviderType providerType, EmailSecretType secretType, string? plaintext)
            => plaintext ?? string.Empty;
        public string UnprotectSecret(Guid tenantId, EmailProviderType providerType, EmailSecretType secretType, string? stored)
            => stored ?? string.Empty;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public RecordingHandler(HttpStatusCode statusCode, string responseBody = "")
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        public Uri? RequestUri { get; private set; }
        public string? Body { get; private set; }
        public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
        public int Calls { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;
            RequestUri = request.RequestUri;
            Body = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            foreach (var header in request.Headers)
                Headers[header.Key] = string.Join(",", header.Value);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class TransientThenSuccessHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(new HttpResponseMessage(
                Calls < 3 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Accepted));
        }
    }

    private static OutboundEmailMessage Message() => new()
    {
        FromName = "Equipe",
        FromEmail = "sender@example.com",
        ToName = "Alvo",
        ToEmail = "target@example.com",
        Subject = "Assunto",
        HtmlBody = "<p>Olá</p><img src=\"cid:logo\">",
        InlineAttachments =
        {
            new InlineAttachment
            {
                ContentId = "logo",
                FileName = "logo.png",
                MediaType = "image/png",
                Bytes = new byte[] { 1, 2, 3, 4 }
            }
        }
    };

    private static SmtpConfig Config(ApiProviderName provider) => new()
    {
        TenantId = Guid.NewGuid(),
        ProviderType = EmailProviderType.ProviderApi,
        ApiProvider = provider,
        SenderEmail = "sender@example.com",
        SenderName = "Equipe",
        EncryptedApiKey = "secret"
    };

    private static ProviderApiEmailSender Sender(RecordingHandler handler)
        => new(
            new HttpClient(handler),
            new Protector(),
            NullLogger<ProviderApiEmailSender>.Instance,
            _ => TimeSpan.Zero,
            () => new DateTimeOffset(2026, 8, 8, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task SendGrid_UsaEndpointFixoEMapeiaCidInline()
    {
        var handler = new RecordingHandler(HttpStatusCode.Accepted);
        var result = await Sender(handler).SendAsync(
            Message(), Config(ApiProviderName.SendGrid), "campaign/c/target/t", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("https://api.sendgrid.com/v3/mail/send", handler.RequestUri!.ToString());
        using var json = JsonDocument.Parse(handler.Body!);
        var attachment = json.RootElement.GetProperty("attachments")[0];
        Assert.Equal("inline", attachment.GetProperty("disposition").GetString());
        Assert.Equal("logo", attachment.GetProperty("content_id").GetString());
        Assert.Equal("AQIDBA==", attachment.GetProperty("content").GetString());
    }

    [Fact]
    public async Task Postmark_UsaContentIdNoContratoDoProvedor()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK, "{\"MessageID\":\"pm-1\"}");
        var result = await Sender(handler).SendAsync(
            Message(), Config(ApiProviderName.Postmark), "campaign/c/target/t", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("pm-1", result.ProviderMessageId);
        Assert.Equal("https://api.postmarkapp.com/email", handler.RequestUri!.ToString());
        using var json = JsonDocument.Parse(handler.Body!);
        Assert.Equal("cid:logo", json.RootElement.GetProperty("Attachments")[0].GetProperty("ContentID").GetString());
        Assert.Equal("secret", handler.Headers["X-Postmark-Server-Token"]);
    }

    [Fact]
    public async Task Brevo_ConverteCidParaDataUriSemCriarUrlArbitraria()
    {
        var handler = new RecordingHandler(HttpStatusCode.Created, "{\"messageId\":\"brevo-1\"}");
        var result = await Sender(handler).SendAsync(
            Message(), Config(ApiProviderName.Brevo), "campaign/c/target/t", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("https://api.brevo.com/v3/smtp/email", handler.RequestUri!.ToString());
        using var json = JsonDocument.Parse(handler.Body!);
        Assert.Contains("data:image/png;base64,AQIDBA==", json.RootElement.GetProperty("htmlContent").GetString());
        Assert.Equal("campaign/c/target/t", json.RootElement.GetProperty("headers").GetProperty("Idempotency-Key").GetString());
    }

    [Fact]
    public async Task AwsSes_UsaSigV4EEnvelopeRawComCid()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK, "{\"MessageId\":\"ses-1\"}");
        var config = Config(ApiProviderName.AwsSes);
        config.ApiAccountIdentifier = "AKIATEST";
        config.ApiRegion = "sa-east-1";

        var result = await Sender(handler).SendAsync(
            Message(), config, "campaign/c/target/t", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("ses-1", result.ProviderMessageId);
        Assert.Equal("https://email.sa-east-1.amazonaws.com/v2/email/outbound-emails", handler.RequestUri!.ToString());
        Assert.StartsWith("AWS4-HMAC-SHA256 Credential=AKIATEST/", handler.Headers["Authorization"]);
        using var json = JsonDocument.Parse(handler.Body!);
        var raw = Convert.FromBase64String(
            json.RootElement.GetProperty("Content").GetProperty("Raw").GetProperty("Data").GetString()!);
        var mime = Encoding.UTF8.GetString(raw);
        Assert.Contains("Content-Id: <logo>", mime, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MailtrapSandbox_UsaEndpointFixo_PreservaDestinatarioECid()
    {
        var handler = new RecordingHandler(
            HttpStatusCode.OK,
            "{\"success\":true,\"message_ids\":[\"sandbox-1\"]}");
        var config = Config(ApiProviderName.MailtrapSandbox);
        config.ApiAccountIdentifier = "4015";

        var result = await Sender(handler).SendAsync(
            Message(), config, "campaign/c/target/t", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("sandbox-1", result.ProviderMessageId);
        Assert.Equal("https://sandbox.api.mailtrap.io/api/send/4015", handler.RequestUri!.ToString());
        Assert.Equal("Bearer secret", handler.Headers["Authorization"]);

        using var json = JsonDocument.Parse(handler.Body!);
        Assert.Equal("target@example.com", json.RootElement.GetProperty("to")[0].GetProperty("email").GetString());
        var attachment = json.RootElement.GetProperty("attachments")[0];
        Assert.Equal("inline", attachment.GetProperty("disposition").GetString());
        Assert.Equal("logo", attachment.GetProperty("content_id").GetString());
        Assert.Equal("AQIDBA==", attachment.GetProperty("content").GetString());
    }

    [Fact]
    public async Task ErroHttpTransitorio_RetentaComAMesmaChaveEConclui()
    {
        var handler = new TransientThenSuccessHandler();
        var sender = new ProviderApiEmailSender(
            new HttpClient(handler),
            new Protector(),
            NullLogger<ProviderApiEmailSender>.Instance,
            _ => TimeSpan.Zero,
            () => new DateTimeOffset(2026, 8, 8, 0, 0, 0, TimeSpan.Zero));

        var result = await sender.SendAsync(
            Message(), Config(ApiProviderName.SendGrid), "campaign/c/target/t", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(3, handler.Calls);
    }
}
