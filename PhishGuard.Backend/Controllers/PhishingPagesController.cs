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
    public class PhishingPagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public PhishingPagesController(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhishingPage>>> GetPages()
        {
            return await _context.PhishingPages.OrderByDescending(p => p.CriadoEm).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PhishingPage>> GetPage(Guid id)
        {
            var page = await _context.PhishingPages.FirstOrDefaultAsync(p => p.Id == id);
            if (page == null) return NotFound();
            return page;
        }

        [HttpPost]
        public async Task<ActionResult<PhishingPage>> PostPage(PhishingPage page)
        {
            page.Id = Guid.NewGuid();
            page.TenantId = _tenantProvider.GetTenantId();
            page.CriadoEm = DateTime.UtcNow;

            _context.PhishingPages.Add(page);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPage(Guid id, PhishingPage page)
        {
            if (id != page.Id) return BadRequest();

            var pageExistente = await _context.PhishingPages.FirstOrDefaultAsync(p => p.Id == id);
            if (pageExistente == null) return NotFound();

            pageExistente.Nome = page.Nome;
            pageExistente.ConteudoHtml = page.ConteudoHtml;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(Guid id)
        {
            var page = await _context.PhishingPages.FirstOrDefaultAsync(p => p.Id == id);
            if (page == null) return NotFound();

            _context.PhishingPages.Remove(page);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}