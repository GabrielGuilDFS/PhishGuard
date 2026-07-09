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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            alvo.TenantId = _tenantProvider.GetTenantId();

            // Trava de cota por plano: consulta o plano do Tenant e o total de alvos
            // já cadastrados (a contagem é automaticamente isolada pelo filtro
            // multi-tenant do AppDbContext) antes de aceitar um novo registro.
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == alvo.TenantId);
            if (tenant is null)
            {
                return BadRequest("Tenant não encontrado para o usuário autenticado.");
            }

            var limiteDeAlvos = PlanoLimites.LimiteDeAlvos(tenant.Plano);
            var totalDeAlvos = await _context.Targets.CountAsync();

            if (totalDeAlvos >= limiteDeAlvos)
            {
                return BadRequest(
                    $"Limite de {limiteDeAlvos} alvos do plano {tenant.Plano} atingido. " +
                    "Faça um upgrade de plano para cadastrar mais colaboradores.");
            }

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