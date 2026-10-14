using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
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
    [ServiceFilter(typeof(TenantMutationFilter))]
    public class TemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHtmlSanitizationService _htmlSanitizer;

        public TemplatesController(
            AppDbContext context,
            ITenantProvider tenantProvider,
            IHtmlSanitizationService htmlSanitizer)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _htmlSanitizer = htmlSanitizer;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplates()
        {
            return await _context.Templates.OrderByDescending(t => t.CriadoEm).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Template>> GetTemplate(Guid id)
        {
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) return NotFound();
            return template;
        }

        [HttpPost]
        public async Task<ActionResult<Template>> PostTemplate(Template template, CancellationToken cancellationToken = default)
        {
            template.Id = Guid.NewGuid();
            template.TenantId = _tenantProvider.GetTenantId();
            template.CriadoEm = DateTime.UtcNow;

            // Anti-XSS: higieniza o HTML hostil do cliente antes de persistir (allow-list).
            template.CorpoHtml = _htmlSanitizer.Sanitize(template.CorpoHtml);

            _context.Templates.Add(template);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTemplate(Guid id, Template template, CancellationToken cancellationToken = default)
        {
            if (id != template.Id) return BadRequest();

            var templateExistente = await _context.Templates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            if (templateExistente == null) return NotFound();

            if (await _context.Campaigns.AnyAsync(c => c.TenantId == _tenantProvider.GetTenantId()
                && c.EmailTemplateId == id && c.Status != CampaignStatus.Rascunho
                && (!c.SnapshotCapturedAtUtc.HasValue || c.EmailBodySnapshot == null), cancellationToken))
                return Conflict(new { message = "Este modelo possui campanha legada sem snapshot. Crie uma cópia até concluir a atualização do histórico." });

            templateExistente.Nome = template.Nome;
            templateExistente.Assunto = template.Assunto;
            templateExistente.RemetenteNome = template.RemetenteNome;
            templateExistente.RemetenteEmail = template.RemetenteEmail;
            // Anti-XSS: higieniza o HTML hostil do cliente antes de persistir (allow-list).
            templateExistente.CorpoHtml = _htmlSanitizer.Sanitize(template.CorpoHtml);

            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken cancellationToken = default)
        {
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            if (template == null) return NotFound();

            if (await _context.Campaigns.AnyAsync(c => c.TenantId == _tenantProvider.GetTenantId() && c.EmailTemplateId == id, cancellationToken))
                return Conflict(new { message = "Este modelo possui campanhas vinculadas e não pode ser excluído." });

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
