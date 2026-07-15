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
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DashboardController(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        // GET /api/Dashboard/metrics
        // Métricas agregadas do tenant ativo para os cartões do painel.
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Targets e Campaigns já são isolados por tenant pelo Global Query Filter.
            var totalColaboradores = await _context.Targets.CountAsync();
            var campanhasAtivas = await _context.Campaigns
                .CountAsync(c => c.Status == CampaignStatus.EmAndamento);

            // SimulationLog NÃO tem Global Query Filter → escopo explícito por tenant.
            var logsTenant = _context.SimulationsLogs.Where(l => l.TenantId == tenantId);

            var emailsDisparados = await logsTenant.CountAsync(l => l.Acao == SimulationActions.Envio);

            // Risco = % de alvos que clicaram OU submeteram dados (distintos) sobre o total
            // de envios. Distinto por TargetId para não contar o mesmo alvo duas vezes.
            var alvosEngajados = await logsTenant
                .Where(l => l.Acao == SimulationActions.Clique || l.Acao == SimulationActions.Submissao)
                .Select(l => l.TargetId)
                .Distinct()
                .CountAsync();

            var riscoGlobal = emailsDisparados == 0
                ? 0
                : (int)Math.Round(100.0 * alvosEngajados / emailsDisparados);

            return Ok(new
            {
                totalColaboradores,
                campanhasAtivas,
                emailsDisparados,
                riscoGlobal
            });
        }

        // GET /api/Dashboard/funnel
        // Dados dos gráficos comparativos do painel:
        //  - Entregabilidade SMTP: disparos FEITOS (tentativas = Envio + Falha) vs ENTREGUES
        //    (Envio, que só é logado no sucesso do SendAsync).
        //  - Comportamento de risco: alvos DISTINTOS que clicaram vs que submeteram dados.
        [HttpGet("funnel")]
        public async Task<IActionResult> GetFunnel()
        {
            var tenantId = _tenantProvider.GetTenantId();

            // SimulationLog NÃO tem Global Query Filter → escopo explícito por tenant.
            var logsTenant = _context.SimulationsLogs.Where(l => l.TenantId == tenantId);

            var entregues = await logsTenant.CountAsync(l => l.Acao == SimulationActions.Envio);
            var falhas = await logsTenant.CountAsync(l => l.Acao == SimulationActions.Falha);

            // Distinto por TargetId: um mesmo alvo que clica/submete várias vezes conta uma vez.
            var cliques = await logsTenant
                .Where(l => l.Acao == SimulationActions.Clique)
                .Select(l => l.TargetId).Distinct().CountAsync();
            var submissoes = await logsTenant
                .Where(l => l.Acao == SimulationActions.Submissao)
                .Select(l => l.TargetId).Distinct().CountAsync();

            return Ok(new
            {
                disparosFeitos = entregues + falhas,
                entregues,
                falhas,
                cliques,
                submissoes
            });
        }

        // GET /api/Dashboard/departments
        // Vulnerabilidade por departamento: colaboradores, e-mails recebidos, cliques e o
        // percentual de risco (engajados / envios) de cada setor do tenant.
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Colaboradores por departamento (Targets tenant-scoped pelo Query Filter).
            var colaboradoresPorDepto = await _context.Targets
                .GroupBy(t => t.Departamento)
                .Select(g => new { Departamento = g.Key, Colaboradores = g.Count() })
                .ToListAsync();

            // Logs do tenant + departamento do alvo. Agregação feita em memória (volume por
            // tenant é modesto neste contexto) para evitar tradução frágil de GroupBy+Distinct.
            var logsComDepto = await (
                from l in _context.SimulationsLogs.Where(l => l.TenantId == tenantId)
                join t in _context.Targets on l.TargetId equals t.Id
                select new { t.Departamento, l.TargetId, l.Acao })
                .ToListAsync();

            var agregadoPorDepto = logsComDepto
                .GroupBy(x => x.Departamento)
                .ToDictionary(g => g.Key ?? string.Empty, g => new
                {
                    Emails = g.Count(x => x.Acao == SimulationActions.Envio),
                    Cliques = g.Where(x => x.Acao == SimulationActions.Clique)
                               .Select(x => x.TargetId).Distinct().Count(),
                    Engajados = g.Where(x => x.Acao == SimulationActions.Clique || x.Acao == SimulationActions.Submissao)
                                 .Select(x => x.TargetId).Distinct().Count()
                });

            var departamentos = colaboradoresPorDepto
                .Select(d =>
                {
                    var chave = d.Departamento ?? string.Empty;
                    agregadoPorDepto.TryGetValue(chave, out var ag);
                    var emails = ag?.Emails ?? 0;
                    var cliques = ag?.Cliques ?? 0;
                    var engajados = ag?.Engajados ?? 0;
                    var risco = emails == 0 ? 0 : (int)Math.Round(100.0 * engajados / emails);

                    return new
                    {
                        id = string.IsNullOrWhiteSpace(chave) ? "(sem-departamento)" : chave,
                        name = string.IsNullOrWhiteSpace(chave) ? "(Sem departamento)" : chave,
                        employees = d.Colaboradores,
                        emails,
                        clicks = cliques,
                        risk = $"{risco}%"
                    };
                })
                .OrderByDescending(d => d.employees)
                .ToList();

            return Ok(departamentos);
        }
    }
}
