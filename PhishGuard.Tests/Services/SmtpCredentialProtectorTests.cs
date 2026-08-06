using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Services;

namespace PhishGuard.Tests.Services;

public sealed class SmtpCredentialProtectorTests
{
    private sealed class NoTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
        public Guid GetCurrentTenantId() => Guid.Empty;
    }

    [Fact]
    public void KeyRingNoBanco_PermiteDecifrarDepoisDeRecriarOContainerDeDi()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"phishguard-dp-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={databasePath};Pooling=False";

        try
        {
            string encrypted;
            using (var firstProvider = BuildProvider(connectionString))
            {
                using var scope = firstProvider.CreateScope();
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
                encrypted = scope.ServiceProvider.GetRequiredService<ISmtpCredentialProtector>()
                    .Protect("senha-de-aplicativo");
            }

            using (var secondProvider = BuildProvider(connectionString))
            using (var scope = secondProvider.CreateScope())
            {
                var plaintext = scope.ServiceProvider.GetRequiredService<ISmtpCredentialProtector>()
                    .Unprotect(encrypted);
                Assert.Equal("senha-de-aplicativo", plaintext);
            }
        }
        finally
        {
            if (File.Exists(databasePath)) File.Delete(databasePath);
        }
    }

    private static ServiceProvider BuildProvider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITenantProvider, NoTenantProvider>();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddDataProtection()
            .SetApplicationName("PhishGuard.Tests")
            .PersistKeysToDbContext<AppDbContext>();
        services.AddSingleton<ISmtpCredentialProtector, SmtpCredentialProtector>();
        return services.BuildServiceProvider();
    }
}
