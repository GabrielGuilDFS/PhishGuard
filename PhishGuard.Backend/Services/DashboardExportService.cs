using System.Globalization;
using System.Security;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using PhishGuard.Backend.DTOs.Dashboard;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PhishGuard.Backend.Services;

public interface IDashboardExportService
{
    DashboardExportFile Generate(
        DashboardExportFormat format,
        DashboardOverviewResponse dashboard,
        DashboardReportContext context,
        DashboardCsvDataset dataset = DashboardCsvDataset.Campaigns);
}

public sealed class DashboardExportService : IDashboardExportService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private const string Primary = "#1814C8";
    private const string Success = "#10B981";
    private const string Warning = "#F59E0B";
    private const string Danger = "#F43F5E";
    private const string Info = "#0284C7";
    private const string Muted = "#64748B";
    private const string Border = "#D8E0F0";
    private sealed record TrendSeries(
        string Label,
        string Color,
        Func<DashboardTrendPointDto, int> Value);

    public DashboardExportFile Generate(
        DashboardExportFormat format,
        DashboardOverviewResponse dashboard,
        DashboardReportContext context,
        DashboardCsvDataset dataset = DashboardCsvDataset.Campaigns)
    {
        var suffix = BuildFileSuffix(context.DepartmentLabel, context.GeneratedAt);
        return format switch
        {
            DashboardExportFormat.Pdf => new DashboardExportFile(
                GeneratePdf(dashboard, context),
                "application/pdf",
                $"phishguard-dashboard-{suffix}.pdf"),
            DashboardExportFormat.Csv => new DashboardExportFile(
                GenerateCsv(dashboard, context, dataset),
                "text/csv; charset=utf-8",
                $"phishguard-dashboard-{dataset.ToString().ToLowerInvariant()}-{suffix}.csv"),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private static byte[] GeneratePdf(DashboardOverviewResponse data, DashboardReportContext context)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontSize(9).FontColor("#0F172A"));
                page.Header().Element(container => ComposeHeader(container, "Relatório de Segurança"));

                page.Content().PaddingVertical(18).Column(column =>
                {
                    column.Spacing(14);
                    column.Item().Element(container => ComposeMetadata(container, data, context));
                    column.Item().Element(container => ComposeExecutiveSummary(container, data));
                    column.Item().Text("Indicadores gerais").Bold().FontSize(13);
                    column.Item().Element(container => ComposeKpis(container, data));
                    column.Item().Element(container => ComposePrivacyNote(container));
                });
                page.Footer().Element(container => ComposeFooter(container, context));
            });

            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontSize(9).FontColor("#0F172A"));
                page.Header().Element(container => ComposeHeader(container, "Análise de Tendências"));

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text("Tendência operacional").Bold().FontSize(13);
                    column.Item().Text("Evolução acumulada de envios, aberturas, cliques e comprometimentos no recorte selecionado.")
                        .FontSize(8).FontColor(Muted);
                    column.Item().Height(142).Svg(BuildOperationalTrendSvg(data.Trend));

                    column.Item().Text("Jornada educacional").Bold().FontSize(13);
                    column.Item().Text("Comparação entre comprometimentos, acessos ao conteúdo educativo e conclusões de treinamento.")
                        .FontSize(8).FontColor(Muted);
                    column.Item().Height(126).Svg(BuildEducationTrendSvg(data.Trend));

                    column.Item().Text("Efetividade do treinamento").Bold().FontSize(13);
                    column.Item().Element(container => ComposeTraining(container, data));
                    column.Item().Element(container => ComposeMethodology(container));
                });
                page.Footer().Element(container => ComposeFooter(container, context));
            });

            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontSize(8).FontColor("#0F172A"));
                page.Header().Element(container => ComposeHeader(container, "Campanhas Recentes"));

                page.Content().PaddingVertical(14).Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text("Detalhamento de campanhas recentes").Bold().FontSize(14);
                    column.Item().Text(
                        $"Período: {data.Period.Label} | Departamento: {context.DepartmentLabel} | {FormatCount(data.Scope.CampaignCount, "campanha", "campanhas")} no recorte")
                        .FontSize(8).FontColor(Muted);
                    column.Item().Element(container => ComposeCampaignsTable(container, data.RecentCampaigns));
                    column.Item().Text(
                        "As taxas respeitam o mesmo período, departamento e tenant usados na tela do dashboard.")
                        .FontSize(7).FontColor(Muted);
                });
                page.Footer().Element(container => ComposeFooter(container, context));
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, string section)
    {
        container.Column(header =>
        {
            header.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Phish").Bold().FontSize(18).FontColor("#0F172A");
                    text.Span("Guard").Bold().FontSize(18).FontColor(Primary);
                });
                row.RelativeItem().AlignRight().Text(section)
                    .Bold().FontSize(13).FontColor(Primary);
            });
            header.Item().PaddingTop(5).BorderBottom(1).BorderColor(Border);
        });
    }

    private static void ComposeFooter(IContainer container, DashboardReportContext context)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text($"Gerado em {context.GeneratedAt:dd/MM/yyyy HH:mm}")
                .FontSize(7).FontColor(Muted);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.DefaultTextStyle(style => style.FontSize(7).FontColor(Muted));
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }

    private static void ComposeMetadata(IContainer container, DashboardOverviewResponse data, DashboardReportContext context)
    {
        container.Background("#F8FAFC").Border(1).BorderColor(Border).Padding(12).Column(column =>
        {
            column.Spacing(6);
            column.Item().Text(SafeReportText(context.Identity.CompanyName, 120)).Bold().FontSize(15);
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text($"CNPJ: {FormatCnpj(context.Identity.Cnpj)}");
                    left.Item().Text($"Responsável pela emissão: {SafeReportText(context.Identity.AdministratorName, 100)}");
                });
                row.RelativeItem().Column(right =>
                {
                    right.Item().Text($"Período: {data.Period.Label} ({data.Period.Start:dd/MM/yyyy} a {data.Period.End:dd/MM/yyyy})");
                    right.Item().Text($"Departamento: {context.DepartmentLabel}");
                });
            });
            column.Item().Text($"Escopo: {FormatCount(data.Scope.CampaignCount, "campanha", "campanhas")} · {FormatInteger(data.Scope.UniqueTargetCount)} destinatários únicos · {FormatInteger(data.Scope.CampaignTargetCount)} e-mails enviados")
                .FontColor(Muted);
        });
    }

    private static void ComposeExecutiveSummary(IContainer container, DashboardOverviewResponse data)
    {
        container.BorderLeft(4).BorderColor(Primary).PaddingLeft(10).Column(column =>
        {
            column.Item().Text("Resumo executivo").Bold().FontSize(12).FontColor(Primary);
            column.Item().PaddingTop(4).Text(BuildNarrative(data)).LineHeight(1.35f);
        });
    }

    private static void ComposeKpis(IContainer container, DashboardOverviewResponse data)
    {
        container.Column(column =>
        {
            column.Spacing(7);
            column.Item().Row(row =>
            {
                var sentComparison = data.Kpis.Sent.DeltaPercent.HasValue
                    ? $"{(data.Kpis.Sent.DeltaPercent >= 0 ? "+" : string.Empty)}{FormatPercent(data.Kpis.Sent.DeltaPercent.Value)} vs. ciclo anterior"
                    : "Sem ciclo anterior comparável";
                Kpi(row.RelativeItem(), "E-mails enviados", FormatInteger(data.Kpis.Sent.Total), Primary, sentComparison);
                row.Spacing(7);
                Kpi(row.RelativeItem(), "Taxa de abertura", FormatPercent(data.Kpis.OpenRate.Rate), Success,
                    $"{FormatInteger(data.Kpis.OpenRate.UniqueTotal)} aberturas efetivas");
                row.Spacing(7);
                Kpi(row.RelativeItem(), "Taxa de clique", FormatPercent(data.Kpis.ClickRate.Rate), Warning,
                    $"{FormatInteger(data.Kpis.ClickRate.UniqueTotal)} cliques únicos");
            });
            column.Item().Row(row =>
            {
                Kpi(row.RelativeItem(), "Comprometimento", FormatPercent(data.Kpis.CompromiseRate.Rate), Danger,
                    $"{FormatInteger(data.Kpis.CompromiseRate.UniqueTotal)} submissões registradas");
                row.Spacing(7);
                Kpi(row.RelativeItem(), "Aprendizado", FormatPercent(data.Kpis.TrainingRate.Rate), Info,
                    $"{FormatInteger(data.Kpis.TrainingRate.UniqueTotal)} conclusões");
                row.Spacing(7);
                Kpi(row.RelativeItem(), "Pixel / inferidas",
                    $"{FormatInteger(data.Kpis.OpenRate.ObservedTotal)} / {FormatInteger(data.Kpis.OpenRate.InferredTotal)}",
                    Success,
                    "Origem das aberturas efetivas");
            });
        });
    }

    private static void Kpi(
        IContainer container,
        string label,
        string value,
        string color,
        string? note = null)
    {
        container.Border(1).BorderColor(Border).Padding(9).Column(column =>
        {
            column.Item().Text(label).FontSize(8).FontColor(Muted);
            column.Item().PaddingTop(3).Text(value).Bold().FontSize(15).FontColor(color);
            if (!string.IsNullOrWhiteSpace(note))
                column.Item().PaddingTop(3).Text(note).FontSize(7).FontColor(Muted);
        });
    }

    private static void ComposeTraining(IContainer container, DashboardOverviewResponse data)
    {
        var metrics = data.TrainingEffectiveness;
        container.Column(column =>
        {
            column.Spacing(7);
            column.Item().Row(row =>
            {
                Kpi(row.RelativeItem(), "Comprometidos", FormatPercent(metrics.Compromised.Rate), Danger);
                row.Spacing(7);
                Kpi(row.RelativeItem(), "Acesso educacional", FormatPercent(metrics.EducationViewed.Rate), Info);
                row.Spacing(7);
                Kpi(row.RelativeItem(), "Conclusão", FormatPercent(metrics.Completed.Rate), Success);
                row.Spacing(7);
                Kpi(row.RelativeItem(), "Abandono", FormatPercent(metrics.Abandoned.Rate), Warning);
            });
            column.Item().Text($"Recuperação após comprometimento: {FormatPercent(metrics.Recovery.Rate)} ({FormatInteger(metrics.Recovery.UniqueTotal)} conclusões entre comprometidos).")
                .FontSize(8).FontColor(Muted);
        });
    }

    private static void ComposePrivacyNote(IContainer container)
    {
        container.Background("#F8FAFC").Padding(10).Text(
            "Este relatório apresenta métricas agregadas. Não contém nomes ou e-mails de participantes, credenciais, valores submetidos nas páginas simuladas, endereços IP ou configurações internas do tenant.")
            .FontSize(8).FontColor(Muted);
    }

    private static void ComposeMethodology(IContainer container)
    {
        container.Background("#F8FAFC").Border(1).BorderColor(Border).Padding(10).Column(column =>
        {
            column.Spacing(3);
            column.Item().Text("Metodologia resumida").Bold().FontSize(10).FontColor(Primary);
            column.Item().Text("- Eventos são deduplicados pelo conjunto campanha, destinatário e ação.").FontSize(8);
            column.Item().Text("- A abertura efetiva combina o pixel observado com aberturas inferidas por ações posteriores, sem duplicar destinatários.").FontSize(8);
            column.Item().Text("- Clique e comprometimento usam os e-mails enviados no recorte como base; abandono usa os acessos educacionais.").FontSize(8);
            column.Item().Text("- Recuperação representa participantes comprometidos que concluíram o treinamento.").FontSize(8);
        });
    }

    private static void ComposeCampaignsTable(IContainer container, IReadOnlyList<DashboardRecentCampaignDto> campaigns)
    {
        if (campaigns.Count == 0)
        {
            container.Background("#F8FAFC").Padding(18).AlignCenter()
                .Text("Nenhuma campanha com envios no período selecionado.").FontColor(Muted);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.4f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(0.7f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(0.8f);
                columns.RelativeColumn(1.0f);
                columns.RelativeColumn(1.0f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(1.1f);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Campanha");
                HeaderCell(header.Cell(), "Status");
                HeaderCell(header.Cell(), "Data");
                HeaderCell(header.Cell(), "Envios");
                HeaderCell(header.Cell(), "Abertura");
                HeaderCell(header.Cell(), "Sem clique");
                HeaderCell(header.Cell(), "Clique");
                HeaderCell(header.Cell(), "Comprom.");
                HeaderCell(header.Cell(), "Educacional");
                HeaderCell(header.Cell(), "Conclusão");
                HeaderCell(header.Cell(), "Abandono");
            });

            foreach (var campaign in campaigns)
            {
                BodyCell(table.Cell(), SafeReportText(campaign.Name, 150), true);
                BodyCell(table.Cell(), SafeReportText(campaign.Status, 30));
                BodyCell(table.Cell(), campaign.Date.ToString("dd/MM/yyyy"));
                BodyCell(table.Cell(), FormatInteger(campaign.Sent));
                BodyCell(table.Cell(), FormatPercent(campaign.OpenRate));
                BodyCell(table.Cell(), $"{FormatInteger(campaign.OpenedWithoutClickTotal)} ({FormatPercent(campaign.OpenedWithoutClickRate)})");
                BodyCell(table.Cell(), FormatPercent(campaign.ClickRate));
                BodyCell(table.Cell(), FormatPercent(campaign.CompromiseRate));
                BodyCell(table.Cell(), FormatPercent(campaign.EducationViewRate));
                BodyCell(table.Cell(), $"{FormatInteger(campaign.TrainedTotal)} ({FormatPercent(campaign.TrainingCompletionRate)})");
                BodyCell(table.Cell(), $"{FormatInteger(campaign.EducationAbandonmentTotal)} ({FormatPercent(campaign.EducationAbandonmentRate)})");
            }
        });
    }

    private static void HeaderCell(IContainer container, string text) =>
        container.Background(Primary).PaddingVertical(6).PaddingHorizontal(4)
            .AlignCenter().Text(text).Bold().FontSize(6.5f).FontColor(Colors.White);

    private static void BodyCell(IContainer container, string text, bool alignLeft = false)
    {
        var cell = container.BorderBottom(1).BorderColor(Border).PaddingVertical(6).PaddingHorizontal(4);
        if (!alignLeft) cell = cell.AlignCenter();
        cell.Text(text).FontSize(6.5f);
    }

    private static string BuildOperationalTrendSvg(IReadOnlyList<DashboardTrendPointDto> trend) =>
        BuildTrendSvg(trend,
        [
            new("Enviados", Primary, point => point.Sent),
            new("Abertos", Success, point => point.Opened),
            new("Clicados", Warning, point => point.Clicked),
            new("Comprometidos", Danger, point => point.Compromised)
        ]);

    private static string BuildEducationTrendSvg(IReadOnlyList<DashboardTrendPointDto> trend) =>
        BuildTrendSvg(trend,
        [
            new("Comprometidos", Danger, point => point.Compromised),
            new("Acesso educacional", Info, point => point.EducationViewed),
            new("Treinamento concluído", Success, point => point.Trained)
        ]);

    private static string BuildTrendSvg(
        IReadOnlyList<DashboardTrendPointDto> trend,
        IReadOnlyList<TrendSeries> series)
    {
        const int width = 720;
        const int height = 190;
        const int left = 36;
        const int right = 14;
        const int top = 12;
        const int bottom = 32;
        var plotWidth = width - left - right;
        var plotHeight = height - top - bottom;
        var max = Math.Max(1, trend
            .SelectMany(point => series.Select(item => item.Value(point)))
            .DefaultIfEmpty(0)
            .Max());
        var builder = new StringBuilder($"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'>");
        builder.Append("<rect width='100%' height='100%' fill='#ffffff'/>");

        var divisions = Math.Min(4, max);
        for (var i = 0; i <= divisions; i++)
        {
            var y = top + plotHeight * i / (double)divisions;
            var value = Math.Round(max * (divisions - i) / (double)divisions);
            builder.Append(CultureInfo.InvariantCulture, $"<line x1='{left}' y1='{y:0.##}' x2='{width - right}' y2='{y:0.##}' stroke='#E2E8F0' stroke-width='1'/>");
            builder.Append(CultureInfo.InvariantCulture, $"<text x='{left - 5}' y='{y + 3:0.##}' text-anchor='end' font-size='8' fill='{Muted}'>{value}</text>");
        }

        foreach (var item in series)
            AppendSeries(builder, trend, item.Value, item.Color, left, top, plotWidth, plotHeight, max);

        if (trend.Count > 0)
        {
            var labelIndexes = new[] { 0, trend.Count / 2, trend.Count - 1 }.Distinct();
            foreach (var index in labelIndexes)
            {
                var x = left + (trend.Count == 1 ? plotWidth / 2.0 : plotWidth * index / (trend.Count - 1.0));
                builder.Append(CultureInfo.InvariantCulture, $"<text x='{x:0.##}' y='{height - 17}' text-anchor='middle' font-size='8' fill='{Muted}'>{SecurityElement.Escape(trend[index].Label)}</text>");
            }
        }

        var legendX = left;
        var legendSpacing = plotWidth / Math.Max(1, series.Count);
        foreach (var item in series)
        {
            builder.Append($"<circle cx='{legendX}' cy='{height - 5}' r='3' fill='{item.Color}'/><text x='{legendX + 6}' y='{height - 2}' font-size='7' fill='{Muted}'>{SecurityElement.Escape(item.Label)}</text>");
            legendX += legendSpacing;
        }

        builder.Append("</svg>");
        return builder.ToString();
    }

    private static void AppendSeries(
        StringBuilder builder,
        IReadOnlyList<DashboardTrendPointDto> trend,
        Func<DashboardTrendPointDto, int> selector,
        string color,
        int left,
        int top,
        int plotWidth,
        int plotHeight,
        int max)
    {
        if (trend.Count == 0) return;
        var points = trend.Select((point, index) =>
        {
            var x = left + (trend.Count == 1 ? plotWidth / 2.0 : plotWidth * index / (trend.Count - 1.0));
            var y = top + plotHeight - plotHeight * selector(point) / (double)max;
            return FormattableString.Invariant($"{x:0.##},{y:0.##}");
        }).ToArray();

        if (points.Length == 1)
        {
            var coordinates = points[0].Split(',');
            builder.Append($"<circle cx='{coordinates[0]}' cy='{coordinates[1]}' r='3' fill='{color}'/>");
            return;
        }

        builder.Append($"<polyline points='{string.Join(' ', points)}' fill='none' stroke='{color}' stroke-width='2.2' stroke-linejoin='round'/>");
    }

    private static byte[] GenerateCsv(
        DashboardOverviewResponse data,
        DashboardReportContext context,
        DashboardCsvDataset dataset)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true);
        var configuration = new CsvConfiguration(PtBr)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            InjectionOptions = InjectionOptions.Escape,
            NewLine = "\r\n"
        };
        using var csv = new CsvWriter(writer, configuration);

        switch (dataset)
        {
            case DashboardCsvDataset.Summary:
                WriteSummaryCsv(csv, data, context);
                break;
            case DashboardCsvDataset.Trend:
                WriteTrendCsv(csv, data, context);
                break;
            default:
                WriteCampaignsCsv(csv, data, context);
                break;
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static void WriteSummaryCsv(CsvWriter csv, DashboardOverviewResponse data, DashboardReportContext context)
    {
        WriteHeader(csv, "Tenant", "Período", "Departamento", "Gerado em", "Campanhas", "Destinatários únicos", "E-mails enviados", "Taxa de abertura", "Taxa de clique", "Taxa de comprometimento", "Acesso educacional", "Conclusão", "Abandono", "Recuperação");
        WriteRow(csv, data.Tenant.Name, data.Period.Label, context.DepartmentLabel, context.GeneratedAt.ToString("O"), data.Scope.CampaignCount, data.Scope.UniqueTargetCount, data.Kpis.Sent.Total, data.Kpis.OpenRate.Rate, data.Kpis.ClickRate.Rate, data.Kpis.CompromiseRate.Rate, data.TrainingEffectiveness.EducationViewed.Rate, data.TrainingEffectiveness.Completed.Rate, data.TrainingEffectiveness.Abandoned.Rate, data.TrainingEffectiveness.Recovery.Rate);
    }

    private static void WriteTrendCsv(CsvWriter csv, DashboardOverviewResponse data, DashboardReportContext context)
    {
        WriteHeader(csv, "Tenant", "Período", "Departamento", "Data", "Enviados", "Abertos", "Clicados", "Comprometidos", "Acesso educacional", "Treinamentos concluídos");
        foreach (var point in data.Trend)
            WriteRow(csv, data.Tenant.Name, data.Period.Label, context.DepartmentLabel, point.BucketStart.ToString("yyyy-MM-dd"), point.Sent, point.Opened, point.Clicked, point.Compromised, point.EducationViewed, point.Trained);
    }

    private static void WriteCampaignsCsv(CsvWriter csv, DashboardOverviewResponse data, DashboardReportContext context)
    {
        WriteHeader(csv, "Tenant", "Período", "Departamento", "Campanha", "Status", "Data", "Enviados", "Abertos", "Taxa de abertura", "Abertos sem clicar", "Taxa sem clique", "Taxa de clique", "Taxa de comprometimento", "Taxa de acesso educacional", "Treinamentos concluídos", "Taxa de conclusão", "Abandono educacional", "Taxa de abandono");
        foreach (var campaign in data.RecentCampaigns)
            WriteRow(csv, data.Tenant.Name, data.Period.Label, context.DepartmentLabel, campaign.Name, campaign.Status, campaign.Date.ToString("O"), campaign.Sent, campaign.OpenedTotal, campaign.OpenRate, campaign.OpenedWithoutClickTotal, campaign.OpenedWithoutClickRate, campaign.ClickRate, campaign.CompromiseRate, campaign.EducationViewRate, campaign.TrainedTotal, campaign.TrainingCompletionRate, campaign.EducationAbandonmentTotal, campaign.EducationAbandonmentRate);
    }

    private static void WriteHeader(CsvWriter csv, params string[] fields)
    {
        foreach (var field in fields) csv.WriteField(field);
        csv.NextRecord();
    }

    private static void WriteRow(CsvWriter csv, params object?[] fields)
    {
        foreach (var field in fields) csv.WriteField(field);
        csv.NextRecord();
    }

    private static string BuildNarrative(DashboardOverviewResponse data)
    {
        if (data.Kpis.Sent.Total == 0)
            return "Não houve e-mails enviados no período e departamento selecionados. As taxas permanecem zeradas até que uma campanha produza eventos dentro deste recorte.";

        var builder = new StringBuilder();
        builder.Append($"No período analisado foram enviados {FormatInteger(data.Kpis.Sent.Total)} e-mails em {FormatCount(data.Scope.CampaignCount, "campanha", "campanhas")}. ");
        builder.Append($"A abertura efetiva foi de {FormatPercent(data.Kpis.OpenRate.Rate)}, a taxa de clique foi de {FormatPercent(data.Kpis.ClickRate.Rate)} e o comprometimento atingiu {FormatPercent(data.Kpis.CompromiseRate.Rate)}. ");
        builder.Append($"{FormatInteger(data.TrainingEffectiveness.EducationViewed.UniqueTotal)} participantes acessaram o conteúdo educacional e {FormatInteger(data.TrainingEffectiveness.Completed.UniqueTotal)} concluíram o treinamento. ");
        builder.Append($"O abandono educacional foi de {FormatPercent(data.TrainingEffectiveness.Abandoned.Rate)} e a recuperação após comprometimento foi de {FormatPercent(data.TrainingEffectiveness.Recovery.Rate)}.");
        return builder.ToString();
    }

    private static string BuildFileSuffix(string department, DateTimeOffset generatedAt)
    {
        var normalized = string.IsNullOrWhiteSpace(department) ? "Todos" : department.Trim();
        var safe = new string(normalized
            .Where(character => char.IsLetterOrDigit(character) || character is '-' or '_' or ' ')
            .Take(40)
            .ToArray())
            .Trim()
            .Replace(' ', '-');
        if (string.IsNullOrWhiteSpace(safe)) safe = "Todos";
        return $"{safe}-{generatedAt:yyyyMMdd-HHmm}";
    }

    private static string FormatInteger(int value) => value.ToString("N0", PtBr);
    private static string FormatPercent(double value) => value.ToString("N1", PtBr) + "%";
    private static string FormatCount(int value, string singular, string plural) =>
        $"{FormatInteger(value)} {(value == 1 ? singular : plural)}";

    private static string FormatCnpj(string value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 14
            ? $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..12]}-{digits[12..]}"
            : "Não informado";
    }

    private static string SafeReportText(string value, int maxLength)
    {
        var normalized = string.Join(' ', (value ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length <= maxLength) return normalized;
        return normalized[..Math.Max(1, maxLength - 1)] + "…";
    }
}
