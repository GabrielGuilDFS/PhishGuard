namespace PhishGuard.Backend.DTOs.Dashboard;

public sealed class DashboardOverviewResponse
{
    public DashboardPeriodDto Period { get; init; } = new();
    public DashboardTenantDto Tenant { get; init; } = new();
    public DashboardScopeDto Scope { get; init; } = new();
    public IReadOnlyList<string> AvailableDepartments { get; init; } = [];
    public DashboardKpisDto Kpis { get; init; } = new();
    public DashboardTrainingEffectivenessDto TrainingEffectiveness { get; init; } = new();
    public IReadOnlyList<DashboardTrendPointDto> Trend { get; init; } = [];
    public IReadOnlyList<DashboardRecentCampaignDto> RecentCampaigns { get; init; } = [];
}

public sealed class DashboardPeriodDto
{
    public DateOnly Start { get; init; }
    public DateOnly End { get; init; }
    public string Label { get; init; } = string.Empty;
}

public sealed class DashboardTenantDto
{
    public string Name { get; init; } = string.Empty;
}

public sealed class DashboardScopeDto
{
    public int CampaignCount { get; init; }
    public int UniqueTargetCount { get; init; }
    public int CampaignTargetCount { get; init; }
}

public sealed class DashboardKpisDto
{
    public DashboardSentKpiDto Sent { get; init; } = new();
    public DashboardOpenRateKpiDto OpenRate { get; init; } = new();
    public DashboardRateKpiDto ClickRate { get; init; } = new();
    public DashboardRateKpiDto CompromiseRate { get; init; } = new();
    public DashboardRateKpiDto TrainingRate { get; init; } = new();
}

public sealed class DashboardSentKpiDto
{
    public int Total { get; init; }
    public double? DeltaPercent { get; init; }
}

public sealed class DashboardRateKpiDto
{
    public double Rate { get; init; }
    public int UniqueTotal { get; init; }
}

public sealed class DashboardOpenRateKpiDto
{
    public double Rate { get; init; }
    public int UniqueTotal { get; init; }
    public int ObservedTotal { get; init; }
    public int InferredTotal { get; init; }
}

public sealed class DashboardTrainingEffectivenessDto
{
    public DashboardRateKpiDto Compromised { get; init; } = new();
    public DashboardRateKpiDto EducationViewed { get; init; } = new();
    public DashboardRateKpiDto Completed { get; init; } = new();
    public DashboardRateKpiDto Abandoned { get; init; } = new();
    public DashboardRateKpiDto Recovery { get; init; } = new();
}

public sealed class DashboardTrendPointDto
{
    public string Label { get; init; } = string.Empty;
    public DateOnly BucketStart { get; init; }
    public int Sent { get; init; }
    public int Opened { get; init; }
    public int Clicked { get; init; }
    public int Compromised { get; init; }
    public int EducationViewed { get; init; }
    public int Trained { get; init; }
}

public sealed class DashboardRecentCampaignDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public int Sent { get; init; }
    public int OpenedTotal { get; init; }
    public double OpenRate { get; init; }
    public int OpenedWithoutClickTotal { get; init; }
    public double OpenedWithoutClickRate { get; init; }
    public double ClickRate { get; init; }
    public double CompromiseRate { get; init; }
    public double EducationViewRate { get; init; }
    public int EducationAbandonmentTotal { get; init; }
    public double EducationAbandonmentRate { get; init; }
    public int TrainedTotal { get; init; }
    public double TrainingCompletionRate { get; init; }
    public double TrainingRate { get; init; }
}
