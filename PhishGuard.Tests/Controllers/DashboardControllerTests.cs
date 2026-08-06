using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs.Dashboard;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using System.Security.Claims;

namespace PhishGuard.Tests.Controllers;

public class DashboardControllerTests
{
    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantIdAtivo { get; set; }
        public Guid GetTenantId() => TenantIdAtivo;
        public Guid GetCurrentTenantId() => TenantIdAtivo;
    }

    private sealed class CapturingDashboardExportService : IDashboardExportService
    {
        public DashboardReportContext? Context { get; private set; }

        public DashboardExportFile Generate(
            DashboardExportFormat format,
            DashboardOverviewResponse dashboard,
            DashboardReportContext context,
            DashboardCsvDataset dataset = DashboardCsvDataset.Campaigns)
        {
            Context = context;
            return new DashboardExportFile([1, 2, 3], "application/pdf", "relatorio.pdf");
        }
    }

    private static (AppDbContext Context, FakeTenantProvider TenantProvider) CriarContexto()
    {
        var provider = new FakeTenantProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return (new AppDbContext(options, provider), provider);
    }

    private static DashboardReportingTime CriarTempoRelatorio(TimeProvider? timeProvider = null) => new(
        timeProvider ?? TimeProvider.System,
        TimeZoneInfo.CreateCustomTimeZone("America/Sao_Paulo.Tests", TimeSpan.FromHours(-3), "São Paulo", "São Paulo"));

    private static Tenant NovoTenant(string nome, string cnpj) => new()
    {
        Id = Guid.NewGuid(),
        NomeEmpresa = nome,
        Cnpj = cnpj,
        Ativo = true,
        CriadoEm = DateTime.UtcNow
    };

    private static Target NovoAlvo(string nome, string departamento) => new()
    {
        Id = Guid.NewGuid(),
        Nome = nome,
        Email = $"{Guid.NewGuid():N}@teste.com",
        Departamento = departamento
    };

    private static Campaign NovaCampanha(string nome, DateTime inicio) => new()
    {
        Id = Guid.NewGuid(),
        NomeCampanha = nome,
        Status = CampaignStatus.EmAndamento,
        DataInicio = inicio,
        EmailTemplateId = Guid.NewGuid(),
        LandingPageId = Guid.NewGuid(),
        EducationalPageId = Guid.NewGuid(),
        CriadoEm = inicio
    };

    private static SimulationLog NovoLog(Campaign campaign, Target target, string acao, DateTime quando) => new()
    {
        Id = Guid.NewGuid(),
        CampaignId = campaign.Id,
        TargetId = target.Id,
        Acao = acao,
        DataHora = quando,
        IpOrigem = "127.0.0.1"
    };

    private static DashboardOverviewResponse Extrair(ActionResult<DashboardOverviewResponse> result)
    {
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        return Assert.IsType<DashboardOverviewResponse>(ok.Value);
    }

    private static DashboardController NovoController(
        AppDbContext context,
        FakeTenantProvider provider,
        Guid? administratorId = null,
        IDashboardExportService? exportService = null,
        DashboardReportingTime? reportingTime = null)
    {
        reportingTime ??= CriarTempoRelatorio();
        var controller = new DashboardController(
            context,
            provider,
            new DashboardOverviewService(context, reportingTime),
            exportService ?? new DashboardExportService(),
            reportingTime);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        if (administratorId.HasValue)
        {
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, administratorId.Value.ToString())],
                "Test"));
        }
        return controller;
    }

    [Fact]
    public async Task Overview_ViradaUtc_UsaDiaCivilDeSaoPauloESomenteTrintaPontos()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;

        var reportingTime = CriarTempoRelatorio(
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 6, 1, 11, 0, TimeSpan.Zero)));
        var response = Extrair(await NovoController(
            context,
            provider,
            reportingTime: reportingTime).GetOverview("30d"));

        Assert.Equal(new DateOnly(2026, 7, 7), response.Period.Start);
        Assert.Equal(new DateOnly(2026, 8, 5), response.Period.End);
        Assert.Equal(30, response.Trend.Count);
        Assert.Equal("05/08", response.Trend[^1].Label);
        Assert.DoesNotContain(response.Trend, point => point.Label == "06/08");
    }

    [Fact]
    public async Task Overview_SemEnvios_RetornaZerosSemNaN()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;
        context.Targets.AddRange(NovoAlvo("Com setor", "TI"), NovoAlvo("Sem setor", ""));
        await context.SaveChangesAsync();

        var response = Extrair(await NovoController(context, provider).GetOverview());

        Assert.Equal("Empresa A", response.Tenant.Name);
        Assert.Equal(0, response.Kpis.Sent.Total);
        Assert.Null(response.Kpis.Sent.DeltaPercent);
        Assert.Equal(0, response.Kpis.OpenRate.Rate);
        Assert.Equal(0, response.Kpis.ClickRate.Rate);
        Assert.Equal(0, response.Kpis.CompromiseRate.Rate);
        Assert.Equal(0, response.Kpis.TrainingRate.Rate);
        Assert.Equal(0, response.Kpis.OpenRate.ObservedTotal);
        Assert.Equal(0, response.Kpis.OpenRate.InferredTotal);
        Assert.Equal(0, response.TrainingEffectiveness.EducationViewed.Rate);
        Assert.Equal(0, response.TrainingEffectiveness.Abandoned.Rate);
        Assert.Equal(0, response.Scope.CampaignCount);
        Assert.Equal(0, response.Scope.UniqueTargetCount);
        Assert.Equal(0, response.Scope.CampaignTargetCount);
        Assert.Equal(["TI"], response.AvailableDepartments);
        Assert.Empty(response.RecentCampaigns);
    }

    [Fact]
    public async Task Overview_ContextoAgregado_ContaTresCampanhasEUmDestinatarioSemConfundirUnidades()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;

        var target = NovoAlvo("Mesmo destinatário", "TI");
        var campaigns = new[]
        {
            NovaCampanha("Campanha 1", DateTime.UtcNow.AddDays(-3)),
            NovaCampanha("Campanha 2", DateTime.UtcNow.AddDays(-2)),
            NovaCampanha("Campanha 3", DateTime.UtcNow.AddDays(-1)),
        };
        context.Targets.Add(target);
        context.Campaigns.AddRange(campaigns);
        await context.SaveChangesAsync();

        var quando = DateTime.UtcNow.AddHours(-1);
        context.SimulationsLogs.AddRange(campaigns.Select(campaign =>
            NovoLog(campaign, target, SimulationActions.Envio, quando)));
        await context.SaveChangesAsync();

        var response = Extrair(await NovoController(context, provider).GetOverview());

        Assert.Equal(3, response.Kpis.Sent.Total);
        Assert.Equal(3, response.Scope.CampaignCount);
        Assert.Equal(1, response.Scope.UniqueTargetCount);
        Assert.Equal(3, response.Scope.CampaignTargetCount);
    }

    [Fact]
    public async Task Overview_DeduplicaEventosPorCampanhaEAlvo()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;

        var targetA = NovoAlvo("A", "TI");
        var targetB = NovoAlvo("B", "TI");
        var campaign = NovaCampanha("Teste Executivo", DateTime.UtcNow.AddDays(-2));
        context.AddRange(targetA, targetB, campaign);
        await context.SaveChangesAsync();

        var agora = DateTime.UtcNow.AddHours(-1);
        context.SimulationsLogs.AddRange(
            NovoLog(campaign, targetA, SimulationActions.Envio, agora),
            NovoLog(campaign, targetA, SimulationActions.Envio, agora.AddMinutes(1)),
            NovoLog(campaign, targetB, SimulationActions.Envio, agora),
            NovoLog(campaign, targetA, SimulationActions.Abertura, agora.AddMinutes(2)),
            NovoLog(campaign, targetA, SimulationActions.Abertura, agora.AddMinutes(3)),
            NovoLog(campaign, targetA, SimulationActions.Clique, agora.AddMinutes(4)),
            NovoLog(campaign, targetA, SimulationActions.Clique, agora.AddMinutes(5)),
            NovoLog(campaign, targetA, SimulationActions.Submissao, agora.AddMinutes(6)),
            NovoLog(campaign, targetA, SimulationActions.PaginaEducacionalVisualizada, agora.AddMinutes(7)),
            NovoLog(campaign, targetA, SimulationActions.TreinamentoConcluido, agora.AddMinutes(8)),
            NovoLog(campaign, targetA, SimulationActions.TreinamentoConcluido, agora.AddMinutes(9)));
        await context.SaveChangesAsync();

        var response = Extrair(await NovoController(context, provider).GetOverview());

        Assert.Equal(2, response.Kpis.Sent.Total);
        Assert.Equal(50, response.Kpis.OpenRate.Rate);
        Assert.Equal(50, response.Kpis.ClickRate.Rate);
        Assert.Equal(50, response.Kpis.CompromiseRate.Rate);
        Assert.Equal(1, response.Kpis.CompromiseRate.UniqueTotal);
        Assert.Equal(100, response.Kpis.TrainingRate.Rate);
        Assert.Equal(1, response.Kpis.TrainingRate.UniqueTotal);
        Assert.Equal(1, response.Kpis.OpenRate.ObservedTotal);
        Assert.Equal(0, response.Kpis.OpenRate.InferredTotal);
        Assert.Single(response.RecentCampaigns);
        Assert.Equal(2, response.RecentCampaigns[0].Sent);
        Assert.Equal(50, response.RecentCampaigns[0].OpenRate);
        Assert.Equal(1, response.RecentCampaigns[0].OpenedTotal);
        Assert.Equal(0, response.RecentCampaigns[0].OpenedWithoutClickTotal);
        Assert.Equal(0, response.RecentCampaigns[0].OpenedWithoutClickRate);
        Assert.Equal(100, response.RecentCampaigns[0].TrainingRate);
        Assert.Equal(50, response.RecentCampaigns[0].EducationViewRate);
        Assert.Equal(0, response.RecentCampaigns[0].EducationAbandonmentTotal);
        Assert.Equal(50, response.RecentCampaigns[0].TrainingCompletionRate);
        Assert.Equal(100, response.TrainingEffectiveness.Recovery.Rate);
    }

    [Fact]
    public async Task Overview_AberturaEfetiva_SeparaObservadaEInferidaPorClique()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;

        var observado = NovoAlvo("Observado", "TI");
        var inferido = NovoAlvo("Inferido", "TI");
        var campaign = NovaCampanha("Aberturas", DateTime.UtcNow.AddDays(-1));
        context.AddRange(observado, inferido, campaign);
        await context.SaveChangesAsync();
        var quando = DateTime.UtcNow.AddHours(-1);
        context.SimulationsLogs.AddRange(
            NovoLog(campaign, observado, SimulationActions.Envio, quando),
            NovoLog(campaign, inferido, SimulationActions.Envio, quando),
            NovoLog(campaign, observado, SimulationActions.Abertura, quando.AddMinutes(1)),
            NovoLog(campaign, inferido, SimulationActions.Clique, quando.AddMinutes(2)),
            NovoLog(campaign, inferido, SimulationActions.PaginaEducacionalVisualizada, quando.AddMinutes(3)),
            NovoLog(campaign, inferido, SimulationActions.TreinamentoConcluido, quando.AddMinutes(4)));
        await context.SaveChangesAsync();

        var response = Extrair(await NovoController(context, provider).GetOverview());

        Assert.Equal(100, response.Kpis.OpenRate.Rate);
        Assert.Equal(2, response.Kpis.OpenRate.UniqueTotal);
        Assert.Equal(1, response.Kpis.OpenRate.ObservedTotal);
        Assert.Equal(1, response.Kpis.OpenRate.InferredTotal);
        Assert.Equal(100, response.Kpis.TrainingRate.Rate);
        Assert.Single(response.RecentCampaigns);
        Assert.Equal(100, response.RecentCampaigns[0].OpenRate);
        Assert.Equal(2, response.RecentCampaigns[0].OpenedTotal);
        Assert.Equal(1, response.RecentCampaigns[0].OpenedWithoutClickTotal);
        Assert.Equal(50, response.RecentCampaigns[0].OpenedWithoutClickRate);
        Assert.Contains(response.Trend, point => point.Opened == 2 && point.Trained == 1);
    }

    [Fact]
    public async Task Overview_EfetividadeTreinamento_CalculaAbandonoERecuperacaoPorConjuntos()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;

        var concluiu = NovoAlvo("Concluiu", "TI");
        var abandonou = NovoAlvo("Abandonou", "TI");
        var campaign = NovaCampanha("Treinamento", DateTime.UtcNow.AddDays(-1));
        context.AddRange(concluiu, abandonou, campaign);
        await context.SaveChangesAsync();
        var quando = DateTime.UtcNow.AddHours(-1);
        context.SimulationsLogs.AddRange(
            NovoLog(campaign, concluiu, SimulationActions.Envio, quando),
            NovoLog(campaign, abandonou, SimulationActions.Envio, quando),
            NovoLog(campaign, concluiu, SimulationActions.Clique, quando.AddMinutes(1)),
            NovoLog(campaign, abandonou, SimulationActions.Clique, quando.AddMinutes(1)),
            NovoLog(campaign, concluiu, SimulationActions.Submissao, quando.AddMinutes(2)),
            NovoLog(campaign, abandonou, SimulationActions.Submissao, quando.AddMinutes(2)),
            NovoLog(campaign, concluiu, SimulationActions.PaginaEducacionalVisualizada, quando.AddMinutes(3)),
            NovoLog(campaign, abandonou, SimulationActions.PaginaEducacionalVisualizada, quando.AddMinutes(3)),
            NovoLog(campaign, concluiu, SimulationActions.TreinamentoConcluido, quando.AddMinutes(4)));
        await context.SaveChangesAsync();

        var response = Extrair(await NovoController(context, provider).GetOverview());

        Assert.Equal(100, response.TrainingEffectiveness.Compromised.Rate);
        Assert.Equal(100, response.TrainingEffectiveness.EducationViewed.Rate);
        Assert.Equal(50, response.TrainingEffectiveness.Completed.Rate);
        Assert.Equal(1, response.TrainingEffectiveness.Abandoned.UniqueTotal);
        Assert.Equal(50, response.TrainingEffectiveness.Abandoned.Rate);
        Assert.Equal(1, response.TrainingEffectiveness.Recovery.UniqueTotal);
        Assert.Equal(50, response.TrainingEffectiveness.Recovery.Rate);
        Assert.Equal(1, response.RecentCampaigns[0].EducationAbandonmentTotal);
        Assert.Equal(50, response.RecentCampaigns[0].EducationAbandonmentRate);
        Assert.Contains(response.Trend, point => point.EducationViewed == 2 && point.Trained == 1);
    }

    [Fact]
    public async Task Overview_FiltroDepartamento_IgnoraCaixaEspacosEAlvosSemDepartamento()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;

        var tiMaiusculo = NovoAlvo("TI 1", "TI");
        var tiMinusculo = NovoAlvo("TI 2", "ti");
        var semSetor = NovoAlvo("Sem setor", "");
        var rh = NovoAlvo("RH", "RH");
        var campaign = NovaCampanha("Filtro", DateTime.UtcNow.AddDays(-1));
        context.AddRange(tiMaiusculo, tiMinusculo, semSetor, rh, campaign);
        await context.SaveChangesAsync();
        var quando = DateTime.UtcNow.AddMinutes(-30);
        context.SimulationsLogs.AddRange(
            NovoLog(campaign, tiMaiusculo, SimulationActions.Envio, quando),
            NovoLog(campaign, tiMinusculo, SimulationActions.Envio, quando),
            NovoLog(campaign, semSetor, SimulationActions.Envio, quando),
            NovoLog(campaign, rh, SimulationActions.Envio, quando));
        await context.SaveChangesAsync();

        var controller = NovoController(context, provider);
        var filtrado = Extrair(await controller.GetOverview("30d", "  tI  "));
        var todos = Extrair(await controller.GetOverview("30d", "Todos"));

        Assert.Equal(2, filtrado.Kpis.Sent.Total);
        Assert.Equal(4, todos.Kpis.Sent.Total);
        Assert.Equal(2, filtrado.AvailableDepartments.Count);
    }

    [Fact]
    public async Task Overview_DepartamentoInexistente_RetornaBadRequest()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;
        context.Targets.Add(NovoAlvo("A", "TI"));
        await context.SaveChangesAsync();

        var result = await NovoController(context, provider).GetOverview("30d", "Financeiro");

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12m")]
    [InlineData("invalido")]
    public async Task Overview_PeriodoInvalido_RetornaBadRequest(string period)
    {
        var (context, provider) = CriarContexto();
        provider.TenantIdAtivo = Guid.NewGuid();

        var result = await NovoController(context, provider).GetOverview(period);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Overview_ComparaEnviosComCicloAnterior()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;
        var targetA = NovoAlvo("A", "TI");
        var targetB = NovoAlvo("B", "TI");
        var campaign = NovaCampanha("Delta", DateTime.UtcNow.AddDays(-5));
        context.AddRange(targetA, targetB, campaign);
        await context.SaveChangesAsync();
        context.SimulationsLogs.AddRange(
            NovoLog(campaign, targetA, SimulationActions.Envio, DateTime.UtcNow.AddDays(-40)),
            NovoLog(campaign, targetA, SimulationActions.Envio, DateTime.UtcNow.AddDays(-2)),
            NovoLog(campaign, targetB, SimulationActions.Envio, DateTime.UtcNow.AddDays(-2)));
        await context.SaveChangesAsync();

        var response = Extrair(await NovoController(context, provider).GetOverview());

        Assert.Equal(2, response.Kpis.Sent.Total);
        Assert.Equal(100, response.Kpis.Sent.DeltaPercent);
    }

    [Fact]
    public async Task Overview_NuncaIncluiDadosDeOutroTenant()
    {
        var (context, provider) = CriarContexto();
        var tenantA = NovoTenant("Empresa A", "11111111000191");
        var tenantB = NovoTenant("Empresa B", "22222222000172");
        context.Tenants.AddRange(tenantA, tenantB);
        await context.SaveChangesAsync();

        provider.TenantIdAtivo = tenantA.Id;
        var targetA = NovoAlvo("A", "TI");
        var campaignA = NovaCampanha("Campanha A", DateTime.UtcNow.AddDays(-1));
        context.AddRange(targetA, campaignA);
        await context.SaveChangesAsync();
        context.SimulationsLogs.Add(NovoLog(campaignA, targetA, SimulationActions.Envio, DateTime.UtcNow.AddHours(-1)));
        await context.SaveChangesAsync();

        provider.TenantIdAtivo = tenantB.Id;
        var targetB = NovoAlvo("B", "Financeiro");
        var campaignB = NovaCampanha("Campanha B", DateTime.UtcNow.AddDays(-1));
        context.AddRange(targetB, campaignB);
        await context.SaveChangesAsync();
        context.SimulationsLogs.AddRange(
            NovoLog(campaignB, targetB, SimulationActions.Envio, DateTime.UtcNow.AddHours(-1)),
            NovoLog(campaignB, targetB, SimulationActions.Clique, DateTime.UtcNow.AddMinutes(-30)));
        await context.SaveChangesAsync();

        provider.TenantIdAtivo = tenantA.Id;
        var response = Extrair(await NovoController(context, provider).GetOverview());

        Assert.Equal("Empresa A", response.Tenant.Name);
        Assert.Equal(1, response.Kpis.Sent.Total);
        Assert.Equal(0, response.Kpis.ClickRate.Rate);
        Assert.DoesNotContain("Financeiro", response.AvailableDepartments);
        Assert.DoesNotContain(response.RecentCampaigns, c => c.Name == "Campanha B");
    }

    [Fact]
    public async Task Export_FormatoInvalido_RetornaBadRequest()
    {
        var (context, provider) = CriarContexto();
        provider.TenantIdAtivo = Guid.NewGuid();

        var result = await NovoController(context, provider).Export(format: "xlsx");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Export_SemTenantAutenticado_RetornaUnauthorized()
    {
        var (context, provider) = CriarContexto();

        var result = await NovoController(context, provider).Export();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Export_SemAdministradorNoJwt_RetornaUnauthorized()
    {
        var (context, provider) = CriarContexto();
        provider.TenantIdAtivo = Guid.NewGuid();

        var result = await NovoController(context, provider).Export();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Export_AdministradorDeOutroTenant_RetornaForbid()
    {
        var (context, provider) = CriarContexto();
        var tenantA = NovoTenant("Empresa A", "11111111000191");
        var tenantB = NovoTenant("Empresa B", "22222222000172");
        context.Tenants.AddRange(tenantA, tenantB);
        await context.SaveChangesAsync();

        provider.TenantIdAtivo = tenantB.Id;
        var otherTenantAdministrator = new Administrador
        {
            Id = Guid.NewGuid(),
            TenantId = tenantB.Id,
            Nome = "Administrador B",
            Email = "admin-b@teste.com",
            PasswordHash = "hash"
        };
        context.Administradores.Add(otherTenantAdministrator);
        await context.SaveChangesAsync();

        provider.TenantIdAtivo = tenantA.Id;
        var result = await NovoController(context, provider, otherTenantAdministrator.Id).Export();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Export_UsaIdentidadeAtualDoAdministradorAutenticado()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa Identificada", "12345678000190");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;

        var administrator = new Administrador
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Nome = "Maria Administradora",
            Email = "maria@empresa.test",
            PasswordHash = "hash"
        };
        context.Administradores.Add(administrator);
        await context.SaveChangesAsync();
        var capture = new CapturingDashboardExportService();

        var result = await NovoController(context, provider, administrator.Id, capture).Export();

        Assert.IsType<FileContentResult>(result);
        Assert.NotNull(capture.Context);
        Assert.Equal("Empresa Identificada", capture.Context.Identity.CompanyName);
        Assert.Equal("12345678000190", capture.Context.Identity.Cnpj);
        Assert.Equal("Maria Administradora", capture.Context.Identity.AdministratorName);
    }

    [Fact]
    public async Task Export_CsvUsaMesmoFiltroEEnviaFilenameUtf8()
    {
        var (context, provider) = CriarContexto();
        var tenant = NovoTenant("Empresa A", "11111111000191");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        provider.TenantIdAtivo = tenant.Id;
        var administrator = new Administrador
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Nome = "Administrador Responsável",
            Email = "admin@empresa-a.test",
            PasswordHash = "hash"
        };
        context.Administradores.Add(administrator);
        await context.SaveChangesAsync();

        var seguranca = NovoAlvo("Analista", "Segurança");
        var financeiro = NovoAlvo("Contador", "Financeiro");
        var campaign = NovaCampanha("Campanha filtrada", DateTime.UtcNow.AddDays(-1));
        context.AddRange(seguranca, financeiro, campaign);
        await context.SaveChangesAsync();
        context.SimulationsLogs.AddRange(
            NovoLog(campaign, seguranca, SimulationActions.Envio, DateTime.UtcNow.AddHours(-2)),
            NovoLog(campaign, financeiro, SimulationActions.Envio, DateTime.UtcNow.AddHours(-2)));
        await context.SaveChangesAsync();

        var controller = NovoController(context, provider, administrator.Id);
        var result = await controller.Export("csv", "30d", "segurança", "campaigns");

        var file = Assert.IsType<FileContentResult>(result);
        var csv = System.Text.Encoding.UTF8.GetString(file.FileContents);
        Assert.Equal("text/csv; charset=utf-8", file.ContentType);
        Assert.Contains("Campanha filtrada", csv);
        Assert.Contains(";1;", csv);
        Assert.Contains("filename*=UTF-8''", controller.Response.Headers.ContentDisposition.ToString());
        Assert.Contains("Seguran%C3%A7a", controller.Response.Headers.ContentDisposition.ToString());
        Assert.Equal("private, no-store", controller.Response.Headers.CacheControl.ToString());
    }
}
