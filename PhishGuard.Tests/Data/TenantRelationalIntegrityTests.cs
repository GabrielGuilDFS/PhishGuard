using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Tests.Security;
using PhishGuard.Tests.TestDoubles;

namespace PhishGuard.Tests.Data;

public sealed class TenantRelationalIntegrityTests
{
    [Fact]
    public async Task Sqlite_RejectsCrossTenantWritesAndHistoryWithoutMembership()
    {
        using var harness = new SqliteTenantHarness(Guid.NewGuid());
        await AssertConstraintsAsync(harness.Db, harness.Tenant);
    }

    [PostgresFact]
    public async Task Postgres_RejectsCrossTenantWritesAndHistoryWithoutMembership()
    {
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var db = new AppDbContext(
            TenantMutationFilterTests.PostgresOptions("phishguard_mvp_relational_constraints"), tenant);
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
        await AssertConstraintsAsync(db, tenant);
    }

    [Fact]
    public async Task Sqlite_AssociationFilterAndPurgeAreScopedAndTransactional()
    {
        using var harness = new SqliteTenantHarness(Guid.NewGuid());
        await AssertFilterAndPurgeAsync(harness.Db, harness.Tenant);
    }

    [PostgresFact]
    public async Task Postgres_AssociationFilterAndPurgeAreScopedAndTransactional()
    {
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var db = new AppDbContext(
            TenantMutationFilterTests.PostgresOptions("phishguard_mvp_relational_purge"), tenant);
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
        await AssertFilterAndPurgeAsync(db, tenant);
    }

    private static async Task AssertConstraintsAsync(AppDbContext db, MutableTenantProvider tenant)
    {
        var graph = await SeedTwoTenantsAsync(db, tenant);
        tenant.TenantIdAtivo = graph.TenantA;
        var localNonParticipant = new Target
        {
            Id = Guid.NewGuid(), Nome = "Sem participação", Email = "sem-participacao@example.test", Departamento = "RH"
        };
        db.Targets.Add(localNonParticipant);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // SQL parametrizado contorna filtros e validações do EF: quem rejeita é o banco.
        var invalidWrites = new (string Sql, object[] Values)[]
        {
            ("UPDATE campaigns SET email_template_id = {0} WHERE id = {1}", [graph.B.EmailTemplateId, graph.A.Id]),
            ("UPDATE campaigns SET landing_page_id = {0} WHERE id = {1}", [graph.B.LandingPageId, graph.A.Id]),
            ("UPDATE campaigns SET educational_page_id = {0} WHERE id = {1}", [graph.B.EducationalPageId, graph.A.Id]),
            ("UPDATE campaign_target SET targets_id = {0} WHERE campaigns_id = {1}", [graph.TargetB, graph.A.Id]),
            ("UPDATE campaign_target SET tenant_id = {0} WHERE campaigns_id = {1}", [graph.TenantB, graph.A.Id]),
            ("UPDATE simulations_logs SET campaign_id = {0} WHERE campaign_id = {1}", [graph.B.Id, graph.A.Id]),
            ("UPDATE simulations_logs SET target_id = {0} WHERE campaign_id = {1}", [graph.TargetB, graph.A.Id]),
            ("UPDATE simulations_logs SET tenant_id = {0} WHERE campaign_id = {1}", [graph.TenantB, graph.A.Id]),
            ("UPDATE campaign_deliveries SET campaign_id = {0} WHERE campaign_id = {1}", [graph.B.Id, graph.A.Id]),
            ("UPDATE campaign_deliveries SET target_id = {0} WHERE campaign_id = {1}", [graph.TargetB, graph.A.Id]),
            ("UPDATE campaign_deliveries SET tenant_id = {0} WHERE campaign_id = {1}", [graph.TenantB, graph.A.Id]),
            ("UPDATE auth_sessions SET administrator_id = {0} WHERE tenant_id = {1}", [graph.AdminB, graph.TenantA]),
            ("UPDATE simulations_logs SET target_id = {0} WHERE campaign_id = {1}", [localNonParticipant.Id, graph.A.Id]),
            ("UPDATE campaign_deliveries SET target_id = {0} WHERE campaign_id = {1}", [localNonParticipant.Id, graph.A.Id]),
            ("DELETE FROM campaign_target WHERE campaigns_id = {0}", [graph.A.Id])
        };
        foreach (var (sql, values) in invalidWrites)
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var error = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(sql, values));
            if (db.Database.IsNpgsql())
                Assert.Equal("23503", Assert.IsType<Npgsql.PostgresException>(error).SqlState);
            else
                Assert.Equal(19, Assert.IsType<Microsoft.Data.Sqlite.SqliteException>(error).SqliteErrorCode);
            await transaction.RollbackAsync();
        }
        Assert.Equal(1, await db.SimulationsLogs.CountAsync());
        Assert.Equal(1, await db.CampaignDeliveries.CountAsync());
        Assert.Equal(graph.TargetA, (await db.SimulationsLogs.SingleAsync()).TargetId);
    }

    private static async Task AssertFilterAndPurgeAsync(AppDbContext db, MutableTenantProvider tenant)
    {
        var graph = await SeedTwoTenantsAsync(db, tenant);
        db.DataProtectionKeys.Add(new DataProtectionKey { FriendlyName = "global", Xml = "<key />" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        tenant.TenantIdAtivo = graph.TenantA;
        Assert.Equal(graph.TenantA, (await db.Set<CampaignTarget>().SingleAsync()).TenantId);
        tenant.TenantIdAtivo = graph.TenantB;
        Assert.Equal(graph.TenantB, (await db.Set<CampaignTarget>().SingleAsync()).TenantId);
        tenant.TenantIdAtivo = Guid.Empty;
        Assert.Empty(await db.Set<CampaignTarget>().ToListAsync());
        Assert.Equal(2, await db.Set<CampaignTarget>().IgnoreQueryFilters().CountAsync());
        tenant.TenantIdAtivo = graph.TenantA;

        var script = ReadPurgeScript();
        // Exercita os DELETEs do artefato de manutenção real, sem psql nem banco de usuários.
        var deletes = Regex.Matches(script,
                @"^DELETE FROM [a-z_]+ WHERE (?:tenant_id|id) = :'tenant_id'::uuid;", RegexOptions.Multiline)
            .Select(match => match.Value.Replace(":'tenant_id'::uuid", "{0}")).ToArray();
        Assert.Equal(12, deletes.Length);
        Assert.Contains("\\set apply false", script);
        Assert.Contains("ROLLBACK;", script);
        Assert.DoesNotContain("DELETE FROM data_protection_keys", script);

        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            foreach (var sql in deletes)
                await db.Database.ExecuteSqlRawAsync(sql, graph.TenantA);
            Assert.False(await db.Tenants.AnyAsync(x => x.Id == graph.TenantA));
            await AssertOtherTenantIntactAsync(db, tenant, graph.TenantB);
            await transaction.RollbackAsync();
        }
        tenant.TenantIdAtivo = graph.TenantA;
        Assert.True(await db.Tenants.AnyAsync(x => x.Id == graph.TenantA));
        Assert.Equal(1, await db.SimulationsLogs.CountAsync());
        Assert.Equal(1, await db.AuthSessions.CountAsync());
        Assert.Equal(1, await db.Set<CampaignTarget>().CountAsync());

        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            foreach (var sql in deletes)
                await db.Database.ExecuteSqlRawAsync(sql, graph.TenantA);
            await transaction.CommitAsync();
        }
        Assert.False(await db.Tenants.AnyAsync(x => x.Id == graph.TenantA));
        Assert.Empty(await db.Set<CampaignTarget>().ToListAsync());
        Assert.Empty(await db.SimulationsLogs.ToListAsync());
        Assert.Empty(await db.CampaignDeliveries.ToListAsync());
        Assert.Empty(await db.AuthSessions.ToListAsync());
        await AssertOtherTenantIntactAsync(db, tenant, graph.TenantB);
        Assert.Equal(1, await db.DataProtectionKeys.CountAsync());
    }

    private static async Task AssertOtherTenantIntactAsync(AppDbContext db, MutableTenantProvider tenant, Guid tenantB)
    {
        var original = tenant.TenantIdAtivo;
        tenant.TenantIdAtivo = tenantB;
        Assert.True(await db.Tenants.AnyAsync(x => x.Id == tenantB));
        Assert.Equal(1, await db.Campaigns.CountAsync());
        Assert.Equal(1, await db.Targets.CountAsync());
        Assert.Equal(1, await db.Set<CampaignTarget>().CountAsync());
        Assert.Equal(1, await db.SimulationsLogs.CountAsync());
        Assert.Equal(1, await db.CampaignDeliveries.CountAsync());
        Assert.Equal(1, await db.Administradores.CountAsync());
        Assert.Equal(1, await db.AuthSessions.CountAsync());
        tenant.TenantIdAtivo = original;
    }

    private static async Task<(Guid TenantA, Guid TenantB, Campaign A, Campaign B,
        Guid TargetA, Guid TargetB, Guid AdminB)> SeedTwoTenantsAsync(AppDbContext db, MutableTenantProvider tenant)
    {
        var tenantA = tenant.TenantIdAtivo;
        var a = await MvpGraph.SeedGraphAsync(db, tenantA, CampaignStatus.Finalizada, history: true);
        await SeedAdminSessionAsync(db, tenantA);
        var tenantB = Guid.NewGuid();
        tenant.TenantIdAtivo = tenantB;
        var b = await MvpGraph.SeedGraphAsync(db, tenantB, CampaignStatus.Finalizada, history: true);
        var adminB = await SeedAdminSessionAsync(db, tenantB);
        var targetA = a.Targets.Single().Id;
        var targetB = b.Targets.Single().Id;
        db.ChangeTracker.Clear();
        return (tenantA, tenantB, a, b, targetA, targetB, adminB);
    }

    private static async Task<Guid> SeedAdminSessionAsync(AppDbContext db, Guid tenantId)
    {
        var adminId = Guid.NewGuid();
        db.Administradores.Add(new Administrador
        {
            Id = adminId, TenantId = tenantId, Nome = "Admin", Email = $"{adminId:N}@example.test", PasswordHash = "test-only"
        });
        db.AuthSessions.Add(new AuthSession
        {
            Id = Guid.NewGuid(), TenantId = tenantId, AdministratorId = adminId,
            RefreshTokenHash = Guid.NewGuid().ToString("N"), CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();
        return adminId;
    }

    private static string ReadPurgeScript()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "scripts", "purge-tenant.sql");
            if (File.Exists(path)) return File.ReadAllText(path);
        }
        throw new FileNotFoundException("scripts/purge-tenant.sql não encontrado no checkout de teste.");
    }
}
