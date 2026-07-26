using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            // ── KPIs consolidados do topo do dashboard ────────────────────────────
            // Total de campanhas DISPARADAS: distintas com ao menos um log de Envio.
            var totalCampanhas = await logsTenant
                .Where(l => l.Acao == SimulationActions.Envio)
                .Select(l => l.CampaignId)
                .Distinct()
                .CountAsync();

            // Cliques ÚNICOS acumulados: par único (CampaignId, TargetId) com Acao=Clique
            // (1 clique por usuário por campanha). Distinct sobre 2 colunas →
            // COUNT(*) FROM (SELECT DISTINCT campaign_id, target_id ...).
            var cliquesUnicosAcumulados = await logsTenant
                .Where(l => l.Acao == SimulationActions.Clique)
                .Select(l => new { l.CampaignId, l.TargetId })
                .Distinct()
                .CountAsync();

            // Volume BRUTO de interações: todas as linhas de interação capturadas
            // (clique + submissão + abertura) — telemetria de rede para auditoria.
            var volumeBrutoInteracoes = await logsTenant
                .CountAsync(l => l.Acao == SimulationActions.Clique
                              || l.Acao == SimulationActions.Submissao
                              || l.Acao == SimulationActions.Abertura);

            return Ok(new
            {
                // Campos legados mantidos p/ compatibilidade (não são mais exibidos como cards).
                totalColaboradores,
                campanhasAtivas,
                emailsDisparados,
                riscoGlobal,
                // KPIs novos do topo.
                totalCampanhas,
                cliquesUnicosAcumulados,
                volumeBrutoInteracoes
            });
        }

        // GET /api/Dashboard/campaign-clicks
        // Alimenta o gráfico de barras por campanha: cliques ÚNICOS (par único
        // CampaignId+TargetId) de cada campanha DISPARADA, com o rótulo já concatenado
        // "Nome (Mês)" pronto para o eixo X do frontend.
        [HttpGet("campaign-clicks")]
        public async Task<IActionResult> GetCampaignClicks()
        {
            var tenantId = _tenantProvider.GetTenantId();

            // SimulationLog NÃO tem Global Query Filter → escopo explícito por tenant.
            // Agregação distinta em MEMÓRIA (volume por tenant é modesto) para evitar a
            // tradução frágil de Distinct+GroupBy — mesmo padrão de GetDepartments.
            var logs = await _context.SimulationsLogs
                .Where(l => l.TenantId == tenantId)
                .Select(l => new { l.CampaignId, l.TargetId, l.Acao })
                .ToListAsync();

            // Campanhas efetivamente disparadas (têm log de Envio).
            var campanhasDisparadas = logs
                .Where(x => x.Acao == SimulationActions.Envio)
                .Select(x => x.CampaignId)
                .ToHashSet();

            // Regra de par único: 1 clique por (campanha, alvo).
            var cliquesUnicosPorCampanha = logs
                .Where(x => x.Acao == SimulationActions.Clique)
                .GroupBy(x => x.CampaignId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.TargetId).Distinct().Count());

            // Nome + mês de disparo (DataInicio). Campaigns é tenant-scoped pelo Query Filter.
            var campanhas = await _context.Campaigns
                .Select(c => new { c.Id, c.NomeCampanha, c.DataInicio })
                .ToListAsync();

            var ptBr = new CultureInfo("pt-BR");
            var resultado = campanhas
                .Where(c => campanhasDisparadas.Contains(c.Id))
                .Select(c => new CampaignClicksDto
                {
                    Label = $"{c.NomeCampanha} | {MesAbreviado(c.DataInicio, ptBr)}",
                    Clicks = cliquesUnicosPorCampanha.TryGetValue(c.Id, out var n) ? n : 0
                })
                .OrderByDescending(r => r.Clicks)
                .ThenBy(r => r.Label)
                .ToList();

            return Ok(resultado);
        }

        // Mês abreviado e capitalizado em pt-BR: 7 → "Jul". Deriva do nome completo
        // ("julho") para não depender do formato de GetAbbreviatedMonthName (que varia
        // com ponto/sem ponto entre runtimes).
        private static string MesAbreviado(DateTime data, CultureInfo cultura)
        {
            var nome = cultura.DateTimeFormat.GetMonthName(data.Month);
            if (string.IsNullOrEmpty(nome)) return string.Empty;
            var abrev = nome.Length >= 3 ? nome.Substring(0, 3) : nome;
            return char.ToUpper(abrev[0], cultura) + abrev.Substring(1);
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
            // Inclui CampaignId: cada e-mail enviado é um par único (CampaignId, TargetId).
            var logsComDepto = await (
                from l in _context.SimulationsLogs.Where(l => l.TenantId == tenantId)
                join t in _context.Targets on l.TargetId equals t.Id
                select new { t.Departamento, l.CampaignId, l.TargetId, l.Acao })
                .ToListAsync();

            var agregadoPorDepto = logsComDepto
                .GroupBy(x => x.Departamento)
                .ToDictionary(g => g.Key ?? string.Empty, g => new
                {
                    // E-mails RECEBIDOS pelo depto: total de disparos (uma linha de Envio por e-mail).
                    Emails = g.Count(x => x.Acao == SimulationActions.Envio),
                    // E-mails COMPROMETIDOS: e-mails ÚNICOS clicados. Cada e-mail é o par
                    // (CampaignId, TargetId) — o MESMO colaborador que caiu em 6 e-mails distintos
                    // conta 6, não 1. Cliques repetidos NO MESMO e-mail não inflam (Distinct).
                    Cliques = g.Where(x => x.Acao == SimulationActions.Clique)
                               .Select(x => new { x.CampaignId, x.TargetId }).Distinct().Count()
                });

            var departamentos = colaboradoresPorDepto
                .Select(d =>
                {
                    var chave = d.Departamento ?? string.Empty;
                    agregadoPorDepto.TryGetValue(chave, out var ag);
                    var emails = ag?.Emails ?? 0;
                    var cliques = ag?.Cliques ?? 0;
                    // Risco (%) = e-mails comprometidos / e-mails recebidos, com 1 casa decimal
                    // (ex.: 5 clicados de 6 recebidos = 83,3%). Antes usava alvos DISTINTOS no
                    // numerador, o que subestimava grosseiramente o risco de quem caía em vários.
                    var risco = emails == 0 ? 0.0 : Math.Round(100.0 * cliques / emails, 1);

                    return new
                    {
                        id = string.IsNullOrWhiteSpace(chave) ? "(sem-departamento)" : chave,
                        name = string.IsNullOrWhiteSpace(chave) ? "(Sem departamento)" : chave,
                        employees = d.Colaboradores,
                        emails,
                        clicks = cliques,
                        risk = risco.ToString("0.#", CultureInfo.InvariantCulture) + "%"
                    };
                })
                .OrderByDescending(d => d.employees)
                .ToList();

            return Ok(departamentos);
        }

        // GET /api/Dashboard/dept-risk-timeline
        // Série temporal de risco por departamento: para cada mês com disparos, calcula
        // a Taxa de Cliques e a Taxa de Submissões de cada setor (par único CampaignId+TargetId).
        // O payload é achatado para consumo direto pelo Recharts LineChart:
        //   { departamentos: ["TI","RH"], timeline: [{ mes:"Jan/25", ordem:202501, TI_cliques:33.3, ... }] }
        [HttpGet("dept-risk-timeline")]
        public async Task<IActionResult> GetDeptRiskTimeline()
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Join: SimulationLog → Target (departamento) + Campaign (DataInicio = mês de disparo).
            // SimulationLog NÃO tem Global Query Filter → escopo explícito por tenant.
            // Campaigns e Targets SÃO tenant-scoped pelo Query Filter.
            var logsJoin = await (
                from l in _context.SimulationsLogs.Where(l => l.TenantId == tenantId)
                join t in _context.Targets on l.TargetId equals t.Id
                join c in _context.Campaigns on l.CampaignId equals c.Id
                select new
                {
                    Ano = c.DataInicio.Year,
                    Mes = c.DataInicio.Month,
                    Departamento = t.Departamento ?? string.Empty,
                    l.CampaignId,
                    l.TargetId,
                    l.Acao
                })
                .ToListAsync();

            if (logsJoin.Count == 0)
                return Ok(new DeptRiskTimelineResponse());

            // Agrupar por (Ano, Mês, Departamento) e calcular taxas com regra de par único.
            var grupos = logsJoin
                .GroupBy(x => new { x.Ano, x.Mes, x.Departamento })
                .Select(g =>
                {
                    var envios = g.Count(x => x.Acao == SimulationActions.Envio);
                    var cliquesUnicos = g
                        .Where(x => x.Acao == SimulationActions.Clique)
                        .Select(x => new { x.CampaignId, x.TargetId })
                        .Distinct().Count();
                    var submisoesUnicas = g
                        .Where(x => x.Acao == SimulationActions.Submissao)
                        .Select(x => new { x.CampaignId, x.TargetId })
                        .Distinct().Count();

                    return new
                    {
                        g.Key.Ano,
                        g.Key.Mes,
                        g.Key.Departamento,
                        TaxaCliques = envios == 0 ? 0.0 : Math.Round(100.0 * cliquesUnicos / envios, 1),
                        TaxaSubmissoes = envios == 0 ? 0.0 : Math.Round(100.0 * submisoesUnicas / envios, 1)
                    };
                })
                .ToList();

            var departamentos = grupos
                .Select(g => g.Departamento)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Montar a timeline achatada: um dicionário por mês com chaves dinâmicas.
            var ptBr = new CultureInfo("pt-BR");
            var meses = grupos
                .Select(g => new { g.Ano, g.Mes })
                .Distinct()
                .OrderBy(m => m.Ano).ThenBy(m => m.Mes)
                .ToList();

            var timeline = new List<Dictionary<string, object>>();
            foreach (var m in meses)
            {
                var ponto = new Dictionary<string, object>
                {
                    ["mes"] = MesAbreviado(new DateTime(m.Ano, m.Mes, 1), ptBr) + "/" + (m.Ano % 100).ToString("D2"),
                    ["ordem"] = m.Ano * 100 + m.Mes
                };

                foreach (var dept in departamentos)
                {
                    var g = grupos.FirstOrDefault(x => x.Ano == m.Ano && x.Mes == m.Mes && x.Departamento == dept);
                    ponto[$"{dept}_cliques"] = g?.TaxaCliques ?? 0.0;
                    ponto[$"{dept}_submissoes"] = g?.TaxaSubmissoes ?? 0.0;
                }

                timeline.Add(ponto);
            }

            return Ok(new DeptRiskTimelineResponse
            {
                Departamentos = departamentos,
                Timeline = timeline
            });
        }
    }
}
