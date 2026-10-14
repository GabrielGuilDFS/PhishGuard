using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;

namespace PhishGuard.Tests.TestDoubles;

internal static class MvpGraph
{
    internal static async Task<Campaign> SeedGraphAsync(AppDbContext db, Guid tenantId,
        string status = CampaignStatus.Rascunho, bool history = false)
    {
        db.Tenants.Add(new Tenant { Id = tenantId, NomeEmpresa = "Teste", Cnpj = Guid.NewGuid().ToString("N")[..14],
            Ativo = true, CriadoEm = DateTime.UtcNow });
        var template = new Template { Id = Guid.NewGuid(), Nome = "Modelo", Assunto = "Assunto", RemetenteNome = "Empresa",
            RemetenteEmail = "teste@example.test", CorpoHtml = "<p>Original</p>", CriadoEm = DateTime.UtcNow };
        var page = new PhishingPage { Id = Guid.NewGuid(), Nome = "Página", HtmlCaptura = "<p>Original</p>", CriadoEm = DateTime.UtcNow };
        var education = new EducationalPage { Id = Guid.NewGuid(), Nome = "Educação", HtmlEducacional = "<p>Original</p>", CriadoEm = DateTime.UtcNow };
        var target = new Target { Id = Guid.NewGuid(), Nome = "Pessoa", Email = "pessoa@example.test", Departamento = "TI" };
        var campaign = new Campaign { Id = Guid.NewGuid(), NomeCampanha = "Original", Status = status, DataInicio = DateTime.UtcNow,
            CriadoEm = DateTime.UtcNow, Template = template, PhishingPage = page, EducationalPage = education, Targets = [target] };
        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();
        if (history)
        {
            db.SimulationsLogs.Add(new SimulationLog { Id = Guid.NewGuid(), CampaignId = campaign.Id, TargetId = target.Id,
                Acao = SimulationActions.Envio, DataHora = DateTime.UtcNow, IpOrigem = "SISTEMA" });
            db.CampaignDeliveries.Add(new CampaignDelivery { Id = Guid.NewGuid(), CampaignId = campaign.Id, TargetId = target.Id,
                IdempotencyKey = Guid.NewGuid().ToString(), Status = CampaignDeliveryStatus.Sent,
                LastAttemptAtUtc = DateTime.UtcNow, SentAtUtc = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
        return campaign;
    }

}
