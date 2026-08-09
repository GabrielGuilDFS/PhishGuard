using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Security;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Tests.Controllers;

public sealed class SmtpConfigControllerTests
{
    private sealed class TenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid GetTenantId() => tenantId;
        public Guid GetCurrentTenantId() => tenantId;
    }

    private sealed class Protector : ISmtpCredentialProtector
    {
        public string Protect(string? plaintext) => $"protected:{plaintext}";
        public string Unprotect(string? stored) => stored?.Replace("protected:", "") ?? string.Empty;
    }

    private sealed class NeverUsedFactory : ISmtpClientFactory
    {
        public ISmtpClient Create() => throw new InvalidOperationException("SMTP não deveria ser aberto neste teste.");
    }

    private sealed class UnreadableProtector : ISmtpCredentialProtector
    {
        public string Protect(string? plaintext) => plaintext ?? string.Empty;
        public string Unprotect(string? stored) => throw new SmtpOperationalException(
            SmtpOperationalPolicy.CredentialUnreadableCode,
            "Credencial ilegível.");
    }

    private sealed class RecordingSmtpClient : ISmtpClient
    {
        public bool IsConnected { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string? Host { get; private set; }
        public int Port { get; private set; }
        public string? User { get; private set; }
        public string? Password { get; private set; }
        public string? Destination { get; private set; }

        public Task ConnectAsync(string host, int port, SecureSocketOptions options, CancellationToken cancellationToken)
        {
            Host = host;
            Port = port;
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {
            User = userName;
            Password = password;
            IsAuthenticated = true;
            return Task.CompletedTask;
        }

        public Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
        {
            Destination = message.To.Mailboxes.Single().Address;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(bool quit, CancellationToken cancellationToken)
        {
            IsConnected = false;
            IsAuthenticated = false;
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }

    private sealed class RecordingFactory(RecordingSmtpClient client) : ISmtpClientFactory
    {
        public ISmtpClient Create() => client;
    }

    private static (AppDbContext Context, SmtpConfigController Controller) Create(
        Guid tenantId,
        Dictionary<string, string?>? settings = null,
        ISmtpCredentialProtector? protector = null,
        ISmtpClientFactory? factory = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var provider = new TenantProvider(tenantId);
        var context = new AppDbContext(options, provider);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings ?? new Dictionary<string, string?>())
            .Build();
        return (context, new SmtpConfigController(
            context,
            provider,
            protector ?? new Protector(),
            factory ?? new NeverUsedFactory(),
            configuration));
    }

    [Fact]
    public async Task Upsert_NovaConfiguracaoSemSenha_RetornaBadRequest()
    {
        var tenantId = Guid.NewGuid();
        var (_, controller) = Create(tenantId);

        var result = await controller.Upsert(new SmtpConfigDto
        {
            Host = "smtp.example.com", Porta = 587, Usuario = "sender@example.com"
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upsert_SalvaNoTenantECifraSenha()
    {
        var tenantId = Guid.NewGuid();
        var (context, controller) = Create(tenantId);

        var result = await controller.Upsert(new SmtpConfigDto
        {
            Host = " smtp.example.com ", Porta = 587,
            Usuario = " sender@example.com ", Senha = "app-password"
        }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var saved = await context.SmtpConfigs.IgnoreQueryFilters().SingleAsync();
        Assert.Equal(tenantId, saved.TenantId);
        Assert.Equal("smtp.example.com", saved.Host);
        Assert.Equal("sender@example.com", saved.Usuario);
        Assert.Equal("protected:app-password", saved.Senha);
    }

    [Fact]
    public async Task Upsert_AtualizacaoComSenhaVazia_PreservaCredencialExistente()
    {
        var tenantId = Guid.NewGuid();
        var (context, controller) = Create(tenantId);
        context.SmtpConfigs.Add(new SmtpConfig
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Host = "old", Porta = 587,
            Usuario = "old@example.com", Senha = "protected:original"
        });
        await context.SaveChangesAsync();

        var result = await controller.Upsert(new SmtpConfigDto
        {
            Host = "new.example.com", Porta = 465, Usuario = "new@example.com", Senha = ""
        }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var saved = await context.SmtpConfigs.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("protected:original", saved.Senha);
        Assert.Equal("new.example.com", saved.Host);
    }

    [Fact]
    public async Task UpsertApi_EGet_NuncaDevolveApiKey()
    {
        var tenantId = Guid.NewGuid();
        var (context, controller) = Create(tenantId);

        var result = await controller.Upsert(new SmtpConfigDto
        {
            ProviderType = EmailProviderType.ProviderApi,
            ApiProvider = ApiProviderName.SendGrid,
            SenderEmail = "sender@example.com",
            SenderName = "Equipe",
            ApiKey = "sg-secret"
        }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var saved = await context.SmtpConfigs.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("protected:sg-secret", saved.EncryptedApiKey);

        var get = await controller.Get();
        var dto = Assert.IsType<SmtpConfigDto>(get.Value);
        Assert.Equal(string.Empty, dto.ApiKey);
        Assert.True(dto.ApiKeyConfigured);
    }

    [Fact]
    public async Task UpsertAwsSes_ExigeAccessKeyIdERegiaoDaAllowList()
    {
        var tenantId = Guid.NewGuid();
        var (_, controller) = Create(tenantId);

        var result = await controller.Upsert(new SmtpConfigDto
        {
            ProviderType = EmailProviderType.ProviderApi,
            ApiProvider = ApiProviderName.AwsSes,
            SenderEmail = "sender@example.com",
            ApiKey = "secret",
            ApiAccountIdentifier = "",
            ApiRegion = "http://169.254.169.254"
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpsertMailtrapSandbox_ExigeSandboxIdNumericoPositivo()
    {
        var tenantId = Guid.NewGuid();
        var (_, controller) = Create(tenantId);

        var result = await controller.Upsert(new SmtpConfigDto
        {
            ProviderType = EmailProviderType.ProviderApi,
            ApiProvider = ApiProviderName.MailtrapSandbox,
            SenderEmail = "sender@example.com",
            ApiKey = "mailtrap-token",
            ApiAccountIdentifier = "https://sandbox.api.mailtrap.io/api/send/4015"
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpsertApi_AoTrocarProvedor_ExigeNovaCredencial()
    {
        var tenantId = Guid.NewGuid();
        var (context, controller) = Create(tenantId);
        context.SmtpConfigs.Add(new SmtpConfig
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderType = EmailProviderType.ProviderApi,
            ApiProvider = ApiProviderName.Brevo,
            SenderEmail = "sender@example.com",
            EncryptedApiKey = "protected:brevo-secret"
        });
        await context.SaveChangesAsync();

        var result = await controller.Upsert(new SmtpConfigDto
        {
            ProviderType = EmailProviderType.ProviderApi,
            ApiProvider = ApiProviderName.MailtrapSandbox,
            SenderEmail = "sender@example.com",
            ApiKey = "",
            ApiAccountIdentifier = "4015"
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Testar_TransporteDesabilitado_Retorna503SemAbrirSocket()
    {
        var tenantId = Guid.NewGuid();
        var (context, controller) = Create(tenantId, new Dictionary<string, string?>
        {
            ["AppSettings:SmtpTransportEnabled"] = "false",
            ["AppSettings:SmtpTransportDisabledReason"] = "Bloqueado pelo ambiente."
        });
        context.SmtpConfigs.Add(new SmtpConfig
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Host = "smtp.example.com", Porta = 587,
            Usuario = "sender@example.com", Senha = "protected:secret"
        });
        await context.SaveChangesAsync();

        var result = await controller.TestarConexao(
            new TestarSmtpDto { EmailDestino = "target@example.com" }, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
        var saved = await context.SmtpConfigs.IgnoreQueryFilters().SingleAsync();
        Assert.False(saved.UltimoTesteSucesso);
        Assert.Equal(SmtpOperationalPolicy.TransportUnavailableCode, saved.UltimoErroCodigo);
    }

    [Fact]
    public async Task GetStatus_CredencialIlegivel_NaoDeclaraSmtpConfigurado()
    {
        var tenantId = Guid.NewGuid();
        var (context, controller) = Create(
            tenantId,
            protector: new UnreadableProtector());
        context.SmtpConfigs.Add(new SmtpConfig
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Host = "smtp.example.com", Porta = 587,
            Usuario = "sender@example.com", Senha = "CfDJ-chave-antiga"
        });
        await context.SaveChangesAsync();

        var action = await controller.GetStatus(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var status = Assert.IsType<SmtpStatusDto>(ok.Value);
        Assert.False(status.Configurado);
        Assert.Equal(SmtpOperationalPolicy.CredentialUnreadableCode, status.UltimoErroCodigo);
    }

    [Fact]
    public async Task Testar_UsaExclusivamenteAConfiguracaoPersistida()
    {
        var tenantId = Guid.NewGuid();
        var smtpClient = new RecordingSmtpClient();
        var (context, controller) = Create(
            tenantId,
            factory: new RecordingFactory(smtpClient));
        context.SmtpConfigs.Add(new SmtpConfig
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Host = "smtp.saved.example", Porta = 587,
            Usuario = "saved@example.com", Senha = "protected:saved-password"
        });
        await context.SaveChangesAsync();

        var result = await controller.TestarConexao(
            new TestarSmtpDto { EmailDestino = "target@example.com" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("smtp.saved.example", smtpClient.Host);
        Assert.Equal(587, smtpClient.Port);
        Assert.Equal("saved@example.com", smtpClient.User);
        Assert.Equal("saved-password", smtpClient.Password);
        Assert.Equal("target@example.com", smtpClient.Destination);
        var saved = await context.SmtpConfigs.IgnoreQueryFilters().SingleAsync();
        Assert.True(saved.UltimoTesteSucesso);
    }
}
