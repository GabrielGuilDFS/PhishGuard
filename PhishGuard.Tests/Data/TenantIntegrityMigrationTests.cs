using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Tests.Security;

namespace PhishGuard.Tests.Data;

public sealed class TenantIntegrityMigrationTests
{
    private const string PreviousMigration = "20261003015038_ProtectMvpCampaignHistoryAndTargetUniqueness";

    [PostgresFact]
    public async Task LegacyCrossTenantReference_AbortsUpgradeWithoutDeletingOrPartiallyBackfilling()
        => await AssertInvalidUpgradeAsync("phishguard_mvp_legacy_cross_fk", crossTenantTemplate: true);

    [PostgresFact]
    public async Task LegacyHistoryWithoutParticipant_AbortsUpgradeWithoutDeletingOrPartiallyBackfilling()
        => await AssertInvalidUpgradeAsync("phishguard_mvp_legacy_missing_participant", crossTenantTemplate: false);

    [PostgresFact]
    public async Task ValidLegacyGraph_PreservesIdsAndMarksAvailableSnapshotAsLegacy()
    {
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var db = new AppDbContext(TenantMutationFilterTests.PostgresOptions("phishguard_mvp_legacy_valid_snapshot"), tenant);
        await db.Database.EnsureDeletedAsync();
        await db.GetService<IMigrator>().MigrateAsync(PreviousMigration);
        var seeded = await SeedLegacyGraphAsync(db, tenant.TenantIdAtivo, crossTenantTemplate: false, includeParticipant: true);
        Assert.Empty(await ReadAuditRelationshipsAsync(db));

        await db.Database.MigrateAsync();

        var participation = await db.Set<CampaignTarget>().SingleAsync();
        Assert.Equal(tenant.TenantIdAtivo, participation.TenantId);
        Assert.Equal(seeded.Campaign, participation.CampaignsId);
        Assert.Equal(seeded.Target, participation.TargetsId);
        Assert.Equal("Pessoa legada", participation.TargetNameSnapshot);
        Assert.Equal("legacy@example.test", participation.TargetEmailSnapshot);
        Assert.Equal("Departamento atual disponível", participation.DepartmentSnapshot);
        Assert.True(participation.IsLegacySnapshot);
        Assert.Equal(seeded.Log, (await db.SimulationsLogs.SingleAsync()).Id);
        Assert.Equal(seeded.Delivery, (await db.CampaignDeliveries.SingleAsync()).Id);
        Assert.Equal(seeded.Campaign, (await db.Campaigns.SingleAsync()).Id);
        Assert.Equal(seeded.Target, (await db.Targets.SingleAsync()).Id);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    private static async Task AssertInvalidUpgradeAsync(string database, bool crossTenantTemplate)
    {
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var db = new AppDbContext(TenantMutationFilterTests.PostgresOptions(database), tenant);
        await db.Database.EnsureDeletedAsync();
        await db.GetService<IMigrator>().MigrateAsync(PreviousMigration);
        var seeded = await SeedLegacyGraphAsync(db, tenant.TenantIdAtivo, crossTenantTemplate,
            includeParticipant: crossTenantTemplate);
        var before = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
        var audit = await ReadAuditRelationshipsAsync(db);
        if (crossTenantTemplate)
            Assert.Contains("campaign_template:tenant_divergente", audit);
        else
        {
            Assert.Contains("simulation_log:alvo_sem_participacao", audit);
            Assert.Contains("campaign_delivery:alvo_sem_participacao", audit);
        }

        var failure = await Assert.ThrowsAsync<PostgresException>(() => db.Database.MigrateAsync());
        Assert.False(string.IsNullOrWhiteSpace(failure.MessageText));
        Assert.Equal(before, (await db.Database.GetAppliedMigrationsAsync()).ToArray());
        Assert.Equal(1L, await ScalarAsync(db, "SELECT count(*) FROM simulations_logs WHERE id = @id", seeded.Log));
        Assert.Equal(1L, await ScalarAsync(db, "SELECT count(*) FROM campaign_deliveries WHERE id = @id", seeded.Delivery));
        Assert.Equal(1L, await ScalarAsync(db, "SELECT count(*) FROM campaigns WHERE id = @id", seeded.Campaign));
        Assert.Equal(1L, await ScalarAsync(db, "SELECT count(*) FROM alvos WHERE id = @id", seeded.Target));
        Assert.Equal(0L, await ScalarAsync(db,
            "SELECT count(*) FROM information_schema.columns WHERE table_name = 'campaign_target' AND column_name = 'tenant_id'"));
        Assert.Equal(0L, await ScalarAsync(db,
            "SELECT count(*) FROM information_schema.columns WHERE table_name = 'campaign_target' AND column_name = 'department_snapshot'"));
    }

    private static async Task<(Guid Campaign, Guid Target, Guid Log, Guid Delivery)> SeedLegacyGraphAsync(
        AppDbContext db, Guid tenantId, bool crossTenantTemplate, bool includeParticipant)
    {
        var otherTenant = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO tenants (id,nome_empresa,cnpj,ativo,criado_em,plano) VALUES ({tenantId},'Legado','11111111000191',true,now(),0),({otherTenant},'Outro','22222222000191',true,now(),0)");
        var templateId = Guid.NewGuid();
        var templateTenant = crossTenantTemplate ? otherTenant : tenantId;
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO templates (id,tenant_id,nome,assunto,remetente_nome,remetente_email,corpo_html,criado_em) VALUES ({templateId},{templateTenant},'Legado','Assunto','Empresa','sender@example.test','<p>Original legado</p>',now())");
        var pageId = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO phishing_pages (id,tenant_id,nome,html_captura,criado_em) VALUES ({pageId},{tenantId},'Página legada','<form>Original</form>',now())");
        var educationalId = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO educational_pages (id,tenant_id,nome,html_educacional,criado_em) VALUES ({educationalId},{tenantId},'Educação legada','<p>Treinamento original</p>',now())");
        var targetId = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO alvos (id,tenant_id,nome,email,email_normalizado,departamento) VALUES ({targetId},{tenantId},'Pessoa legada','legacy@example.test','legacy@example.test','Departamento atual disponível')");
        var campaignId = Guid.NewGuid();
        var status = CampaignStatus.Finalizada;
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO campaigns (id,tenant_id,nome_campanha,status,data_inicio,email_template_id,landing_page_id,educational_page_id,criado_em) VALUES ({campaignId},{tenantId},'Campanha legada',{status},now(),{templateId},{pageId},{educationalId},now())");
        if (includeParticipant)
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO campaign_target (campaigns_id,targets_id) VALUES ({campaignId},{targetId})");
        var logId = Guid.NewGuid();
        var action = SimulationActions.Envio;
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO simulations_logs (id,tenant_id,campaign_id,target_id,acao,data_hora,ip_origem) VALUES ({logId},{tenantId},{campaignId},{targetId},{action},now(),'SISTEMA')");
        var deliveryId = Guid.NewGuid();
        var token = Guid.NewGuid();
        var idempotency = $"campaign/{campaignId:N}/target/{targetId:N}";
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO campaign_deliveries (id,tenant_id,campaign_id,target_id,idempotency_key,status,attempt_count,concurrency_token,last_attempt_at_utc,sent_at_utc) VALUES ({deliveryId},{tenantId},{campaignId},{targetId},{idempotency},1,1,{token},now(),now())");
        return (campaignId, targetId, logId, deliveryId);
    }

    private static async Task<long> ScalarAsync(AppDbContext db, string sql, Guid? id = null)
    {
        await db.Database.OpenConnectionAsync();
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        if (id is { } value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = "id";
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private static async Task<List<string>> ReadAuditRelationshipsAsync(AppDbContext db)
    {
        string? path = null;
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "scripts", "check-mvp-integrity.sql");
            if (File.Exists(candidate)) { path = candidate; break; }
        }
        Assert.NotNull(path);
        await db.Database.OpenConnectionAsync();
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = File.ReadAllText(path);
        await using var reader = await command.ExecuteReaderAsync();
        var relationships = new List<string>();
        do
        {
            var hasRelationship = Enumerable.Range(0, reader.FieldCount).Any(i => reader.GetName(i) == "relacao");
            while (await reader.ReadAsync())
                if (hasRelationship)
                    relationships.Add($"{reader.GetString(reader.GetOrdinal("relacao"))}:{reader.GetString(reader.GetOrdinal("motivo"))}");
        } while (await reader.NextResultAsync());
        return relationships;
    }
}
