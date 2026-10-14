using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Content;
using PhishGuard.Backend.Contracts;
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
    public class PhishingPagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHtmlSanitizationService _htmlSanitizer;
        private readonly TrackingCampaignScope? _trackingCampaignScope;

        public PhishingPagesController(
            AppDbContext context,
            ITenantProvider tenantProvider,
            IHtmlSanitizationService htmlSanitizer,
            TrackingCampaignScope? trackingCampaignScope = null)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _htmlSanitizer = htmlSanitizer;
            _trackingCampaignScope = trackingCampaignScope;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPages()
        {
            var paginas = await _context.PhishingPages
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => new 
                {
                    p.Id,
                    p.Nome,
                    conteudoHtml = p.HtmlCaptura 
                })
                .ToListAsync();

            return Ok(paginas);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetPage(Guid id,
            [FromQuery(Name = TrackingContract.QueryCampaign)] Guid? campaignId = null,
            [FromQuery(Name = TrackingContract.QueryTarget)] Guid? targetId = null,
            [FromQuery(Name = TrackingContract.QueryToken)] string? trackingToken = null,
            CancellationToken cancellationToken = default)
        {
            // Anonymous access is limited to the signed participant's campaign.
            // Knowing a page GUID never grants access to another tenant's catalog.
            if (campaignId.HasValue && targetId.HasValue)
            {
                if (_trackingCampaignScope is null) return NotFound();
                using var trackingScope = await _trackingCampaignScope.ResolveAsync(
                    campaignId.Value, targetId.Value, trackingToken, cancellationToken);
                if (trackingScope is null || trackingScope.Campaign.LandingPageId != id) return NotFound();
                var campaign = trackingScope.Campaign;
                var page = await _context.PhishingPages.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.TenantId == campaign.TenantId && p.Id == id, cancellationToken);
                if (page is null) return NotFound();
                var templateId = campaign.SnapshotCapturedAtUtc.HasValue
                    ? campaign.LandingTemplateIdSnapshot : OfficialPageCatalog.ResolveLandingId(page.HtmlCaptura);
                var html = _htmlSanitizer.Sanitize(OfficialPageCatalog.ResolveLandingHtml(campaign.LandingHtmlSnapshot ?? page.HtmlCaptura));
                return Ok(new
                {
                    id, nome = campaign.LandingNameSnapshot ?? page.Nome,
                    conteudoHtml = OfficialPageCatalog.RestoreLegacyLandingClasses(html, templateId, _htmlSanitizer.Sanitize),
                    templateId,
                    isSnapshot = campaign.SnapshotCapturedAtUtc.HasValue,
                    isLegacySnapshot = campaign.IsLegacySnapshot || !campaign.SnapshotCapturedAtUtc.HasValue
                });
            }
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty) return NotFound();
            var currentPage = await _context.PhishingPages.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
            if (currentPage is null) return NotFound();
            return Ok(new { currentPage.Id, currentPage.Nome, conteudoHtml = currentPage.HtmlCaptura });
        }

        [HttpPost]
        public async Task<ActionResult> PostPage([FromBody] PageInputDto input, CancellationToken cancellationToken = default)
        {
            var novaPagina = new PhishingPage
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantProvider.GetTenantId(),
                Nome = input.Nome,

                // Anti-XSS: higieniza o HTML hostil do cliente antes de persistir (allow-list).
                HtmlCaptura = _htmlSanitizer.Sanitize(input.ConteudoHtml),

                CriadoEm = DateTime.UtcNow
            };

            _context.PhishingPages.Add(novaPagina);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetPage), new { id = novaPagina.Id }, new 
            {
                novaPagina.Id,
                novaPagina.Nome,
                conteudoHtml = novaPagina.HtmlCaptura
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPage(Guid id, [FromBody] PageInputDto input, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant (FindAsync contornaria o Query Filter).
            var pageExistente = await _context.PhishingPages
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
            if (pageExistente == null) return NotFound();

            if (await _context.Campaigns.AnyAsync(c => c.TenantId == tenantId && c.LandingPageId == id
                && c.Status != CampaignStatus.Rascunho && (!c.SnapshotCapturedAtUtc.HasValue || c.LandingHtmlSnapshot == null), cancellationToken))
                return Conflict(new { message = "Esta página possui campanha legada sem snapshot. Crie uma cópia até concluir a atualização do histórico." });

            pageExistente.Nome = input.Nome;

            // Anti-XSS: higieniza o HTML hostil do cliente antes de persistir (allow-list).
            pageExistente.HtmlCaptura = _htmlSanitizer.Sanitize(input.ConteudoHtml);

            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(Guid id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant (FindAsync contornaria o Query Filter).
            var page = await _context.PhishingPages
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
            if (page == null) return NotFound();

            if (await _context.Campaigns.AnyAsync(c => c.TenantId == tenantId && c.LandingPageId == id, cancellationToken))
                return Conflict(new { message = "Esta página possui campanhas vinculadas e não pode ser excluída." });

            _context.PhishingPages.Remove(page);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }

    public class PageInputDto
    {
        public string Nome { get; set; } = string.Empty;
        public string ConteudoHtml { get; set; } = string.Empty;
    }
}
