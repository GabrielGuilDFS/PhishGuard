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
        public async Task<ActionResult<object>> GetPage(Guid id)
        {
            var page = await _context.PhishingPages.FindAsync(id);
            if (page == null) return NotFound();

            return Ok(new 
            {
                page.Id,
                page.Nome,
                conteudoHtml = page.HtmlCaptura 
            });
        }

        [HttpPost]
        public async Task<ActionResult> PostPage([FromBody] PageInputDto input)
        {
            var novaPagina = new PhishingPage
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantProvider.GetTenantId(),
                Nome = input.Nome,
                
                HtmlCaptura = input.ConteudoHtml, 
                
                CriadoEm = DateTime.UtcNow
            };

            _context.PhishingPages.Add(novaPagina);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPage), new { id = novaPagina.Id }, new 
            {
                novaPagina.Id,
                novaPagina.Nome,
                conteudoHtml = novaPagina.HtmlCaptura
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPage(Guid id, [FromBody] PageInputDto input)
        {
            var pageExistente = await _context.PhishingPages.FindAsync(id);
            if (pageExistente == null) return NotFound();

            pageExistente.Nome = input.Nome;
            
            pageExistente.HtmlCaptura = input.ConteudoHtml; 

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(Guid id)
        {
            var page = await _context.PhishingPages.FindAsync(id);
            if (page == null) return NotFound();

            _context.PhishingPages.Remove(page);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class PageInputDto
    {
        public string Nome { get; set; }
        public string ConteudoHtml { get; set; } 
    }
}