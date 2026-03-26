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
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PhishGuard.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

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
                Status = "Rascunho",
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

        [HttpPost("{id}/iniciar")]
        public async Task<IActionResult> IniciarDisparo(Guid id)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // 1. Busca a Campanha com TUDO que ela precisa
            var campaign = await _context.Campaigns
                .Include(c => c.Template)
                .Include(c => c.Targets)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (campaign == null) return NotFound("Campanha não encontrada.");
            if (campaign.Status != "Rascunho") return BadRequest("Esta campanha já foi iniciada.");
            if (!campaign.Targets.Any()) return BadRequest("A campanha não possui alvos selecionados.");

            // 2. Busca o SMTP do Tenant
            var smtpConfig = await _context.SmtpConfigs.FirstOrDefaultAsync(s => s.TenantId == tenantId);
            if (smtpConfig == null) return BadRequest("Configuração SMTP não encontrada. Configure o servidor de e-mail primeiro.");

            try
            {
                using var client = new SmtpClient();
                // "Auto" escolhe a melhor criptografia disponível (TLS) automaticamente
                await client.ConnectAsync(smtpConfig.Host, smtpConfig.Porta, SecureSocketOptions.StartTls);
                
                // Na versão final, a autenticação é OBRIGATÓRIA
                await client.AuthenticateAsync(smtpConfig.Usuario, smtpConfig.Senha);

                foreach (var target in campaign.Targets)
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(campaign.Template.RemetenteNome, campaign.Template.RemetenteEmail));
                    message.To.Add(new MailboxAddress(target.Nome, target.Email));
                    message.Subject = campaign.Template.Assunto;

                    // Personaliza o corpo do e-mail
                    var corpoPersonalizado = campaign.Template.CorpoHtml.Replace("{{NOME}}", target.Nome);
                    var builder = new BodyBuilder { HtmlBody = corpoPersonalizado };
                    message.Body = builder.ToMessageBody();

                    // Dispara
                    await client.SendAsync(message);

                    // THROTTLING: Pausa de 1 segundo para o Gmail/Outlook não te banir por spam
                    await Task.Delay(1000); 
                }

                await client.DisconnectAsync(true);

                campaign.Status = "Em Andamento";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Disparo iniciado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro SMTP: {ex.Message}");
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