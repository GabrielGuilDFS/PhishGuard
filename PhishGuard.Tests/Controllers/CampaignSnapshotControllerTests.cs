using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Services.Delivery;
using PhishGuard.Tests.Data;
using PhishGuard.Tests.Security;
using PhishGuard.Tests.TestDoubles;
using System.Text.Json;

namespace PhishGuard.Tests.Controllers;

public sealed class CampaignSnapshotControllerTests
{
    private static readonly HtmlSanitizationService Sanitizer = new();
    private sealed class Tokens : ITrackingTokenService
    {
        public string Create(Guid campaignId, Guid targetId) => $"{campaignId:N}:{targetId:N}";
        public bool Validate(string? token, Guid campaignId, Guid targetId) => token == Create(campaignId, targetId);
    }

    private static async Task AddTransportAsync(SqliteTenantHarness harness)
    {
        harness.Db.SmtpConfigs.Add(new SmtpConfig { Id = Guid.NewGuid(), Host = "smtp.test", Porta = 587,
            Usuario = "sender@example.test", Senha = "test-only", SenderName = "Equipe original" });
        await harness.Db.SaveChangesAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Activation_FreezesRecipientAndResources_AllowsCatalogManagementAndPreservesRetry(bool scheduled)
    {
        using var harness = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(harness.Db, harness.Tenant.TenantIdAtivo);
        campaign.DataInicio = DateTime.UtcNow.AddHours(scheduled ? 2 : -2);
        await AddTransportAsync(harness);
        var campaigns = new CampaignsController(harness.Db, harness.Tenant, htmlSanitizer: Sanitizer);
        var activation = await campaigns.AtivarCampanha(campaign.Id);
        Assert.True(activation is AcceptedResult or OkObjectResult);
        var capturedAt = Assert.IsType<DateTime>(campaign.SnapshotCapturedAtUtc);
        var recipient = Assert.Single(campaign.CampaignTargets);
        Assert.Equal(capturedAt, recipient.SnapshotCapturedAtUtc);
        Assert.False(campaign.IsLegacySnapshot);
        Assert.False(recipient.IsLegacySnapshot);

        var target = campaign.Targets.Single();
        Assert.IsType<NoContentResult>(await new TargetsController(harness.Db, harness.Tenant).PutAlvo(target.Id,
            new TargetInputDto { Nome = "Nome atual", Email = "atual@example.test", Departamento = "RH" }));
        Assert.IsType<NoContentResult>(await new TemplatesController(harness.Db, harness.Tenant, Sanitizer).PutTemplate(campaign.EmailTemplateId,
            new Template { Id = campaign.EmailTemplateId, Nome = "Modelo atual", Assunto = "Assunto atual", RemetenteNome = "Novo", RemetenteEmail = "novo@example.test", CorpoHtml = "<p>Novo</p>" }));
        Assert.IsType<NoContentResult>(await new PhishingPagesController(harness.Db, harness.Tenant, Sanitizer).PutPage(campaign.LandingPageId,
            new PageInputDto { Nome = "Página atual", ConteudoHtml = "amazon-login" }));
        Assert.IsType<NoContentResult>(await new EducationalPagesController(harness.Db, harness.Tenant, Sanitizer).PutPage(campaign.EducationalPageId,
            new PageInputDto { Nome = "Educação atual", ConteudoHtml = "<p>Novo</p>" }));
        campaign.Status = CampaignStatus.FalhaNoDisparo;
        await harness.Db.SaveChangesAsync();
        Assert.IsType<AcceptedResult>(await campaigns.RetryDispatch(campaign.Id));

        var message = new EmailMessageComposer(new Tokens(), "https://api.example.test").Compose(campaign, target,
            "different-sender@example.test", "Nome atual");
        Assert.Equal("pessoa@example.test", message.ToEmail);
        Assert.Equal("Pessoa", message.ToName);
        Assert.Equal("Assunto", message.Subject);
        Assert.Equal("Equipe original", message.FromName);
        Assert.Equal("sender@example.test", message.FromEmail);
        Assert.Contains("Original", message.HtmlBody);
        Assert.DoesNotContain("Novo", message.HtmlBody);
        Assert.Equal("TI", recipient.DepartmentSnapshot);
        Assert.Equal(capturedAt, campaign.SnapshotCapturedAtUtc);

        var tokens = new Tokens();
        var landing = new PhishingPagesController(harness.Db, harness.Tenant, Sanitizer,
            new TrackingCampaignScope(harness.Db, tokens));
        var result = Assert.IsType<OkObjectResult>((await landing.GetPage(campaign.LandingPageId, campaign.Id, target.Id, tokens.Create(campaign.Id, target.Id))).Result);
        var json = JsonSerializer.SerializeToElement(result.Value);
        Assert.Contains("Original", json.GetProperty("conteudoHtml").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("templateId").ValueKind);
        Assert.IsType<NotFoundResult>((await landing.GetPage(campaign.LandingPageId, campaign.Id, Guid.Empty, tokens.Create(campaign.Id, Guid.Empty))).Result);
        harness.UsarTenant(Guid.Empty);
        Assert.IsType<NotFoundResult>((await landing.GetPage(campaign.LandingPageId)).Result);
    }

    [Fact]
    public async Task OfficialSnapshots_MaterializeHtmlAndInteractiveTraining_SanitizeAndKeepHistoricalDepartment()
    {
        using var harness = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(harness.Db, harness.Tenant.TenantIdAtivo);
        campaign.Template.CorpoHtml = "mercadoliv-novo-acesso";
        campaign.PhishingPage.HtmlCaptura = "mercadoliv-login";
        campaign.EducationalPage.HtmlEducacional = "<div data-feedback-training=\"mercadoliv\"></div>";
        await AddTransportAsync(harness);
        Assert.IsType<AcceptedResult>(await new CampaignsController(harness.Db, harness.Tenant, htmlSanitizer: Sanitizer).AtivarCampanha(campaign.Id));
        Assert.Contains("<", campaign.EmailBodySnapshot);
        Assert.Contains("<form", campaign.LandingHtmlSnapshot);
        Assert.Equal("mercado-liv-login", campaign.LandingTemplateIdSnapshot);
        Assert.DoesNotContain("onsubmit=", campaign.LandingHtmlSnapshot);
        Assert.DoesNotContain("{{DATA_ACESSO}}", campaign.EducationalFeedbackConfigJsonSnapshot);
        Assert.DoesNotContain("onsubmit=", campaign.EducationalFeedbackConfigJsonSnapshot);
        var target = campaign.Targets.Single();
        harness.Db.SimulationsLogs.Add(new SimulationLog { Id = Guid.NewGuid(), CampaignId = campaign.Id, TargetId = target.Id,
            Acao = SimulationActions.Envio, DataHora = DateTime.UtcNow, IpOrigem = "SISTEMA" });
        await harness.Db.SaveChangesAsync();
        Assert.IsType<NoContentResult>(await new TargetsController(harness.Db, harness.Tenant).PutAlvo(target.Id,
            new TargetInputDto { Nome = "Atual", Email = "agora@example.test", Departamento = "RH" }));
        var reportingTime = new DashboardReportingTime(TimeProvider.System, TimeZoneInfo.Utc);
        var metrics = await new DashboardOverviewService(harness.Db, reportingTime).GetOverviewAsync(campaign.TenantId, "30d", "TI");
        Assert.Equal(1, metrics.Kpis.Sent.Total);
        Assert.Equal(0, metrics.Scope.LegacyCampaignTargetCount);
        Assert.Contains("TI", metrics.AvailableDepartments);
        var currentDepartment = await new DashboardOverviewService(harness.Db, reportingTime).GetOverviewAsync(campaign.TenantId, "30d", "RH");
        Assert.Equal(0, currentDepartment.Kpis.Sent.Total);
        var dashboard = new DashboardController(harness.Db, harness.Tenant,
            new DashboardOverviewService(harness.Db, reportingTime), new DashboardExportService(), reportingTime);
        var departmentResult = Assert.IsType<OkObjectResult>(await dashboard.GetDepartments());
        var departments = JsonSerializer.SerializeToElement(departmentResult.Value);
        var historicalDepartment = departments.EnumerateArray().Single(d => d.GetProperty("name").GetString() == "TI");
        Assert.Equal(1, historicalDepartment.GetProperty("emails").GetInt32());
        Assert.Equal(0, historicalDepartment.GetProperty("employees").GetInt32());
        var timelineResult = Assert.IsType<OkObjectResult>(await dashboard.GetDeptRiskTimeline());
        var timeline = JsonSerializer.SerializeToElement(timelineResult.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal("TI", timeline.GetProperty("departamentos")[0].GetString());
        var tokens = new Tokens();
        var tracking = new TrackingController(harness.Db, new ConfigurationBuilder().Build(),
            new TrackingCampaignScope(harness.Db, tokens), Sanitizer)
            { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        var education = Assert.IsType<OkObjectResult>(await tracking.GetEducational(campaign.Id, target.Id, tokens.Create(campaign.Id, target.Id)));
        var json = JsonSerializer.SerializeToElement(education.Value);
        Assert.Equal("mercadoliv", json.GetProperty("feedbackConfigJson").GetProperty("id").GetString());
        Assert.False(json.GetProperty("isLegacySnapshot").GetBoolean());
        Assert.IsType<NotFoundObjectResult>(await tracking.GetEducational(campaign.Id, Guid.Empty, tokens.Create(campaign.Id, Guid.Empty)));
    }

    [Fact]
    public async Task SignedParticipantEndpoints_AnonymousScopeIsFilteredAndRestored_AuthenticatedCrossTenantIsRejected()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        using var harness = new SqliteTenantHarness(tenantA);
        var campaignA = await MvpGraph.SeedGraphAsync(harness.Db, tenantA,
            CampaignStatus.EmAndamento, history: true);
        harness.UsarTenant(tenantB);
        var campaignB = await MvpGraph.SeedGraphAsync(harness.Db, tenantB,
            CampaignStatus.EmAndamento, history: true);
        harness.Db.ChangeTracker.Clear();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AppSettings:Token"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64))
        }).Build();
        var tokens = new TrackingTokenService(TimeProvider.System, configuration);
        var resolver = new TrackingCampaignScope(harness.Db, tokens);
        var targetA = campaignA.Targets.Single();
        var tokenA = tokens.Create(campaignA.Id, targetA.Id);
        harness.UsarTenant(Guid.Empty);
        var tracking = new TrackingController(harness.Db, configuration, resolver, Sanitizer)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var landing = new PhishingPagesController(harness.Db, harness.Tenant, Sanitizer, resolver);

        Assert.IsType<FileContentResult>(await tracking.TrackOpen(campaignA.Id, targetA.Id, trackingToken: tokenA));
        Assert.Equal(Guid.Empty, harness.Db.TenantIdAtual);
        Assert.Equal(0, await harness.Db.Campaigns.CountAsync());
        Assert.Equal(0, await harness.Db.SimulationsLogs.CountAsync());
        Assert.IsType<OkObjectResult>((await landing.GetPage(campaignA.LandingPageId, campaignA.Id, targetA.Id, tokenA)).Result);
        Assert.Equal(Guid.Empty, harness.Db.TenantIdAtual);
        Assert.IsType<OkObjectResult>(await tracking.GetEducational(campaignA.Id, targetA.Id, tokenA));
        Assert.Equal(Guid.Empty, harness.Db.TenantIdAtual);
        Assert.IsType<NotFoundResult>((await landing.GetPage(campaignB.LandingPageId, campaignA.Id, targetA.Id, tokenA)).Result);
        Assert.Equal(Guid.Empty, harness.Db.TenantIdAtual);
        Assert.IsType<NotFoundObjectResult>(await tracking.GetEducational(campaignB.Id, campaignB.Targets.Single().Id, tokenA));
        Assert.Equal(Guid.Empty, harness.Db.TenantIdAtual);

        harness.UsarTenant(tenantA);
        Assert.Equal(1, await harness.Db.SimulationsLogs.CountAsync(l => l.Acao == SimulationActions.Abertura));
        harness.UsarTenant(tenantB);
        Assert.Equal(0, await harness.Db.SimulationsLogs.CountAsync(l => l.Acao == SimulationActions.Abertura));
        Assert.IsType<NotFoundResult>((await landing.GetPage(campaignA.LandingPageId, campaignA.Id, targetA.Id, tokenA)).Result);
        Assert.IsType<NotFoundObjectResult>(await tracking.GetEducational(campaignA.Id, targetA.Id, tokenA));
        Assert.Equal(tenantB, harness.Db.TenantIdAtual);
        Assert.Equal(1, await harness.Db.Campaigns.CountAsync());
        Assert.Equal(0, await harness.Db.SimulationsLogs.CountAsync(l => l.Acao == SimulationActions.Abertura));
    }

    [PostgresFact]
    public async Task Postgres_SignedParticipantScope_DiscoversOnlyTenant_AndKeepsGlobalFiltersActive()
    {
        var tenantId = Guid.NewGuid();
        var provider = new MutableTenantProvider { TenantIdAtivo = tenantId };
        // The existing harness rejects any connection outside the disposable MVP DB.
        await using var context = new AppDbContext(
            TenantMutationFilterTests.PostgresOptions("phishguard_mvp_tracking_scope"), provider);
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
        var campaign = await MvpGraph.SeedGraphAsync(context, tenantId, CampaignStatus.EmAndamento, history: true);
        var target = campaign.Targets.Single();
        context.ChangeTracker.Clear();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AppSettings:Token"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64))
        }).Build();
        var tokens = new TrackingTokenService(TimeProvider.System, configuration);
        var resolver = new TrackingCampaignScope(context, tokens);
        var token = tokens.Create(campaign.Id, target.Id);
        provider.TenantIdAtivo = Guid.Empty;
        Assert.Null(await resolver.ResolveAsync(campaign.Id, target.Id, token + "invalid"));
        Assert.Equal(Guid.Empty, context.TenantIdAtual);
        using (var lease = await resolver.ResolveAsync(campaign.Id, target.Id, token))
        {
            Assert.NotNull(lease);
            Assert.Equal(tenantId, context.TenantIdAtual);
            Assert.Equal(campaign.Id, lease.Campaign.Id);
            Assert.Equal(1, await context.Campaigns.CountAsync());
            Assert.Equal(1, await context.CampaignTargets.CountAsync());
            Assert.Equal(1, await context.SimulationsLogs.CountAsync());
        }
        Assert.Equal(Guid.Empty, context.TenantIdAtual);
        Assert.Equal(0, await context.Campaigns.CountAsync());
        provider.TenantIdAtivo = Guid.NewGuid();
        Assert.Null(await resolver.ResolveAsync(campaign.Id, target.Id, token));
        Assert.Equal(provider.TenantIdAtivo, context.TenantIdAtual);
        Assert.Equal(0, await context.Campaigns.CountAsync());
    }
}
