using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using PhishGuard.Tests.TestDoubles;

using PhishGuard.Tests.Data;

namespace PhishGuard.Tests.Controllers;

public class MvpHistoryControllerTests
{
    [Theory]
    [InlineData(CampaignStatus.Agendada)]
    [InlineData(CampaignStatus.Processando)]
    [InlineData(CampaignStatus.EmAndamento)]
    [InlineData(CampaignStatus.Finalizada)]
    [InlineData(CampaignStatus.FalhaNoDisparo)]
    public async Task ActivatedCampaign_CannotEditOrDelete(string status)
    {
        using var h = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(h.Db, h.Tenant.TenantIdAtivo, status, history: true);
        var controller = new CampaignsController(h.Db, h.Tenant, htmlSanitizer: new PhishGuard.Backend.Services.HtmlSanitizationService());
        Assert.IsType<ConflictObjectResult>(await controller.PutCampaign(campaign.Id, new CampaignInputDto { NomeCampanha = "Modificada" }));
        Assert.IsType<ConflictObjectResult>(await controller.DeleteCampaign(campaign.Id));
        Assert.Equal("Original", campaign.NomeCampanha);
        Assert.Equal(1, await h.Db.SimulationsLogs.CountAsync());
        Assert.Equal(1, await h.Db.CampaignDeliveries.CountAsync());
    }

    [Fact]
    public async Task DraftWithHistory_CannotDeleteEditOrActivate()
    {
        using var h = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(h.Db, h.Tenant.TenantIdAtivo, history: true);
        var controller = new CampaignsController(h.Db, h.Tenant, htmlSanitizer: new HtmlSanitizationService());
        Assert.IsType<ConflictObjectResult>(await controller.DeleteCampaign(campaign.Id));
        Assert.IsType<ConflictObjectResult>(await controller.PutCampaign(campaign.Id, new CampaignInputDto { NomeCampanha = "Alterada" }));
        Assert.IsType<ConflictObjectResult>(await controller.AtivarCampanha(campaign.Id));
        Assert.Equal("Original", campaign.NomeCampanha);
        Assert.Equal(CampaignStatus.Rascunho, campaign.Status);
        Assert.Equal(1, await h.Db.SimulationsLogs.CountAsync());
        Assert.Equal(1, await h.Db.CampaignDeliveries.CountAsync());
    }

    [Fact]
    public async Task DraftWithoutHistory_CanDeleteThenDeleteTarget()
    {
        using var h = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(h.Db, h.Tenant.TenantIdAtivo);
        var targetId = campaign.Targets.Single().Id;
        var targets = new TargetsController(h.Db, h.Tenant);
        Assert.IsType<ConflictObjectResult>(await targets.DeleteAlvo(targetId));
        Assert.IsType<NoContentResult>(await new CampaignsController(h.Db, h.Tenant, htmlSanitizer: new PhishGuard.Backend.Services.HtmlSanitizationService()).DeleteCampaign(campaign.Id));
        Assert.IsType<NoContentResult>(await targets.DeleteAlvo(targetId));
    }

    [Fact]
    public async Task TargetWithActivatedCampaign_CannotChangeDepartmentOrEmail()
    {
        using var h = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(h.Db, h.Tenant.TenantIdAtivo, CampaignStatus.Agendada);
        var target = campaign.Targets.Single();
        var controller = new TargetsController(h.Db, h.Tenant);
        Assert.IsType<ConflictObjectResult>(await controller.PutAlvo(target.Id,
            new TargetInputDto { Nome = target.Nome, Email = target.Email, Departamento = "RH" }));
        Assert.IsType<ConflictObjectResult>(await controller.PutAlvo(target.Id,
            new TargetInputDto { Nome = target.Nome, Email = "nova@example.test", Departamento = target.Departamento }));
        Assert.Equal("TI", target.Departamento);
        Assert.Equal("pessoa@example.test", target.Email);
        Assert.IsType<NoContentResult>(await controller.PutAlvo(target.Id,
            new TargetInputDto { Nome = "Nome corrigido", Email = target.Email, Departamento = target.Departamento }));
    }

    [Fact]
    public async Task ActivatedResources_CannotChangeOrDelete()
    {
        using var h = new SqliteTenantHarness(Guid.NewGuid());
        var campaign = await MvpGraph.SeedGraphAsync(h.Db, h.Tenant.TenantIdAtivo, CampaignStatus.Agendada);
        var sanitizer = new HtmlSanitizationService();
        var templates = new TemplatesController(h.Db, h.Tenant, sanitizer);
        Assert.IsType<ConflictObjectResult>(await templates.PutTemplate(campaign.EmailTemplateId,
            new Template { Id = campaign.EmailTemplateId, CorpoHtml = "novo" }));
        Assert.IsType<ConflictObjectResult>(await templates.DeleteTemplate(campaign.EmailTemplateId));
        var pages = new PhishingPagesController(h.Db, h.Tenant, sanitizer);
        Assert.IsType<ConflictObjectResult>(await pages.PutPage(campaign.LandingPageId, new PageInputDto { ConteudoHtml = "novo" }));
        Assert.IsType<ConflictObjectResult>(await pages.DeletePage(campaign.LandingPageId));
        var education = new EducationalPagesController(h.Db, h.Tenant, sanitizer);
        Assert.IsType<ConflictObjectResult>(await education.PutPage(campaign.EducationalPageId, new PageInputDto { ConteudoHtml = "novo" }));
        Assert.IsType<ConflictObjectResult>(await education.DeletePage(campaign.EducationalPageId));
        Assert.Equal("<p>Original</p>", campaign.Template.CorpoHtml);
        Assert.Equal("<p>Original</p>", campaign.PhishingPage.HtmlCaptura);
        Assert.Equal("<p>Original</p>", campaign.EducationalPage.HtmlEducacional);
    }

}
