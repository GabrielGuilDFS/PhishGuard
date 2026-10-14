using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PhishGuard.Backend.Data;

namespace PhishGuard.Backend.Security;

/// <summary>
/// Serializa escritas curtas por tenant no PostgreSQL, incluindo checagem de cota,
/// edição de recursos e ativação. O worker nunca processa rascunhos e não faz I/O SMTP aqui.
/// O lock pertence à transação no banco, coordenando também múltiplas instâncias da API.
/// </summary>
public sealed class TenantMutationFilter(AppDbContext db, ITenantProvider tenantProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (HttpMethods.IsGet(context.HttpContext.Request.Method) || HttpMethods.IsHead(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        var tenantId = tenantProvider.GetTenantId();
        if (tenantId == Guid.Empty)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var ct = context.HttpContext.RequestAborted;
        // InMemory é exclusivo dos testes unitários; concorrência real é testada com Npgsql.
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        if (db.Database.IsNpgsql())
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM tenants WHERE id = {tenantId} FOR UPDATE", ct);

        var executed = await next();
        if (executed.Exception is { } exception && !executed.ExceptionHandled)
        {
            var message = ConflictMessage(exception);
            if (message is not null)
            {
                executed.ExceptionHandled = true;
                executed.Result = new ConflictObjectResult(new { message });
            }
            return; // Dispose faz rollback; exceções inesperadas/cancelamento propagam.
        }

        var status = executed.Result switch
        {
            ObjectResult result => result.StatusCode ?? 200,
            StatusCodeResult result => result.StatusCode,
            _ => 200
        };
        if (status < 400 && transaction is not null)
            await transaction.CommitAsync(ct);
    }

    private static string? ConflictMessage(Exception exception) => exception switch
    {
        DbUpdateConcurrencyException => "O recurso foi alterado simultaneamente. Atualize a tela e tente novamente.",
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ux_alvos_tenant_email_normalizado" } } => "Este e-mail já está cadastrado neste tenant.",
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } }
            => "O recurso possui vínculos ou uma referência deixou de existir. Atualize a tela antes de tentar novamente.",
        _ => null
    };
}
