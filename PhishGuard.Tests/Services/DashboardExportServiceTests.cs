using System.Text;
using PhishGuard.Backend.DTOs.Dashboard;
using PhishGuard.Backend.Services;
using QuestPDF.Infrastructure;

namespace PhishGuard.Tests.Services;

public class DashboardExportServiceTests
{
    static DashboardExportServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Fact]
    public void Generate_PdfProduzDocumentoValido()
    {
        var service = new DashboardExportService();

        var result = service.Generate(
            DashboardExportFormat.Pdf,
            CriarDashboard("Campanha de conscientização com um nome corporativo extenso", 10),
            CriarContexto());
        Assert.Equal("application/pdf", result.ContentType);
        Assert.EndsWith(".pdf", result.FileName);
        Assert.True(result.Content.Length > 1_000);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(result.Content, 0, 4));
    }

    [Fact]
    public void Generate_PdfSemCampanhasMantemAsTresSecoes()
    {
        var service = new DashboardExportService();

        var result = service.Generate(
            DashboardExportFormat.Pdf,
            CriarDashboard(campaignCount: 0),
            CriarContexto());

        Assert.Equal("application/pdf", result.ContentType);
        Assert.True(result.Content.Length > 1_000);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(result.Content, 0, 4));
    }

    [Fact]
    public void BuildOperationalTrendSvg_DelegaLegendaAoQuestPdf()
    {
        var trend = CriarDashboard().Trend;
        var svg = DashboardExportService.BuildOperationalTrendSvg(trend);
        var legend = DashboardExportService.BuildOperationalTrendLegend(trend);

        Assert.Collection(legend,
            item => Assert.Equal(("Enviados", 10, 100d), (item.Label, item.Total, item.Percentage)),
            item => Assert.Equal(("Abertos", 8, 80d), (item.Label, item.Total, item.Percentage)),
            item => Assert.Equal(("Clicados", 5, 50d), (item.Label, item.Total, item.Percentage)),
            item => Assert.Equal(("Comprometidos", 3, 30d), (item.Label, item.Total, item.Percentage)));
        Assert.DoesNotContain(">Enviados</text>", svg);
        Assert.DoesNotContain("<circle", svg);
    }

    [Fact]
    public void BuildEducationTrendSvg_DelegaLegendaAoQuestPdf()
    {
        var trend = CriarDashboard().Trend;
        var svg = DashboardExportService.BuildEducationTrendSvg(trend);
        var legend = DashboardExportService.BuildEducationTrendLegend(trend);

        Assert.Collection(legend,
            item => Assert.Equal(("Comprometidos", 3, 30d), (item.Label, item.Total, item.Percentage)),
            item => Assert.Equal(("Acesso educacional", 3, 30d), (item.Label, item.Total, item.Percentage)),
            item => Assert.Equal(("Treinamento concluído", 2, 20d), (item.Label, item.Total, item.Percentage)));
        Assert.DoesNotContain(">Acesso educacional</text>", svg);
        Assert.DoesNotContain("<circle", svg);
    }

    [Fact]
    public void BuildTrendLegend_SemEnvios_RetornaTotaisEPercentuaisZerados()
    {
        var legend = DashboardExportService.BuildOperationalTrendLegend([]);

        Assert.All(legend, item =>
        {
            Assert.Equal(0, item.Total);
            Assert.Equal(0, item.Percentage);
        });
    }

    [Fact]
    public void Generate_CsvEscapaFormulaEIncluiBomUtf8()
    {
        var service = new DashboardExportService();
        var dashboard = CriarDashboard("=HYPERLINK(\"https://exemplo.test\")");

        var result = service.Generate(
            DashboardExportFormat.Csv,
            dashboard,
            CriarContexto(),
            DashboardCsvDataset.Campaigns);
        var csv = Encoding.UTF8.GetString(result.Content);

        Assert.Equal("text/csv; charset=utf-8", result.ContentType);
        Assert.Equal(0xEF, result.Content[0]);
        Assert.Equal(0xBB, result.Content[1]);
        Assert.Equal(0xBF, result.Content[2]);
        Assert.Contains("Campanha", csv);
        Assert.DoesNotContain(";=HYPERLINK", csv);
        Assert.Contains("'=HYPERLINK", csv);
    }

    [Theory]
    [InlineData(DashboardCsvDataset.Summary, "Taxa de abertura")]
    [InlineData(DashboardCsvDataset.Trend, "Acesso educacional")]
    public void Generate_CsvDisponibilizaDatasetsExecutivos(
        DashboardCsvDataset dataset,
        string expectedHeader)
    {
        var service = new DashboardExportService();

        var result = service.Generate(
            DashboardExportFormat.Csv,
            CriarDashboard(),
            CriarContexto(),
            dataset);

        Assert.Contains(expectedHeader, Encoding.UTF8.GetString(result.Content));
    }

    private static DashboardReportContext CriarContexto() => new()
    {
        DepartmentLabel = "Segurança",
        GeneratedAt = new DateTimeOffset(2026, 8, 5, 9, 30, 0, TimeSpan.FromHours(-3)),
        Identity = new DashboardReportIdentityDto
        {
            CompanyName = "Empresa Segura Ltda.",
            Cnpj = "12345678000190",
            AdministratorName = "Maria Administradora"
        }
    };

    private static DashboardOverviewResponse CriarDashboard(
        string campaignName = "Simulação Agosto",
        int campaignCount = 1) => new()
    {
        Tenant = new DashboardTenantDto { Name = "Tenant Teste" },
        Period = new DashboardPeriodDto
        {
            Label = "Últimos 30 dias",
            Start = new DateOnly(2026, 7, 7),
            End = new DateOnly(2026, 8, 5)
        },
        Scope = new DashboardScopeDto
        {
            CampaignCount = campaignCount,
            UniqueTargetCount = 10,
            CampaignTargetCount = 10
        },
        Kpis = new DashboardKpisDto
        {
            Sent = new DashboardSentKpiDto { Total = 10, DeltaPercent = 25 },
            OpenRate = new DashboardOpenRateKpiDto
            {
                Rate = 80,
                UniqueTotal = 8,
                ObservedTotal = 7,
                InferredTotal = 1
            },
            ClickRate = new DashboardRateKpiDto { Rate = 50, UniqueTotal = 5 },
            CompromiseRate = new DashboardRateKpiDto { Rate = 30, UniqueTotal = 3 },
            TrainingRate = new DashboardRateKpiDto { Rate = 66.7, UniqueTotal = 2 }
        },
        TrainingEffectiveness = new DashboardTrainingEffectivenessDto
        {
            Compromised = new DashboardRateKpiDto { Rate = 30, UniqueTotal = 3 },
            EducationViewed = new DashboardRateKpiDto { Rate = 100, UniqueTotal = 3 },
            Completed = new DashboardRateKpiDto { Rate = 66.7, UniqueTotal = 2 },
            Abandoned = new DashboardRateKpiDto { Rate = 33.3, UniqueTotal = 1 },
            Recovery = new DashboardRateKpiDto { Rate = 66.7, UniqueTotal = 2 }
        },
        Trend =
        [
            new DashboardTrendPointDto
            {
                Label = "03/08",
                BucketStart = new DateOnly(2026, 8, 3),
                Sent = 4,
                Opened = 2,
                Clicked = 1,
                Compromised = 1,
                EducationViewed = 1,
                Trained = 0
            },
            new DashboardTrendPointDto
            {
                Label = "04/08",
                BucketStart = new DateOnly(2026, 8, 4),
                Sent = 7,
                Opened = 5,
                Clicked = 3,
                Compromised = 2,
                EducationViewed = 2,
                Trained = 1
            },
            new DashboardTrendPointDto
            {
                Label = "05/08",
                BucketStart = new DateOnly(2026, 8, 5),
                Sent = 10,
                Opened = 8,
                Clicked = 5,
                Compromised = 3,
                EducationViewed = 3,
                Trained = 2
            }
        ],
        RecentCampaigns = Enumerable.Range(1, campaignCount)
            .Select(index => new DashboardRecentCampaignDto
            {
                Id = Guid.NewGuid(),
                Name = campaignCount == 1 ? campaignName : $"{campaignName} {index:D2}",
                Status = "Em andamento",
                Date = new DateTime(2026, 8, 5),
                Sent = 10,
                OpenedTotal = 8,
                OpenRate = 80,
                OpenedWithoutClickTotal = 3,
                OpenedWithoutClickRate = 30,
                ClickRate = 50,
                CompromiseRate = 30,
                EducationViewRate = 30,
                TrainedTotal = 2,
                TrainingCompletionRate = 66.7,
                EducationAbandonmentTotal = 1,
                EducationAbandonmentRate = 33.3
            })
            .ToList()
    };
}
