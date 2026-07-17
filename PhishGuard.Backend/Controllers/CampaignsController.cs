using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.DTOs;
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

        // O disparo de e-mails NÃO é responsabilidade deste controller: ele apenas
        // transiciona o estado da campanha. O envio assíncrono fica a cargo do
        // CampaignSchedulerWorker (baixo acoplamento + resiliência).
        public CampaignsController(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
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
                    // Data de Encerramento da Coleta: incluída na LISTAGEM (antes só o GET
                    // por id a trazia), por isso a coluna aparecia vazia mesmo em campanhas
                    // que já tinham o prazo definido. Nula = coleta sem prazo (indefinida).
                    dataFim = c.DataFim,
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
                DataInicio = ParaUtc(input.DataInicio),
                DataFim = ParaUtc(input.DataFim),
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

        // Ativa (homologa) uma campanha em Rascunho. NÃO dispara e-mails na thread HTTP:
        // apenas transiciona o estado e delega o envio (assíncrono) ao worker.
        //  - DataInicio no FUTURO  → "Agendada" (o worker dispara no horário).
        //  - DataInicio já atingida → CLAIM "Processando" (o worker envia no próximo ciclo).
        [HttpPost("{id}/ativar")]
        public async Task<IActionResult> AtivarCampanha(Guid id)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant.
            var campaign = await _context.Campaigns
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

            // Início já alcançado: faz o CLAIM ("Processando") e RETORNA na hora. O envio
            // em lote roda de forma assíncrona e idempotente no worker — a thread HTTP não
            // fica bloqueada por minutos nem segura a conexão do banco durante o disparo.
            campaign.Status = CampaignStatus.Processando;
            await _context.SaveChangesAsync();

            return Accepted(new
            {
                message = "Disparo enfileirado. Os e-mails serão enviados em instantes.",
                status = campaign.Status
            });
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
            campaignExistente.DataInicio = ParaUtc(input.DataInicio);
            campaignExistente.DataFim = ParaUtc(input.DataFim);
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
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant. FindAsync busca só por PK e
            // contornaria o Global Query Filter, permitindo apagar recurso de outro tenant.
            var campaign = await _context.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
            if (campaign == null) return NotFound();

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // TIMEZONE (blindagem de agendamento): as colunas de data são 'timestamp with time
        // zone' (timestamptz) e o worker compara o agendamento contra DateTime.UtcNow. Por
        // isso normalizamos toda data recebida para UTC AQUI, na fronteira da API:
        //  - Utc         → mantém (o front envia ISO 8601 com 'Z');
        //  - Local       → converte para UTC (preserva o instante);
        //  - Unspecified → ASSUME UTC (contrato explícito da API). Sem isto, o Npgsql
        //                  REJEITA um DateTime 'Unspecified' ao gravar em timestamptz, e a
        //                  criação/edição da campanha falharia (ex.: chamada via Swagger sem
        //                  fuso). Garante também que a comparação do worker seja UTC × UTC.
        private static DateTime ParaUtc(DateTime valor) => valor.Kind switch
        {
            DateTimeKind.Utc => valor,
            DateTimeKind.Local => valor.ToUniversalTime(),
            _ => DateTime.SpecifyKind(valor, DateTimeKind.Utc)
        };

        private static DateTime? ParaUtc(DateTime? valor) => valor.HasValue ? ParaUtc(valor.Value) : null;

        private async Task<bool> ResourcesExistAndBelongToTenant(CampaignInputDto input, Guid tenantId)
        {
            var templateExists = await _context.Templates.AnyAsync(t => t.Id == input.EmailTemplateId && t.TenantId == tenantId);
            var landingPageExists = await _context.PhishingPages.AnyAsync(p => p.Id == input.LandingPageId && p.TenantId == tenantId);
            var educationalPageExists = await _context.EducationalPages.AnyAsync(e => e.Id == input.EducationalPageId && e.TenantId == tenantId);

            return templateExists && landingPageExists && educationalPageExists;
        }
    }
}