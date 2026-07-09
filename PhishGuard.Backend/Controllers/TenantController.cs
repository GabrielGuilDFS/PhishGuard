using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using System.Threading.Tasks;

namespace PhishGuard.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public TenantController(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        /// <summary>
        /// Expõe a cota de alvos do plano ativo do Tenant para que a UI possa
        /// desabilitar o cadastro quando o limite for atingido.
        /// </summary>
        [HttpGet("quota")]
        public async Task<IActionResult> GetQuotaAlvos()
        {
            var tenantId = _tenantProvider.GetTenantId();

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant is null)
            {
                return NotFound("Tenant não encontrado para o usuário autenticado.");
            }

            var limiteAlvos = PlanoLimites.LimiteDeAlvos(tenant.Plano);
            var totalAlvos = await _context.Targets.CountAsync();

            return Ok(new
            {
                plano = tenant.Plano.ToString(),
                limiteAlvos,
                totalAlvos,
                limiteAtingido = totalAlvos >= limiteAlvos
            });
        }
    }
}
