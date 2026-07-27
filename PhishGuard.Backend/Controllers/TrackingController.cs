using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PhishGuard.Backend.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _appBaseUrl;

        public TrackingController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            // URL PÚBLICA do frontend (landing/feedback), destino dos redirects de rastreamento.
            // Precisa ser acessível PELO ALVO; default localhost apenas para dev local.
            var appBase = configuration["AppSettings:PublicAppBaseUrl"]?.TrimEnd('/');
            _appBaseUrl = string.IsNullOrWhiteSpace(appBase) ? "http://localhost:5173" : appBase;
        }

        private async Task<Campaign> RegistrarAcao(Guid campaignId, Guid targetId, string acao)
        {
            // Busca a campanha no banco para obter o TenantId (sem sessão do alvo logado)
            var campaign = await _context.Campaigns
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null)
            {
                return null;
            }

            // HARDENING (anti-poluição de métricas): o endpoint é anônimo, então o targetId
            // da rota é ARBITRÁRIO. Sem esta checagem, um caller podia POSTar
            // submit/{campanhaReal}/{guidQualquer} e injetar logs órfãos, falseando os
            // relatórios do tenant. Exige que o alvo exista e pertença ao MESMO tenant da
            // campanha (IgnoreQueryFilters porque não há sessão/tenant no tracking).
            var alvoValido = await _context.Targets
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Id == targetId && t.TenantId == campaign.TenantId);

            if (!alvoValido)
            {
                return null;
            }

            // Verifica se já existe um log idêntico para evitar duplicação (flood)
            var logExistente = await _context.SimulationsLogs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.CampaignId == campaignId && l.TargetId == targetId && l.Acao == acao);

            if (logExistente == null)
            {
                // Captura o IP
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";
                
                // Limita a string do IP a 45 caracteres
                if (ip.Length > 45)
                {
                    ip = ip.Substring(0, 45);
                }

                // Salva o log no banco
                var log = new SimulationLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = campaign.TenantId,
                    CampaignId = campaignId,
                    TargetId = targetId,
                    Acao = acao,
                    DataHora = DateTime.UtcNow,
                    IpOrigem = ip
                };

                _context.SimulationsLogs.Add(log);
                await _context.SaveChangesAsync();
            }

            return campaign;
        }

        [HttpGet("open/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackOpen(Guid campaignId, Guid targetId)
        {
            await RegistrarAcao(campaignId, targetId, "Abertura");

            // Retorna um GIF transparente de 1x1 pixel em Base64
            byte[] pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            return File(pixel, "image/gif");
        }

        [HttpGet("click/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackClick(Guid campaignId, Guid targetId)
        {
            var campaign = await RegistrarAcao(campaignId, targetId, "Clique");

            if (campaign != null)
            {
                // Redireciona para a página falsa do React
                var url = $"{_appBaseUrl}/landing/{campaign.LandingPageId}?c={campaignId}&t={targetId}";
                return Redirect(url);
            }

            return Redirect($"{_appBaseUrl}/");
        }

        [HttpPost("submit/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackSubmit(Guid campaignId, Guid targetId, [FromBody] CaptureMetadataDto metadata = null)
        {
            // Registra o evento "Submissão de Dados" (o alvo digitou credenciais na landing simulada).
            // NOTA LGPD: 'metadata' contém apenas propriedades de validação (flags/tamanho).
            // A senha real NÃO é recebida nem armazenada — só o evento em si é a métrica do TCC.
            var campaign = await RegistrarAcao(campaignId, targetId, "Submissão de Dados");

            if (campaign == null)
            {
                return NotFound(new { message = "Campanha não encontrada ou link expirado." });
            }

            // Devolve a rota educacional interna para a landing redirecionar o alvo:
            // após "inserir dados", ele é levado ao treinamento de conscientização.
            return Ok(new { status = "Inseriu Dados", redirectUrl = $"/educational-feedback?campaign={campaignId}" });
        }

        [HttpPost("complete/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackComplete(Guid campaignId, Guid targetId)
        {
            // Registra a CONCLUSÃO do módulo educacional (Just-in-Time Training) do alvo,
            // gerando trilha de auditoria de que o treinamento foi consumido. Idempotente
            // (RegistrarAcao ignora duplicatas) e anônimo — nenhum dado sensível é coletado.
            var campaign = await RegistrarAcao(campaignId, targetId, SimulationActions.TreinamentoConcluido);

            if (campaign == null)
            {
                return NotFound(new { message = "Campanha não encontrada ou link expirado." });
            }

            return Ok(new { status = "Treinamento concluído" });
        }

        [HttpGet("educational/{campaignId}")]
        public async Task<IActionResult> GetEducational(Guid campaignId)
        {
            // Resolve a campanha (sem sessão do alvo) para descobrir qual página
            // educacional foi configurada como feedback do treinamento.
            var campaign = await _context.Campaigns
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null)
            {
                return NotFound(new { message = "Campanha não encontrada ou link expirado." });
            }

            var pagina = await _context.EducationalPages
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == campaign.EducationalPageId);

            if (pagina == null)
            {
                return NotFound(new { message = "Página educacional não configurada para esta campanha." });
            }

            return Ok(new { conteudoHtml = pagina.HtmlEducacional });
        }
    }
}
