using Microsoft.AspNetCore.DataProtection;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Services.Delivery;
using Xunit;

namespace PhishGuard.Tests.Services;

public class SmtpOperationalPolicyTests
{
    private static EmailSecretProtector CreateProtector() =>
        new(new EphemeralDataProtectionProvider());

    [Fact]
    public void ValidateCredential_AceitaSenhaSmtpProtegidaComContextoDoTenant()
    {
        var tenantId = Guid.NewGuid();
        var protector = CreateProtector();
        var config = CreateSmtpConfig(tenantId);
        config.Senha = protector.ProtectSecret(
            tenantId,
            EmailProviderType.Smtp,
            EmailSecretType.SmtpPassword,
            "senha-e2e");

        var error = SmtpOperationalPolicy.ValidateCredential(config, protector);

        Assert.Null(error);
    }

    [Fact]
    public void ValidateCredential_RejeitaSenhaProtegidaPorOutroTenant()
    {
        var tenantId = Guid.NewGuid();
        var protector = CreateProtector();
        var config = CreateSmtpConfig(tenantId);
        config.Senha = protector.ProtectSecret(
            Guid.NewGuid(),
            EmailProviderType.Smtp,
            EmailSecretType.SmtpPassword,
            "senha-de-outro-tenant");

        var error = SmtpOperationalPolicy.ValidateCredential(config, protector);

        Assert.Equal(SmtpOperationalPolicy.CredentialUnreadableCode, error);
    }

    [Fact]
    public void ValidateCredential_AceitaApiKeyProtegidaComContextoDoTenant()
    {
        var tenantId = Guid.NewGuid();
        var protector = CreateProtector();
        var config = new SmtpConfig
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderType = EmailProviderType.ProviderApi,
            ApiProvider = ApiProviderName.MailtrapSandbox,
            ApiAccountIdentifier = "4475065",
            SenderEmail = "sender@test.io",
        };
        config.EncryptedApiKey = protector.ProtectSecret(
            tenantId,
            EmailProviderType.ProviderApi,
            EmailSecretType.ApiKey,
            "api-key-e2e");

        var error = SmtpOperationalPolicy.ValidateCredential(config, protector);

        Assert.Null(error);
    }

    [Fact]
    public void ValidateCredential_RejeitaPayloadCriptografadoCorrompido()
    {
        var protector = CreateProtector();
        var config = CreateSmtpConfig(Guid.NewGuid());
        config.Senha = "CfDJ-payload-invalido";

        var error = SmtpOperationalPolicy.ValidateCredential(config, protector);

        Assert.Equal(SmtpOperationalPolicy.CredentialUnreadableCode, error);
    }

    [Fact]
    public void ValidateCredential_PreservaCompatibilidadeComSenhaLegada()
    {
        var protector = CreateProtector();
        var config = CreateSmtpConfig(Guid.NewGuid());
        config.Senha = protector.Protect("senha-legada");

        var error = SmtpOperationalPolicy.ValidateCredential(config, protector);

        Assert.Null(error);
    }

    private static SmtpConfig CreateSmtpConfig(Guid tenantId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        ProviderType = EmailProviderType.Smtp,
        Host = "mailpit",
        Porta = 1025,
        Usuario = "phishguard@test.io",
        Senha = "placeholder",
    };
}
