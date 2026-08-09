using Microsoft.AspNetCore.DataProtection;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Tests.Security;

public sealed class EmailSecretProtectorTests
{
    [Fact]
    public void SegredoContextual_SoPodeSerAbertoNoMesmoTenantProvedorETipo()
    {
        var protector = new EmailSecretProtector(new EphemeralDataProtectionProvider());
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var encrypted = protector.ProtectSecret(
            tenantA,
            EmailProviderType.ProviderApi,
            EmailSecretType.ApiKey,
            "segredo-api");

        Assert.Equal(
            "segredo-api",
            protector.UnprotectSecret(
                tenantA,
                EmailProviderType.ProviderApi,
                EmailSecretType.ApiKey,
                encrypted));
        Assert.Throws<SmtpOperationalException>(() => protector.UnprotectSecret(
            tenantB,
            EmailProviderType.ProviderApi,
            EmailSecretType.ApiKey,
            encrypted));
        Assert.Throws<SmtpOperationalException>(() => protector.UnprotectSecret(
            tenantA,
            EmailProviderType.Smtp,
            EmailSecretType.SmtpPassword,
            encrypted));
    }
}
