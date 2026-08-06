using PhishGuard.Backend.Services;

namespace PhishGuard.Tests.Services;

public class DashboardReportingTimeTests
{
    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    [Fact]
    public void AmericaSaoPaulo_ResolveNoRuntimeAtualEConverteViradaUtc()
    {
        var reportingTime = new DashboardReportingTime(
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 6, 1, 11, 0, TimeSpan.Zero)),
            DashboardReportingTime.DefaultTimeZoneId);

        Assert.Equal(new DateTime(2026, 8, 5, 22, 11, 0), reportingTime.LocalNow.DateTime);
        Assert.Equal(TimeSpan.FromHours(-3), reportingTime.LocalNow.Offset);
        Assert.Equal(
            new DateTime(2026, 8, 5, 3, 0, 0, DateTimeKind.Utc),
            reportingTime.StartOfDayUtc(new DateOnly(2026, 8, 5)));
    }
}
