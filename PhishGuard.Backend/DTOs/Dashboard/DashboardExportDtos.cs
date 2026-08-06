namespace PhishGuard.Backend.DTOs.Dashboard;

public enum DashboardExportFormat
{
    Pdf,
    Csv
}

public enum DashboardCsvDataset
{
    Campaigns,
    Trend,
    Summary
}

public sealed class DashboardReportContext
{
    public DateTimeOffset GeneratedAt { get; init; }
    public string DepartmentLabel { get; init; } = "Todos";
    public DashboardReportIdentityDto Identity { get; init; } = new();
}

public sealed class DashboardReportIdentityDto
{
    public string CompanyName { get; init; } = string.Empty;
    public string Cnpj { get; init; } = string.Empty;
    public string AdministratorName { get; init; } = string.Empty;
}

public sealed record DashboardExportFile(
    byte[] Content,
    string ContentType,
    string FileName);
