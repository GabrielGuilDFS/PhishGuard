using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;

namespace PhishGuard.Backend.Security;

public interface IAuthSessionValidator
{
    Task<bool> IsActiveAsync(
        Guid administratorId,
        Guid tenantId,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Valida a sessao durante o evento OnTokenValidated, antes de o HttpContext.User
/// estar estabelecido. Por isso a consulta ignora os filtros globais e reescopa
/// explicitamente todas as entidades pelos IDs assinados no JWT.
/// </summary>
public sealed class AuthSessionValidator(
    AppDbContext context,
    TimeProvider timeProvider) : IAuthSessionValidator
{
    public Task<bool> IsActiveAsync(
        Guid administratorId,
        Guid tenantId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        return (
            from session in context.AuthSessions.IgnoreQueryFilters()
            join administrator in context.Administradores.IgnoreQueryFilters()
                on session.AdministratorId equals administrator.Id
            join tenant in context.Tenants.IgnoreQueryFilters()
                on session.TenantId equals tenant.Id
            where session.Id == sessionId
                && session.AdministratorId == administratorId
                && session.TenantId == tenantId
                && administrator.Id == administratorId
                && administrator.TenantId == tenantId
                && tenant.Id == tenantId
                && tenant.Ativo
                && session.RevokedAtUtc == null
                && session.ExpiresAtUtc > nowUtc
            select session.Id)
            .AnyAsync(cancellationToken);
    }
}
