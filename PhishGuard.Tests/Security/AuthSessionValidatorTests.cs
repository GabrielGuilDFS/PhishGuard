using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;

namespace PhishGuard.Tests.Security;

public class AuthSessionValidatorTests
{
    private sealed class EmptyTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
        public Guid GetCurrentTenantId() => Guid.Empty;
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    [Fact]
    public async Task ValidaAntesDoHttpContextUser_MasMantemEscopoCompletoERevogacao()
    {
        await using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            new EmptyTenantProvider());
        var now = new DateTimeOffset(2026, 8, 5, 12, 0, 0, TimeSpan.Zero);
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(), NomeEmpresa = "Tenant A", Cnpj = "12345678000199",
            Ativo = true, CriadoEm = now.UtcDateTime
        };
        var admin = new Administrador
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id, Nome = "Admin",
            Email = "admin@example.com", PasswordHash = "hash"
        };
        var session = new AuthSession
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id, AdministratorId = admin.Id,
            RefreshTokenHash = "HASH", CreatedAtUtc = now.UtcDateTime,
            ExpiresAtUtc = now.AddDays(1).UtcDateTime
        };
        context.AddRange(tenant, admin, session);
        await context.SaveChangesAsync();
        var validator = new AuthSessionValidator(context, new FixedTimeProvider(now));

        Assert.True(await validator.IsActiveAsync(admin.Id, tenant.Id, session.Id));
        Assert.False(await validator.IsActiveAsync(admin.Id, Guid.NewGuid(), session.Id));
        Assert.False(await validator.IsActiveAsync(Guid.NewGuid(), tenant.Id, session.Id));

        session.RevokedAtUtc = now.UtcDateTime;
        await context.SaveChangesAsync();
        Assert.False(await validator.IsActiveAsync(admin.Id, tenant.Id, session.Id));
    }
}
