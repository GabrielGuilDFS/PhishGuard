using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhishGuard.Backend.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class TargetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider; // 1. Adicionamos o Provider

        public TargetsController(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Target>>> GetAlvos()
        {
            return await _context.Targets.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Target>> PostAlvo(Target alvo)
        {

            alvo.TenantId = _tenantProvider.GetTenantId(); 

            alvo.Id = Guid.NewGuid();

            _context.Targets.Add(alvo);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetAlvos), new { id = alvo.Id }, alvo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlvo(Guid id)
        {
            var alvo = await _context.Targets.FirstOrDefaultAsync(a => a.Id == id);
            
            if (alvo == null) return NotFound();

            _context.Targets.Remove(alvo);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlvo(Guid id, Target alvo)
        {
            if (id != alvo.Id) return BadRequest();

            var alvoExistente = await _context.Targets.FirstOrDefaultAsync(a => a.Id == id);
            
            if (alvoExistente == null) return NotFound();

            alvoExistente.Nome = alvo.Nome;
            alvoExistente.Email = alvo.Email;
            alvoExistente.Departamento = alvo.Departamento;

            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}