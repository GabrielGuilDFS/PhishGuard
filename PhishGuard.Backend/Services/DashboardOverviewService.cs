using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs.Dashboard;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services;

public interface IDashboardOverviewService
{
    Task<DashboardOverviewResponse> GetOverviewAsync(
        Guid tenantId,
        string period,
        string? department,
        CancellationToken cancellationToken = default);
}

public sealed class DashboardQueryException(string message) : Exception(message);

public sealed class DashboardOverviewService : IDashboardOverviewService
{
    private readonly AppDbContext _context;
    private readonly DashboardReportingTime _reportingTime;

    public DashboardOverviewService(AppDbContext context, DashboardReportingTime reportingTime)
    {
        _context = context;
        _reportingTime = reportingTime;
    }

    public async Task<DashboardOverviewResponse> GetOverviewAsync(
        Guid tenantId,
        string period,
        string? department,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolvePeriod(period, out var days, out var periodLabel))
            throw new DashboardQueryException("Período inválido. Use 7d, 30d ou 90d.");

        var departmentFilter = NormalizeDepartmentFilter(department);
        if (departmentFilter is { Length: > 80 })
            throw new DashboardQueryException("Departamento inválido.");

        var departments = await _context.Targets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Select(t => t.Departamento)
            .Distinct()
            .ToListAsync(cancellationToken);

        var availableDepartments = departments
            .Select(d => d?.Trim())
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(d => d, StringComparer.Create(new CultureInfo("pt-BR"), ignoreCase: true))
            .ToList();

        string[]? matchingDepartments = null;
        if (departmentFilter is not null)
        {
            matchingDepartments = departments
                .Where(d => !string.IsNullOrWhiteSpace(d)
                    && string.Equals(d.Trim(), departmentFilter, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (matchingDepartments.Length == 0)
                throw new DashboardQueryException("Departamento não encontrado para este tenant.");
        }

        var nowUtc = _reportingTime.UtcNow.UtcDateTime;
        var nowLocal = _reportingTime.LocalNow;
        var endDate = DateOnly.FromDateTime(nowLocal.DateTime);
        var startDate = endDate.AddDays(-(days - 1));
        var previousStartDate = startDate.AddDays(-days);

        var startUtc = _reportingTime.StartOfDayUtc(startDate);
        var previousStartUtc = _reportingTime.StartOfDayUtc(previousStartDate);
        // Mantém a comparação com o ciclo anterior na mesma hora civil do ciclo atual.
        var previousEndUtc = _reportingTime.ToUtc(nowLocal.DateTime.AddDays(-days));

        var logsQuery = _context.SimulationsLogs
            .AsNoTracking()
            .Where(l => l.TenantId == tenantId
                && l.DataHora >= previousStartUtc
                && l.DataHora <= nowUtc
                && (l.Acao == SimulationActions.Envio
                    || l.Acao == SimulationActions.Abertura
                    || l.Acao == SimulationActions.Clique
                    || l.Acao == SimulationActions.Submissao
                    || l.Acao == SimulationActions.PaginaEducacionalVisualizada
                    || l.Acao == SimulationActions.TreinamentoConcluido));

        if (matchingDepartments is not null)
        {
            var filteredTargets = _context.Targets.Where(t => t.TenantId == tenantId
                && matchingDepartments.Contains(t.Departamento));
            logsQuery = logsQuery.Where(l => filteredTargets.Any(t => t.Id == l.TargetId));
        }

        // O ciclo anterior só participa do delta de envios: conte no banco.
        var previousSentCount = await logsQuery
            .Where(l => l.DataHora <= previousEndUtc && l.Acao == SimulationActions.Envio)
            .Select(l => new { l.CampaignId, l.TargetId }).Distinct()
            .CountAsync(cancellationToken);

        // Uma ocorrência por ação/par é suficiente para todos os KPIs e para a
        // primeira aparição no gráfico cumulativo. Não materialize logs anteriores.
        var currentLogs = await logsQuery.Where(l => l.DataHora >= startUtc)
            .GroupBy(l => new { l.CampaignId, l.TargetId, l.Acao })
            .Select(g => new DashboardLog(g.Key.CampaignId, g.Key.TargetId, g.Key.Acao, g.Min(l => l.DataHora)))
            .ToListAsync(cancellationToken);

        var currentSent = KeysForAction(currentLogs, SimulationActions.Envio);
        var observedOpened = KeysForAction(currentLogs, SimulationActions.Abertura, currentSent);
        var clicked = KeysForAction(currentLogs, SimulationActions.Clique, currentSent);
        var compromised = KeysForAction(currentLogs, SimulationActions.Submissao, currentSent);
        var educationViewed = KeysForAction(currentLogs, SimulationActions.PaginaEducacionalVisualizada, currentSent);
        var trained = KeysForAction(currentLogs, SimulationActions.TreinamentoConcluido, currentSent);
        var opened = UnionKeys(observedOpened, clicked, compromised, trained);
        var inferredOpened = opened.Except(observedOpened).ToHashSet();
        var educationAbandoned = educationViewed.Except(trained).ToHashSet();
        var recoveredAfterCompromise = compromised.Intersect(trained).ToHashSet();

        var tenantName = await _context.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenantId)
            .Select(t => t.NomeEmpresa)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var campaignIds = currentSent.Select(k => k.CampaignId).Distinct().ToArray();
        var campaigns = await _context.Campaigns
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId && campaignIds.Contains(c.Id))
            .Select(c => new { c.Id, c.NomeCampanha, c.Status, c.DataInicio })
            .OrderByDescending(c => c.DataInicio)
            .Take(10)
            .ToListAsync(cancellationToken);

        var recentCampaigns = campaigns.Select(c =>
        {
            var sentByCampaign = currentSent.Count(k => k.CampaignId == c.Id);
            var openedByCampaign = opened.Count(k => k.CampaignId == c.Id);
            var clickedByCampaign = clicked.Count(k => k.CampaignId == c.Id);
            var openedWithoutClickByCampaign = opened.Count(k => k.CampaignId == c.Id && !clicked.Contains(k));
            var compromisedByCampaign = compromised.Count(k => k.CampaignId == c.Id);
            var educationViewedByCampaign = educationViewed.Count(k => k.CampaignId == c.Id);
            var educationAbandonedByCampaign = educationAbandoned.Count(k => k.CampaignId == c.Id);
            var trainedByCampaign = trained.Count(k => k.CampaignId == c.Id);

            return new DashboardRecentCampaignDto
            {
                Id = c.Id,
                Name = c.NomeCampanha,
                Status = c.Status,
                Date = c.DataInicio,
                Sent = sentByCampaign,
                OpenedTotal = openedByCampaign,
                OpenRate = Rate(openedByCampaign, sentByCampaign),
                OpenedWithoutClickTotal = openedWithoutClickByCampaign,
                OpenedWithoutClickRate = Rate(openedWithoutClickByCampaign, sentByCampaign),
                ClickRate = Rate(clickedByCampaign, sentByCampaign),
                CompromiseRate = Rate(compromisedByCampaign, sentByCampaign),
                EducationViewRate = Rate(educationViewedByCampaign, sentByCampaign),
                EducationAbandonmentTotal = educationAbandonedByCampaign,
                EducationAbandonmentRate = Rate(educationAbandonedByCampaign, educationViewedByCampaign),
                TrainedTotal = trainedByCampaign,
                TrainingCompletionRate = Rate(trainedByCampaign, sentByCampaign),
                TrainingRate = Rate(trainedByCampaign, clickedByCampaign)
            };
        }).ToList();

        return new DashboardOverviewResponse
        {
            Period = new DashboardPeriodDto { Start = startDate, End = endDate, Label = periodLabel },
            Tenant = new DashboardTenantDto { Name = tenantName },
            Scope = new DashboardScopeDto
            {
                CampaignCount = campaignIds.Length,
                UniqueTargetCount = currentSent.Select(key => key.TargetId).Distinct().Count(),
                CampaignTargetCount = currentSent.Count
            },
            AvailableDepartments = availableDepartments,
            Kpis = new DashboardKpisDto
            {
                Sent = new DashboardSentKpiDto
                {
                    Total = currentSent.Count,
                    DeltaPercent = DeltaPercent(currentSent.Count, previousSentCount)
                },
                OpenRate = new DashboardOpenRateKpiDto
                {
                    Rate = Rate(opened.Count, currentSent.Count),
                    UniqueTotal = opened.Count,
                    ObservedTotal = observedOpened.Count,
                    InferredTotal = inferredOpened.Count
                },
                ClickRate = new DashboardRateKpiDto { Rate = Rate(clicked.Count, currentSent.Count), UniqueTotal = clicked.Count },
                CompromiseRate = new DashboardRateKpiDto { Rate = Rate(compromised.Count, currentSent.Count), UniqueTotal = compromised.Count },
                TrainingRate = new DashboardRateKpiDto { Rate = Rate(trained.Count, clicked.Count), UniqueTotal = trained.Count }
            },
            TrainingEffectiveness = new DashboardTrainingEffectivenessDto
            {
                Compromised = new DashboardRateKpiDto { Rate = Rate(compromised.Count, currentSent.Count), UniqueTotal = compromised.Count },
                EducationViewed = new DashboardRateKpiDto { Rate = Rate(educationViewed.Count, currentSent.Count), UniqueTotal = educationViewed.Count },
                Completed = new DashboardRateKpiDto { Rate = Rate(trained.Count, currentSent.Count), UniqueTotal = trained.Count },
                Abandoned = new DashboardRateKpiDto { Rate = Rate(educationAbandoned.Count, educationViewed.Count), UniqueTotal = educationAbandoned.Count },
                Recovery = new DashboardRateKpiDto { Rate = Rate(recoveredAfterCompromise.Count, compromised.Count), UniqueTotal = recoveredAfterCompromise.Count }
            },
            Trend = BuildTrend(currentLogs, currentSent, startDate, nowUtc, days),
            RecentCampaigns = recentCampaigns
        };
    }

    private static bool TryResolvePeriod(string? period, out int days, out string label)
    {
        switch (period?.Trim().ToLowerInvariant())
        {
            case "7d": days = 7; label = "Últimos 7 dias"; return true;
            case "30d": days = 30; label = "Últimos 30 dias"; return true;
            case "90d": days = 90; label = "Últimos 90 dias"; return true;
            default: days = 0; label = string.Empty; return false;
        }
    }

    private static string? NormalizeDepartmentFilter(string? department)
    {
        if (string.IsNullOrWhiteSpace(department)) return null;
        var normalized = department.Trim();
        return normalized.Equals("todos", StringComparison.OrdinalIgnoreCase) ? null : normalized;
    }

    private static HashSet<DashboardEmailKey> KeysForAction(
        IEnumerable<DashboardLog> logs,
        string action,
        HashSet<DashboardEmailKey>? cohort = null) =>
        logs.Where(l => l.Acao == action)
            .Select(l => new DashboardEmailKey(l.CampaignId, l.TargetId))
            .Where(k => cohort is null || cohort.Contains(k))
            .ToHashSet();

    private static HashSet<DashboardEmailKey> UnionKeys(params IEnumerable<DashboardEmailKey>[] collections) =>
        collections.SelectMany(keys => keys).ToHashSet();

    private static double Rate(int numerator, int denominator) =>
        denominator == 0 ? 0 : Math.Round(100.0 * numerator / denominator, 1);

    private static double? DeltaPercent(int current, int previous) =>
        previous == 0 ? null : Math.Round(100.0 * (current - previous) / previous, 1);

    private IReadOnlyList<DashboardTrendPointDto> BuildTrend(
        IReadOnlyList<DashboardLog> logs,
        HashSet<DashboardEmailKey> sentCohort,
        DateOnly startDate,
        DateTime endUtc,
        int days)
    {
        var result = new List<DashboardTrendPointDto>();
        var bucketDays = days >= 90 ? 7 : 1;
        var culture = new CultureInfo("pt-BR");
        var cursor = startDate;
        var orderedLogs = logs.OrderBy(l => l.DataHora).ToArray();
        var index = 0;
        var sent = new HashSet<DashboardEmailKey>();
        var opened = new HashSet<DashboardEmailKey>();
        var clicked = new HashSet<DashboardEmailKey>();
        var compromised = new HashSet<DashboardEmailKey>();
        var educationViewed = new HashSet<DashboardEmailKey>();
        var trained = new HashSet<DashboardEmailKey>();

        for (var offset = 0; offset < days; offset += bucketDays)
        {
            var bucketEndDate = cursor.AddDays(bucketDays);
            var bucketEndUtc = _reportingTime.StartOfDayUtc(bucketEndDate);
            var throughUtc = bucketEndUtc <= endUtc ? bucketEndUtc : endUtc.AddTicks(1);
            while (index < orderedLogs.Length && orderedLogs[index].DataHora < throughUtc)
            {
                var log = orderedLogs[index++];
                var key = new DashboardEmailKey(log.CampaignId, log.TargetId);
                if (log.Acao == SimulationActions.Envio) sent.Add(key);
                if (!sentCohort.Contains(key)) continue;
                switch (log.Acao)
                {
                    case SimulationActions.Abertura: opened.Add(key); break;
                    case SimulationActions.Clique: clicked.Add(key); opened.Add(key); break;
                    case SimulationActions.Submissao: compromised.Add(key); opened.Add(key); break;
                    case SimulationActions.PaginaEducacionalVisualizada: educationViewed.Add(key); break;
                    case SimulationActions.TreinamentoConcluido: trained.Add(key); opened.Add(key); break;
                }
            }

            result.Add(new DashboardTrendPointDto
            {
                BucketStart = cursor,
                Label = cursor.ToString(days >= 90 ? "dd MMM" : "dd/MM", culture),
                Sent = sent.Count,
                Opened = opened.Count,
                Clicked = clicked.Count,
                Compromised = compromised.Count,
                EducationViewed = educationViewed.Count,
                Trained = trained.Count
            });

            cursor = bucketEndDate;
        }

        return result;
    }

    private sealed record DashboardLog(Guid CampaignId, Guid TargetId, string Acao, DateTime DataHora);
    private readonly record struct DashboardEmailKey(Guid CampaignId, Guid TargetId);
}
