using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using PhishGuard.Tests.TestDoubles;

namespace PhishGuard.Tests.Data;

public class MvpIntegrityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CapturedSnapshots_AreImmutableForContextWriters(bool participantSnapshot)
    {
        using var h = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(h.Db, h.Tenant.TenantIdAtivo);
        var participant = await h.Db.CampaignTargets.SingleAsync();
        var captured = DateTime.UtcNow;
        campaign.EmailBodySnapshot = "<p>Conteúdo congelado</p>";
        campaign.SnapshotCapturedAtUtc = captured;
        participant.DepartmentSnapshot = "TI";
        participant.SnapshotCapturedAtUtc = captured;
        await h.Db.SaveChangesAsync();

        // Lifecycle bookkeeping still works; changing current contact data is allowed.
        campaign.Status = CampaignStatus.Agendada;
        participant.Target.Departamento = "RH";
        await h.Db.SaveChangesAsync();
        if (participantSnapshot) participant.DepartmentSnapshot = "RH";
        else campaign.EmailBodySnapshot = "<p>Conteúdo alterado</p>";

        await Assert.ThrowsAsync<InvalidOperationException>(() => h.Db.SaveChangesAsync());
        h.Db.ChangeTracker.Clear();
        Assert.Equal("<p>Conteúdo congelado</p>", (await h.Db.Campaigns.SingleAsync()).EmailBodySnapshot);
        Assert.Equal("TI", (await h.Db.CampaignTargets.SingleAsync()).DepartmentSnapshot);
        Assert.Equal("RH", (await h.Db.Targets.SingleAsync()).Departamento);
    }

    [Fact]
    public async Task TargetDeletion_CannotCascadeHistoryEvenWithoutController()
    {
        using var h = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(h.Db, h.Tenant.TenantIdAtivo, CampaignStatus.Finalizada, true);
        h.Db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            h.Db.Targets.Remove(await h.Db.Targets.SingleAsync());
            await h.Db.SaveChangesAsync();
        });
        h.Db.ChangeTracker.Clear();
        Assert.Equal(1, await h.Db.Targets.CountAsync());
        Assert.Equal(1, await h.Db.SimulationsLogs.CountAsync());
        Assert.Equal(1, await h.Db.CampaignDeliveries.CountAsync());
    }

    [Fact]
    public async Task NormalizedEmail_IsUniquePerTenantAtDatabaseAndController()
    {
        using var h = new SqliteTenantHarness(Guid.NewGuid());
        await MvpGraph.SeedGraphAsync(h.Db, h.Tenant.TenantIdAtivo);
        var input = new TargetInputDto { Nome = "Duplicada", Email = " PESSOA@example.test ", Departamento = "RH" };
        var controller = new TargetsController(h.Db, h.Tenant);
        Assert.IsType<ConflictObjectResult>((await controller.PostAlvo(input)).Result);
        var duplicate = new Target { Nome = input.Nome, Email = input.Email, Departamento = input.Departamento };
        h.Db.Targets.Add(duplicate);
        await Assert.ThrowsAsync<DbUpdateException>(() => h.Db.SaveChangesAsync());
        h.Db.ChangeTracker.Clear();
        var anotherTenant = Guid.NewGuid();
        h.UsarTenant(anotherTenant);
        await MvpGraph.SeedGraphAsync(h.Db, anotherTenant);
        Assert.Equal(1, await h.Db.Targets.CountAsync());
    }
}
