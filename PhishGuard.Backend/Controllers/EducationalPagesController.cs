using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Content;
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
    public class EducationalPagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHtmlSanitizationService _htmlSanitizer;

        public EducationalPagesController(AppDbContext context, ITenantProvider tenantProvider, IHtmlSanitizationService htmlSanitizer)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _htmlSanitizer = htmlSanitizer;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPages()
        {
            var paginas = await _context.EducationalPages
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync();

            return Ok(paginas.Select(p => new
            {
                p.Id, p.Nome,
                conteudoHtml = OfficialPageCatalog.RestoreLegacyEducationalClasses(p.HtmlEducacional, _htmlSanitizer.Sanitize)
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPage(Guid id)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant (FindAsync contornaria o Query Filter).
            var page = await _context.EducationalPages
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
            if (page == null) return NotFound();

            return Ok(new 
            {
                page.Id,
                page.Nome,
                conteudoHtml = OfficialPageCatalog.RestoreLegacyEducationalClasses(page.HtmlEducacional, _htmlSanitizer.Sanitize)
            });
        }

        [HttpPost]
        public async Task<ActionResult> PostPage([FromBody] PageInputDto input, CancellationToken cancellationToken = default)
        {
            var novaPagina = new EducationalPage
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantProvider.GetTenantId(),
                Nome = input.Nome,
                HtmlEducacional = _htmlSanitizer.Sanitize(input.ConteudoHtml),
                CriadoEm = DateTime.UtcNow
            };

            _context.EducationalPages.Add(novaPagina);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetPage), new { id = novaPagina.Id }, new 
            {
                novaPagina.Id,
                novaPagina.Nome,
                conteudoHtml = novaPagina.HtmlEducacional
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPage(Guid id, [FromBody] PageInputDto input, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant (FindAsync contornaria o Query Filter).
            var pageExistente = await _context.EducationalPages
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
            if (pageExistente == null) return NotFound();

            if (await _context.Campaigns.AnyAsync(c => c.TenantId == tenantId && c.EducationalPageId == id
                && c.Status != CampaignStatus.Rascunho && (!c.SnapshotCapturedAtUtc.HasValue || c.EducationalHtmlSnapshot == null), cancellationToken))
                return Conflict(new { message = "Esta página possui campanha legada sem snapshot. Crie uma cópia até concluir a atualização do histórico." });

            pageExistente.Nome = input.Nome;
            pageExistente.HtmlEducacional = _htmlSanitizer.Sanitize(input.ConteudoHtml);

            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(Guid id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant (FindAsync contornaria o Query Filter).
            var page = await _context.EducationalPages
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
            if (page == null) return NotFound();

            if (await _context.Campaigns.AnyAsync(c => c.TenantId == tenantId && c.EducationalPageId == id, cancellationToken))
                return Conflict(new { message = "Esta página possui campanhas vinculadas e não pode ser excluída." });

            _context.EducationalPages.Remove(page);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
