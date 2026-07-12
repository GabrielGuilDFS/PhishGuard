using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishGuard.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICampaignDispatchService _dispatchService;

        public CampaignsController(
            AppDbContext context,
            ITenantProvider tenantProvider,
            ICampaignDispatchService dispatchService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _dispatchService = dispatchService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCampaigns()
        {
            var campaigns = await _context.Campaigns
                .Include(c => c.Template)
                .Include(c => c.PhishingPage)
                .Include(c => c.EducationalPage)
                .OrderByDescending(c => c.CriadoEm)
                .Select(c => new
                {
                    c.Id,
                    nomeCampanha = c.NomeCampanha,
                    status = c.Status,
                    dataInicio = c.DataInicio,
                    templateNome = c.Template.Nome,
                    landingPageNome = c.PhishingPage.Nome,
                    educationalPageNome = c.EducationalPage.Nome
                })
                .ToListAsync();

            return Ok(campaigns);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCampaign(Guid id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.Template)
                .Include(c => c.PhishingPage)
                .Include(c => c.EducationalPage)
                .Include(c => c.Targets) 
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    nomeCampanha = c.NomeCampanha,
                    status = c.Status,
                    dataInicio = c.DataInicio,
                    dataFim = c.DataFim,
                    emailTemplateId = c.EmailTemplateId,
                    landingPageId = c.LandingPageId,
                    educationalPageId = c.EducationalPageId,
                    templateNome = c.Template.Nome,
                    landingPageNome = c.PhishingPage.Nome,
                    educationalPageNome = c.EducationalPage.Nome,
                    targetIds = c.Targets.Select(t => t.Id).ToList() 
                })
                .FirstOrDefaultAsync();

            if (campaign == null) return NotFound();

            return Ok(campaign);
        }

        [HttpPost]
        public async Task<ActionResult> PostCampaign([FromBody] CampaignInputDto input)
        {
            var tenantId = _tenantProvider.GetTenantId();

            if (!await ResourcesExistAndBelongToTenant(input, tenantId))
            {
                return BadRequest("Um ou mais recursos referenciados não existem ou não pertencem ao tenant atual.");
            }

            var targetsSelecionados = await _context.Targets
                .Where(t => input.TargetIds.Contains(t.Id) && t.TenantId == tenantId)
                .ToListAsync();

            var novaCampanha = new Campaign
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                NomeCampanha = input.NomeCampanha,
                DataInicio = input.DataInicio,
                DataFim = input.DataFim,
                EmailTemplateId = input.EmailTemplateId,
                LandingPageId = input.LandingPageId,
                EducationalPageId = input.EducationalPageId,
                Status = CampaignStatus.Rascunho,
                CriadoEm = DateTime.UtcNow,
                Targets = targetsSelecionados 
            };

            _context.Campaigns.Add(novaCampanha);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCampaign), new { id = novaCampanha.Id }, new 
            {
                novaCampanha.Id,
                novaCampanha.NomeCampanha,
                novaCampanha.Status
            });
        }

        // Ativa (homologa) uma campanha em Rascunho. Decide entre AGENDAR ou DISPARAR JÁ
        // conforme a DataInicio, implementando a transição Rascunho → Agendada/Em Andamento.
        [HttpPost("{id}/ativar")]
        public async Task<IActionResult> AtivarCampanha(Guid id)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Busca a campanha (escopada ao tenant) com o que o disparo precisa.
            var campaign = await _context.Campaigns
                .Include(c => c.Template)
                .Include(c => c.Targets)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (campaign == null) return NotFound("Campanha não encontrada.");
            if (campaign.Status != CampaignStatus.Rascunho)
                return BadRequest("Apenas campanhas em Rascunho podem ser ativadas.");
            if (!campaign.Targets.Any()) return BadRequest("A campanha não possui alvos selecionados.");

            // Início no FUTURO: apenas agenda. O CampaignSchedulerWorker faz o disparo
            // automaticamente quando a DataInicio for atingida.
            if (campaign.DataInicio > DateTime.UtcNow)
            {
                campaign.Status = CampaignStatus.Agendada;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Campanha agendada. O disparo ocorrerá no horário de início.", status = campaign.Status });
            }

            // Início já alcançado: dispara IMEDIATAMENTE, reutilizando o mesmo serviço do
            // worker (PASSO 3). O serviço move o status para "Em Andamento".
            try
            {
                await _dispatchService.DispatchAsync(campaign);
                return Ok(new { message = "Disparo iniciado com sucesso!", status = campaign.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no disparo: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCampaign(Guid id, [FromBody] CampaignInputDto input)
        {
            var tenantId = _tenantProvider.GetTenantId();
            
            var campaignExistente = await _context.Campaigns
                .Include(c => c.Targets)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
                
            if (campaignExistente == null) return NotFound();

            if (!await ResourcesExistAndBelongToTenant(input, tenantId))
            {
                return BadRequest("Um ou mais recursos referenciados não existem ou não pertencem ao tenant atual.");
            }

            var novosTargets = await _context.Targets
                .Where(t => input.TargetIds.Contains(t.Id) && t.TenantId == tenantId)
                .ToListAsync();

            campaignExistente.NomeCampanha = input.NomeCampanha;
            campaignExistente.DataInicio = input.DataInicio;
            campaignExistente.DataFim = input.DataFim;
            campaignExistente.EmailTemplateId = input.EmailTemplateId;
            campaignExistente.LandingPageId = input.LandingPageId;
            campaignExistente.EducationalPageId = input.EducationalPageId;

            campaignExistente.Targets.Clear(); 
            foreach (var target in novosTargets)
            {
                campaignExistente.Targets.Add(target); 
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCampaign(Guid id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ResourcesExistAndBelongToTenant(CampaignInputDto input, Guid tenantId)
        {
            var templateExists = await _context.Templates.AnyAsync(t => t.Id == input.EmailTemplateId && t.TenantId == tenantId);
            var landingPageExists = await _context.PhishingPages.AnyAsync(p => p.Id == input.LandingPageId && p.TenantId == tenantId);
            var educationalPageExists = await _context.EducationalPages.AnyAsync(e => e.Id == input.EducationalPageId && e.TenantId == tenantId);

            return templateExists && landingPageExists && educationalPageExists;
        }
    }
}