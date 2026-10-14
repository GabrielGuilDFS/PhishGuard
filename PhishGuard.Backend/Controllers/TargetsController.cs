using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Security;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhishGuard.Backend.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TenantMutationFilter))]
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
        public async Task<ActionResult<Target>> PostAlvo(TargetInputDto input, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized();
            var normalizedEmail = Target.NormalizeEmail(input.Email);
            if (await _context.Targets.AnyAsync(t => t.TenantId == tenantId && t.EmailNormalizado == normalizedEmail, cancellationToken))
                return Conflict(new { message = "Este e-mail já está cadastrado neste tenant." });
            var alvo = new Target
            {
                TenantId = tenantId,
                Nome = input.Nome.Trim(),
                Email = input.Email.Trim(),
                Departamento = input.Departamento.Trim()
            };

            // Trava de cota por plano: consulta o plano do Tenant e o total de alvos
            // já cadastrados (a contagem é automaticamente isolada pelo filtro
            // multi-tenant do AppDbContext) antes de aceitar um novo registro.
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == alvo.TenantId, cancellationToken);
            if (tenant is null)
            {
                return BadRequest("Tenant não encontrado para o usuário autenticado.");
            }

            var limiteDeAlvos = PlanoLimites.LimiteDeAlvos(tenant.Plano);
            var totalDeAlvos = await _context.Targets.CountAsync(cancellationToken);

            if (totalDeAlvos >= limiteDeAlvos)
            {
                return BadRequest(
                    $"Limite de {limiteDeAlvos} alvos do plano {tenant.Plano} atingido. " +
                    "Faça um upgrade de plano para cadastrar mais colaboradores.");
            }

            alvo.Id = Guid.NewGuid();

            _context.Targets.Add(alvo);
            await _context.SaveChangesAsync(cancellationToken);
            
            return CreatedAtAction(nameof(GetAlvos), new { id = alvo.Id }, alvo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlvo(Guid id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var alvo = await _context.Targets.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId, cancellationToken);
            
            if (alvo == null) return NotFound();

            if (await _context.Campaigns.AnyAsync(c => c.TenantId == tenantId && c.Targets.Any(t => t.Id == id), cancellationToken)
                || await _context.SimulationsLogs.AnyAsync(l => l.TenantId == tenantId && l.TargetId == id, cancellationToken)
                || await _context.CampaignDeliveries.AnyAsync(d => d.TenantId == tenantId && d.TargetId == id, cancellationToken))
                return Conflict(new { message = "Este alvo possui vínculos com campanhas ou histórico. Desvincule-o dos rascunhos antes de excluir; histórico de campanhas ativadas deve ser preservado." });

            _context.Targets.Remove(alvo);
            await _context.SaveChangesAsync(cancellationToken);
            
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlvo(Guid id, TargetInputDto alvo, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (alvo.Id.HasValue && id != alvo.Id) return BadRequest();

            var tenantId = _tenantProvider.GetTenantId();

            var alvoExistente = await _context.Targets.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId, cancellationToken);
            
            if (alvoExistente == null) return NotFound();

            var normalizedEmail = Target.NormalizeEmail(alvo.Email);
            var department = alvo.Departamento.Trim();
            var historicalChange = normalizedEmail != alvoExistente.EmailNormalizado || department != alvoExistente.Departamento;
            if (historicalChange &&
                (await _context.Campaigns.AnyAsync(c => c.TenantId == tenantId && c.Status != CampaignStatus.Rascunho
                    && (!c.SnapshotCapturedAtUtc.HasValue || c.CampaignTargets.Any(p => !p.SnapshotCapturedAtUtc.HasValue))
                    && c.Targets.Any(t => t.Id == id), cancellationToken)
                 || await _context.SimulationsLogs.AnyAsync(l => l.TenantId == tenantId && l.TargetId == id
                     && !_context.CampaignTargets.Any(p => p.TenantId == tenantId && p.CampaignsId == l.CampaignId
                         && p.TargetsId == id && p.SnapshotCapturedAtUtc.HasValue), cancellationToken)
                 || await _context.CampaignDeliveries.AnyAsync(d => d.TenantId == tenantId && d.TargetId == id
                     && !_context.CampaignTargets.Any(p => p.TenantId == tenantId && p.CampaignsId == d.CampaignId
                         && p.TargetsId == id && p.SnapshotCapturedAtUtc.HasValue), cancellationToken)))
                return Conflict(new { message = "Este alvo possui histórico legado sem snapshot. Conclua a atualização do histórico antes de alterar e-mail ou departamento." });
            if (await _context.Targets.AnyAsync(t => t.TenantId == tenantId && t.Id != id && t.EmailNormalizado == normalizedEmail, cancellationToken))
                return Conflict(new { message = "Este e-mail já está cadastrado neste tenant." });

            alvoExistente.Nome = alvo.Nome.Trim();
            alvoExistente.Email = alvo.Email.Trim();
            alvoExistente.Departamento = department;

            await _context.SaveChangesAsync(cancellationToken);
            
            return NoContent();
        }
    }
}
