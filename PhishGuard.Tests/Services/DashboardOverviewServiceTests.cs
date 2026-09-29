using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;

namespace PhishGuard.Tests.Services;

public class DashboardOverviewServiceTests
{
    private sealed class TenantProvider(Guid id) : ITenantProvider
    {
        public Guid GetTenantId() => id;
        public Guid GetCurrentTenantId() => id;
    }
    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Theory]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    public async Task AgregacoesRelacionais_PreservamCicloAnteriorCoorteEFronteiras(int days)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var tenant = Guid.NewGuid();
        await using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options, new TenantProvider(tenant));
        await context.Database.EnsureCreatedAsync();
        // Fixture foca nas consultas: referências de templates não são necessárias.
        await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
        var now = new DateTimeOffset(2026, 8, 6, 1, 0, 0, TimeSpan.Zero);
        var time = new DashboardReportingTime(new Clock(now),
            TimeZoneInfo.CreateCustomTimeZone("Test", TimeSpan.FromHours(-3), "Test", "Test"));
        var start = time.StartOfDayUtc(new DateOnly(2026, 8, 5).AddDays(1 - days));
        var target = Guid.NewGuid(); var orphan = Guid.NewGuid(); var campaign = Guid.NewGuid();
        context.Tenants.Add(new Tenant { Id = tenant, NomeEmpresa = "Empresa", Cnpj = "12345678000199" });
        context.Targets.AddRange(
            new Target { Id = target, Nome = "Alvo", Email = "a@test", Departamento = " Segurança " },
            new Target { Id = orphan, Nome = "Sem envio", Email = "b@test", Departamento = " Segurança " });
        await context.SaveChangesAsync();
        SimulationLog Log(Guid recipient, string action, DateTime date) => new()
        {
            Id = Guid.NewGuid(), CampaignId = campaign, TargetId = recipient,
            Acao = action, DataHora = date, IpOrigem = "127.0.0.1"
        };
        context.SimulationsLogs.AddRange(
            Log(target, SimulationActions.Envio, start),
            Log(target, SimulationActions.Clique, start.AddDays(1)),
            Log(target, SimulationActions.TreinamentoConcluido, now.UtcDateTime),
            Log(orphan, SimulationActions.Clique, start));
        var previous = Log(target, SimulationActions.Envio, start.AddDays(-days));
        previous.CampaignId = Guid.NewGuid();
        context.SimulationsLogs.Add(previous);
        await context.SaveChangesAsync();

        var result = await new DashboardOverviewService(context, time)
            .GetOverviewAsync(tenant, $"{days}d", "segurança");
        Assert.Equal(1, result.Kpis.Sent.Total);
        Assert.Equal(0d, result.Kpis.Sent.DeltaPercent);
        Assert.Equal(1, result.Kpis.OpenRate.InferredTotal);
        Assert.Equal(1, result.Trend[^1].Clicked);
        Assert.Equal(1, result.Trend[^1].Trained);
        Assert.Equal(days == 90 ? 13 : days, result.Trend.Count);
        Assert.Equal(0, result.Trend[0].Trained);
        Assert.Equal(days == 90 ? 1 : 0, result.Trend[0].Clicked);
        Assert.Equal(new[] { "Segurança" }, result.AvailableDepartments);
    }
}
