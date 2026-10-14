using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Tests.Data;
using PhishGuard.Tests.TestDoubles;

namespace PhishGuard.Tests.Security;

public class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PHISHGUARD_TEST_POSTGRES")))
            Skip = "Defina PHISHGUARD_TEST_POSTGRES para o PostgreSQL descartável do MVP.";
    }
}

public class TenantMutationFilterTests
{
    internal static DbContextOptions<AppDbContext> PostgresOptions(string? database = null)
    {
        var connection = new Npgsql.NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("PHISHGUARD_TEST_POSTGRES"));
        // Nunca executar esta suíte no banco da aplicação.
        if (connection.Database != "phishguard_mvp_tests")
            throw new InvalidOperationException("A suíte requer o banco descartável phishguard_mvp_tests.");
        if (database is not null) connection.Database = database;
        return new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection.ConnectionString).Options;
    }

    internal static async Task<IActionResult> ExecuteAsync(AppDbContext db, MutableTenantProvider tenant,
        object controller, Func<Task<IActionResult>> action, CancellationToken cancellationToken = default)
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "POST";
        http.RequestAborted = cancellationToken;
        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var executing = new ActionExecutingContext(actionContext, filters, new Dictionary<string, object?>(), controller);
        ActionExecutedContext? executed = null;
        await new TenantMutationFilter(db, tenant).OnActionExecutionAsync(executing, async () =>
        {
            executed = new ActionExecutedContext(actionContext, filters, controller);
            try { executed.Result = await action(); }
            catch (Exception exception) { executed.Exception = exception; }
            return executed;
        });
        if (executed?.Exception is { } error && !executed.ExceptionHandled) throw error;
        return executing.Result ?? executed!.Result!;
    }

    [Fact]
    public async Task MissingTenant_CannotReachMutation()
    {
        var tenant = new MutableTenantProvider();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, tenant);
        var called = false;
        var result = await ExecuteAsync(db, tenant, new object(), () =>
        {
            called = true;
            return Task.FromResult<IActionResult>(new NoContentResult());
        });
        Assert.IsType<UnauthorizedResult>(result);
        Assert.False(called);
    }

    [PostgresFact]
    public async Task ParallelCreates_DoNotExceedQuota()
    {
        var options = PostgresOptions();
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using (var db = new AppDbContext(options, tenant))
        {
            await db.Database.MigrateAsync();
            await MvpGraph.SeedGraphAsync(db, tenant.TenantIdAtivo);
            for (var i = 0; i < PlanoLimites.LimiteAlvosBronze - 2; i++)
                db.Targets.Add(new Target { Nome = "Pessoa", Email = $"pessoa{i}@example.test", Departamento = "TI" });
            await db.SaveChangesAsync();
        }
        async Task<IActionResult> Create(string email)
        {
            await using var db = new AppDbContext(options, tenant);
            var controller = new TargetsController(db, tenant);
            return await ExecuteAsync(db, tenant, controller, async () =>
                (await controller.PostAlvo(new TargetInputDto { Nome = "Pessoa", Email = email, Departamento = "TI" })).Result!);
        }
        var results = await Task.WhenAll(Create("ultima1@example.test"), Create("ultima2@example.test"));
        Assert.Single(results.OfType<CreatedAtActionResult>());
        Assert.Single(results.OfType<BadRequestObjectResult>());
        await using var check = new AppDbContext(options, tenant);
        Assert.Equal(PlanoLimites.LimiteAlvosBronze, await check.Targets.CountAsync());
    }

    [PostgresFact]
    public async Task TenantLock_DoesNotBlockOtherTenant_AndCancellationRollsBack()
    {
        var options = PostgresOptions();
        var tenantA = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        var tenantB = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var dbA = new AppDbContext(options, tenantA);
        await dbA.Database.MigrateAsync();
        await MvpGraph.SeedGraphAsync(dbA, tenantA.TenantIdAtivo);
        await using var dbB = new AppDbContext(options, tenantB);
        await MvpGraph.SeedGraphAsync(dbB, tenantB.TenantIdAtivo);
        var locked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var holding = ExecuteAsync(dbA, tenantA, new object(), async () =>
        {
            locked.SetResult();
            await release.Task;
            return new NoContentResult();
        });
        await locked.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            var controllerB = new TargetsController(dbB, tenantB);
            var other = await ExecuteAsync(dbB, tenantB, controllerB, async () =>
                (await controllerB.PostAlvo(new TargetInputDto { Nome = "Pessoa", Email = "outro@example.test", Departamento = "TI" })).Result!)
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsType<CreatedAtActionResult>(other);
            await using var waitingDb = new AppDbContext(options, tenantA);
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ExecuteAsync(waitingDb, tenantA,
                new object(), () => Task.FromResult<IActionResult>(new NoContentResult()), cancellation.Token));
        }
        finally { release.TrySetResult(); await holding; }
        await using var check = new AppDbContext(options, tenantA);
        Assert.Equal(1, await check.Targets.CountAsync());
    }

    [PostgresFact]
    public async Task ActivationWinningRace_BlocksLaterEdit()
    {
        var options = PostgresOptions();
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var first = new AppDbContext(options, tenant);
        await first.Database.MigrateAsync();
        var campaign = await MvpGraph.SeedGraphAsync(first, tenant.TenantIdAtivo);
        var locked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var activation = ExecuteAsync(first, tenant, new object(), async () =>
        {
            campaign.Status = CampaignStatus.Agendada;
            await first.SaveChangesAsync();
            locked.SetResult();
            await release.Task;
            return new NoContentResult();
        });
        await locked.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await using var second = new AppDbContext(options, tenant);
        var controller = new CampaignsController(second, tenant, htmlSanitizer: new PhishGuard.Backend.Services.HtmlSanitizationService());
        var edit = ExecuteAsync(second, tenant, controller, () => controller.PutCampaign(campaign.Id, new CampaignInputDto { NomeCampanha = "Nova" }));
        release.SetResult();
        await activation;
        Assert.IsType<ConflictObjectResult>(await edit);
        Assert.Equal("Original", (await second.Campaigns.SingleAsync()).NomeCampanha);
    }

    [PostgresFact]
    public async Task DatabaseUniqueViolation_ReturnsConflictAndRollsBack()
    {
        var options = PostgresOptions();
        var tenant = new MutableTenantProvider { TenantIdAtivo = Guid.NewGuid() };
        await using var db = new AppDbContext(options, tenant);
        await db.Database.MigrateAsync();
        await MvpGraph.SeedGraphAsync(db, tenant.TenantIdAtivo);
        var result = await ExecuteAsync(db, tenant, new object(), async () =>
        {
            db.Targets.Add(new Target { Nome = "Duplicada", Email = "PESSOA@example.test", Departamento = "RH" });
            await db.SaveChangesAsync();
            return new NoContentResult();
        });
        Assert.IsType<ConflictObjectResult>(result);
        db.ChangeTracker.Clear();
        Assert.Equal(1, await db.Targets.CountAsync());
    }
}
