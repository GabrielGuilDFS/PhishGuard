namespace PhishGuard.Backend.Services;

/// <summary>
/// Centraliza o relógio e o fuso usados nos recortes civis do dashboard.
/// Os eventos permanecem persistidos em UTC; apenas os limites e rótulos são locais.
/// </summary>
public sealed class DashboardReportingTime
{
    public const string DefaultTimeZoneId = "America/Sao_Paulo";
    private const string WindowsSaoPauloTimeZoneId = "E. South America Standard Time";

    private readonly TimeProvider _timeProvider;

    public DashboardReportingTime(TimeProvider timeProvider, string timeZoneId)
        : this(timeProvider, ResolveTimeZone(timeZoneId))
    {
    }

    public DashboardReportingTime(TimeProvider timeProvider, TimeZoneInfo timeZone)
    {
        _timeProvider = timeProvider;
        TimeZone = timeZone;
    }

    public TimeZoneInfo TimeZone { get; }

    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow().ToUniversalTime();

    public DateTimeOffset LocalNow => TimeZoneInfo.ConvertTime(UtcNow, TimeZone);

    public DateTime StartOfDayUtc(DateOnly localDate) => ToUtc(localDate.ToDateTime(TimeOnly.MinValue));

    public DateTime ToUtc(DateTime localDateTime)
    {
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        if (TimeZone.IsInvalidTime(unspecified))
            throw new InvalidOperationException("O horário local informado não existe no fuso do dashboard.");

        return TimeZoneInfo.ConvertTimeToUtc(unspecified, TimeZone);
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        var configuredId = string.IsNullOrWhiteSpace(timeZoneId)
            ? DefaultTimeZoneId
            : timeZoneId.Trim();

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(configuredId);
        }
        catch (TimeZoneNotFoundException) when (
            configuredId.Equals(DefaultTimeZoneId, StringComparison.OrdinalIgnoreCase))
        {
            // Windows tradicional usa IDs próprios; Linux/Docker usa IANA.
            return TimeZoneInfo.FindSystemTimeZoneById(WindowsSaoPauloTimeZoneId);
        }
    }
}
