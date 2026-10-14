using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Security;

/// <summary>
/// Resolves only the tenant of a signed tracking capability, then keeps every
/// entity query inside that explicit tenant with the global filters active.
/// </summary>
public sealed class TrackingCampaignScope(AppDbContext context, ITrackingTokenService tokens)
{
    public async Task<TrackingCampaignLease?> ResolveAsync(Guid campaignId, Guid targetId,
        string? trackingToken, CancellationToken cancellationToken = default)
    {
        if (!tokens.Validate(trackingToken, campaignId, targetId))
            return null;

        Guid tenantId;
        if (context.Database.IsRelational())
        {
            // No campaign payload is read outside its tenant. The signed pair is
            // checked before this fixed, parametrized scalar discovery query.
            tenantId = await context.Database.SqlQuery<Guid>(
                    $"SELECT tenant_id AS \"Value\" FROM campaigns WHERE id = {campaignId}")
                .SingleOrDefaultAsync(cancellationToken);
        }
        else
        {
            // InMemory fixtures already establish their own tenant. They cannot
            // discover another tenant, even if a test token service is permissive.
            tenantId = await context.Campaigns.Where(c => c.Id == campaignId)
                .Select(c => c.TenantId).SingleOrDefaultAsync(cancellationToken);
        }

        if (tenantId == Guid.Empty
            || context.TenantIdAtual != Guid.Empty && context.TenantIdAtual != tenantId)
            return null;

        var tenantScope = context.UseSystemTenantScope(tenantId);
        try
        {
            var campaign = await context.Campaigns.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.TenantId == tenantId, cancellationToken);
            if (campaign is null || !await context.CampaignTargets.AnyAsync(p =>
                p.TenantId == tenantId && p.CampaignsId == campaignId && p.TargetsId == targetId,
                cancellationToken))
            {
                tenantScope.Dispose();
                return null;
            }

            return new TrackingCampaignLease(campaign, tenantScope);
        }
        catch
        {
            tenantScope.Dispose();
            throw;
        }
    }
}

/// <summary>Owns the temporary tenant scope for the complete tracking operation.</summary>
public sealed class TrackingCampaignLease : IDisposable
{
    private IDisposable? _tenantScope;

    internal TrackingCampaignLease(Campaign campaign, IDisposable tenantScope)
    {
        Campaign = campaign;
        _tenantScope = tenantScope;
    }

    public Campaign Campaign { get; }

    public void Dispose() => Interlocked.Exchange(ref _tenantScope, null)?.Dispose();
}
