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
        public DateTimeOffset Now { get; set; } = utcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public async Task ValidaAntesDoHttpContextUser_MasMantemEscopoCompletoERevogacao()
    {
        var now = new DateTimeOffset(2026, 8, 5, 12, 0, 0, TimeSpan.Zero);
        var clock = new FixedTimeProvider(now);
        using var cache = new AuthSessionCache(clock);
        await using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            new EmptyTenantProvider(), cache);
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
        var validator = new AuthSessionValidator(context, clock, cache);

        Assert.True(await validator.IsActiveAsync(admin.Id, tenant.Id, session.Id));
        Assert.True(cache.Contains(admin.Id, tenant.Id, session.Id));
        // Um acerto não necessita nem de um DbContext utilizável.
        var disposedContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().Options, new EmptyTenantProvider());
        await disposedContext.DisposeAsync();
        Assert.True(await new AuthSessionValidator(disposedContext, clock, cache)
            .IsActiveAsync(admin.Id, tenant.Id, session.Id));
        Assert.False(await validator.IsActiveAsync(admin.Id, Guid.NewGuid(), session.Id));
        Assert.False(await validator.IsActiveAsync(Guid.NewGuid(), tenant.Id, session.Id));

        session.RevokedAtUtc = now.UtcDateTime;
        await context.SaveChangesAsync();
        Assert.False(await validator.IsActiveAsync(admin.Id, tenant.Id, session.Id));

        session.RevokedAtUtc = null;
        await context.SaveChangesAsync();
        Assert.True(await validator.IsActiveAsync(admin.Id, tenant.Id, session.Id));
        tenant.Ativo = false;
        await context.SaveChangesAsync();
        Assert.False(await validator.IsActiveAsync(admin.Id, tenant.Id, session.Id));
        tenant.Ativo = true;
        await context.SaveChangesAsync();
        Assert.True(await validator.IsActiveAsync(admin.Id, tenant.Id, session.Id));
        context.Administradores.Remove(admin);
        await context.SaveChangesAsync();
        Assert.False(await validator.IsActiveAsync(admin.Id, tenant.Id, session.Id));
    }

    [Fact]
    public void Cache_ExpiraSemRenovarNoAcertoERespeitaExpiracaoDaSessao()
    {
        var clock = new FixedTimeProvider(DateTimeOffset.UtcNow);
        using var cache = new AuthSessionCache(clock);
        var admin = Guid.NewGuid(); var tenant = Guid.NewGuid(); var session = Guid.NewGuid();
        cache.Store(admin, tenant, session, clock.Now.AddHours(1).UtcDateTime, cache.Generation);
        clock.Now = clock.Now.AddSeconds(9);
        Assert.True(cache.Contains(admin, tenant, session));
        clock.Now = clock.Now.AddSeconds(1);
        Assert.False(cache.Contains(admin, tenant, session));
        cache.Store(admin, tenant, session, clock.Now.AddSeconds(2).UtcDateTime, cache.Generation);
        clock.Now = clock.Now.AddSeconds(2);
        Assert.False(cache.Contains(admin, tenant, session));
    }

    [Fact]
    public void Cache_ConsultaAnteriorARevogacaoNaoRepopulaEntrada()
    {
        using var cache = new AuthSessionCache(TimeProvider.System);
        var generation = cache.Generation;
        cache.Invalidate();
        var admin = Guid.NewGuid(); var tenant = Guid.NewGuid(); var session = Guid.NewGuid();
        cache.Store(admin, tenant, session, DateTime.UtcNow.AddHours(1), generation);
        Assert.False(cache.Contains(admin, tenant, session));
        cache.Invalidate(explicitTransaction: true);
        cache.Store(admin, tenant, session, DateTime.UtcNow.AddHours(1), cache.Generation);
        Assert.False(cache.Contains(admin, tenant, session));
    }
}
