using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishGuard.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public TemplatesController(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
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
        public async Task<ActionResult<Template>> PostTemplate(Template template)
        {
            template.Id = Guid.NewGuid();
            template.TenantId = _tenantProvider.GetTenantId();
            template.CriadoEm = DateTime.UtcNow;

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTemplate(Guid id, Template template)
        {
            if (id != template.Id) return BadRequest();

            var templateExistente = await _context.Templates.FirstOrDefaultAsync(t => t.Id == id);
            if (templateExistente == null) return NotFound();

            templateExistente.Nome = template.Nome;
            templateExistente.Assunto = template.Assunto;
            templateExistente.RemetenteNome = template.RemetenteNome;
            templateExistente.RemetenteEmail = template.RemetenteEmail;
            templateExistente.CorpoHtml = template.CorpoHtml;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) return NotFound();

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}