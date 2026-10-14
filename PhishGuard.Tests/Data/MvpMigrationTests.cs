using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Tests.Security;

namespace PhishGuard.Tests.Data;

public class MvpMigrationTests
{
    private const string PreviousMigration = "20260810183052_RestoreMercadoLivScenarioLabels";

    [PostgresFact]
    public async Task LegacyEmails_BackfilledBeforeUniqueIndex()
    {
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var db = new AppDbContext(TenantMutationFilterTests.PostgresOptions("phishguard_mvp_upgrade"), tenant);
        await db.Database.EnsureDeletedAsync();
        await db.GetService<IMigrator>().MigrateAsync(PreviousMigration);
        await SeedLegacyAsync(db, tenant.TenantIdAtivo, false);
        await db.Database.MigrateAsync();
        Assert.Equal("pessoa@example.test", (await db.Targets.SingleAsync()).EmailNormalizado);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [PostgresFact]
    public async Task LegacyUnicodeEmail_NormalizesLikeApplication()
    {
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var db = new AppDbContext(TenantMutationFilterTests.PostgresOptions("phishguard_mvp_upgrade"), tenant);
        await db.Database.EnsureDeletedAsync();
        await db.GetService<IMigrator>().MigrateAsync(PreviousMigration);
        const string email = "\tİÉCOLE@EXAMPLE.test\u00a0";
        await SeedLegacyAsync(db, tenant.TenantIdAtivo, false, email);
        await db.Database.MigrateAsync();
        Assert.Equal(Target.NormalizeEmail(email), (await db.Targets.SingleAsync()).EmailNormalizado);
    }

    [PostgresFact]
    public async Task LegacyDuplicates_AbortMigrationWithoutDeletingRows()
    {
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var db = new AppDbContext(TenantMutationFilterTests.PostgresOptions("phishguard_mvp_duplicates"), tenant);
        await db.Database.EnsureDeletedAsync();
        await db.GetService<IMigrator>().MigrateAsync(PreviousMigration);
        await SeedLegacyAsync(db, tenant.TenantIdAtivo, true);
        var error = await Assert.ThrowsAsync<PostgresException>(() => db.Database.MigrateAsync());
        Assert.Contains("duplicados", error.MessageText);
        Assert.DoesNotContain(await db.Database.GetAppliedMigrationsAsync(), name => name.Contains("ProtectMvp"));
        await using var command = db.Database.GetDbConnection().CreateCommand();
        await db.Database.OpenConnectionAsync();
        command.CommandText = "SELECT count(*) FROM alvos";
        Assert.Equal(2L, await command.ExecuteScalarAsync());
        command.CommandText = "SELECT count(*) FROM information_schema.columns WHERE table_name = 'alvos' AND column_name = 'email_normalizado'";
        Assert.Equal(0L, await command.ExecuteScalarAsync());
    }

    private static async Task SeedLegacyAsync(AppDbContext db, Guid tenantId, bool duplicate, string email = " PESSOA@example.test ")
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO tenants (id,nome_empresa,cnpj,ativo,criado_em,plano) VALUES ({tenantId},'Teste','11111111000191',true,now(),0)");
        var targetId = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO alvos (id,tenant_id,nome,email,departamento) VALUES ({targetId},{tenantId},'Pessoa',{email},'TI')");
        if (duplicate)
        {
            var duplicateId = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO alvos (id,tenant_id,nome,email,departamento) VALUES ({duplicateId},{tenantId},'Pessoa','pessoa@example.test','RH')");
        }
    }
}
