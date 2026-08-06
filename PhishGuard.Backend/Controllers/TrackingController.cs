using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PhishGuard.Backend.Contracts;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using System;
using System.Linq;
using System.Threading;
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
        private readonly ITrackingTokenService _trackingTokenService;

        public TrackingController(
            AppDbContext context,
            IConfiguration configuration,
            ITrackingTokenService trackingTokenService)
        {
            _context = context;
            _trackingTokenService = trackingTokenService;
            // URL PÚBLICA do frontend (landing/feedback), destino dos redirects de rastreamento.
            // Precisa ser acessível PELO ALVO; default localhost apenas para dev local.
            var appBase = configuration["AppSettings:PublicAppBaseUrl"]?.TrimEnd('/');
            _appBaseUrl = string.IsNullOrWhiteSpace(appBase) ? "http://localhost:5173" : appBase;
        }

        private async Task<Campaign?> RegistrarAcao(
            Guid campaignId,
            Guid targetId,
            string acao,
            string? trackingToken,
            CancellationToken cancellationToken = default)
        {
            if (!_trackingTokenService.Validate(trackingToken, campaignId, targetId))
                return null;

            // Busca a campanha no banco para obter o TenantId (sem sessão do alvo logado)
            var campaign = await _context.Campaigns
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

            if (campaign == null)
            {
                return null;
            }

            // HARDENING (anti-poluição de métricas): o endpoint é anônimo, então o targetId
            // da rota é ARBITRÁRIO. Sem esta checagem, um caller podia POSTar
            // submit/{campanhaReal}/{guidQualquer} e injetar logs órfãos, falseando os
            // relatórios do tenant. Exige que o alvo exista e pertença ao MESMO tenant da
            // campanha (IgnoreQueryFilters porque não há sessão/tenant no tracking).
            var alvoValido = await _context.Campaigns
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == campaignId
                    && c.TenantId == campaign.TenantId
                    && c.Targets.Any(t => t.Id == targetId && t.TenantId == campaign.TenantId),
                    cancellationToken);

            if (!alvoValido)
            {
                return null;
            }

            // Preserva a ordem mínima do funil e bloqueia chamadas anônimas que tentem
            // fabricar eventos apenas conhecendo os GUIDs. A conclusão aceita Clique
            // como pré-requisito porque o fluxo educativo também pode ser acessado pela
            // ação "Cancelar", sem submissão de dados.
            var acaoAnteriorObrigatoria = acao switch
            {
                SimulationActions.Abertura => SimulationActions.Envio,
                SimulationActions.Clique => SimulationActions.Envio,
                SimulationActions.Submissao => SimulationActions.Clique,
                SimulationActions.PaginaEducacionalVisualizada => SimulationActions.Clique,
                SimulationActions.TreinamentoConcluido => SimulationActions.PaginaEducacionalVisualizada,
                _ => null
            };

            if (acaoAnteriorObrigatoria is not null)
            {
                var possuiAcaoAnterior = await _context.SimulationsLogs
                    .IgnoreQueryFilters()
                    .AnyAsync(l => l.CampaignId == campaignId
                        && l.TargetId == targetId
                        && l.Acao == acaoAnteriorObrigatoria,
                        cancellationToken);

                if (!possuiAcaoAnterior)
                    return null;
            }

            // Verifica se já existe um log idêntico para evitar duplicação (flood)
            var logExistente = await _context.SimulationsLogs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.CampaignId == campaignId
                    && l.TargetId == targetId
                    && l.Acao == acao,
                    cancellationToken);

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
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException ex) when (ex.GetBaseException() is PostgresException
                    { SqlState: PostgresErrorCodes.UniqueViolation })
                {
                    // Outra requisição venceu a corrida pelo mesmo
                    // (CampaignId, TargetId, Acao). O resultado funcional já existe.
                    _context.Entry(log).State = EntityState.Detached;
                }
            }

            return campaign;
        }

        [HttpGet("open/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackOpen(
            Guid campaignId,
            Guid targetId,
            CancellationToken cancellationToken = default,
            [FromQuery(Name = TrackingContract.QueryToken)] string? trackingToken = null)
        {
            await RegistrarAcao(campaignId, targetId, SimulationActions.Abertura, trackingToken, cancellationToken);

            Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";

            // Retorna um GIF transparente de 1x1 pixel em Base64
            byte[] pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            return File(pixel, "image/gif");
        }

        [HttpGet("click/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackClick(
            Guid campaignId,
            Guid targetId,
            CancellationToken cancellationToken = default,
            [FromQuery(Name = TrackingContract.QueryToken)] string? trackingToken = null)
        {
            var campaign = await RegistrarAcao(
                campaignId,
                targetId,
                SimulationActions.Clique,
                trackingToken,
                cancellationToken);

            if (campaign != null)
            {
                // Redireciona para a página falsa do React
                var tokenQuery = string.IsNullOrEmpty(trackingToken)
                    ? string.Empty
                    : $"&k={Uri.EscapeDataString(trackingToken)}";
                var url = $"{_appBaseUrl}/landing/{campaign.LandingPageId}?c={campaignId}&t={targetId}{tokenQuery}";
                return Redirect(url);
            }

            return Redirect($"{_appBaseUrl}/");
        }

        [HttpPost("submit/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackSubmit(
            Guid campaignId,
            Guid targetId,
            [FromBody] CaptureMetadataDto? metadata = null,
            CancellationToken cancellationToken = default,
            [FromQuery(Name = TrackingContract.QueryToken)] string? trackingToken = null)
        {
            // Registra o evento "Submissão de Dados" (o alvo digitou credenciais na landing simulada).
            // NOTA LGPD: 'metadata' contém apenas propriedades de validação (flags/tamanho).
            // A senha real NÃO é recebida nem armazenada — só o evento em si é a métrica do TCC.
            var campaign = await RegistrarAcao(
                campaignId,
                targetId,
                SimulationActions.Submissao,
                trackingToken,
                cancellationToken);

            if (campaign == null)
            {
                return NotFound(new { message = "Campanha não encontrada ou link expirado." });
            }

            // Devolve a rota educacional interna para a landing redirecionar o alvo:
            // após "inserir dados", ele é levado ao treinamento de conscientização.
            // URL montada pelo TrackingContract (parâmetros canônicos c/t) — MESMO
            // formato do front (§1.3d). O `template` fica a cargo da landing (o back
            // não conhece qual treinamento; a rota educacional cai no default sem ele).
            return Ok(new TrackSubmitResponseDto
            {
                Status = "Inseriu Dados",
                RedirectUrl = TrackingContract.EducationalFeedbackUrl(
                    template: null,
                    campaignId: campaignId.ToString(),
                    targetId: targetId.ToString(),
                    trackingToken: trackingToken),
            });
        }

        [HttpPost("complete/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackComplete(
            Guid campaignId,
            Guid targetId,
            CancellationToken cancellationToken = default,
            [FromQuery(Name = TrackingContract.QueryToken)] string? trackingToken = null)
        {
            // Registra a CONCLUSÃO do módulo educacional (Just-in-Time Training) do alvo,
            // gerando trilha de auditoria de que o treinamento foi consumido. Idempotente
            // (RegistrarAcao ignora duplicatas) e anônimo — nenhum dado sensível é coletado.
            var campaign = await RegistrarAcao(
                campaignId,
                targetId,
                SimulationActions.TreinamentoConcluido,
                trackingToken,
                cancellationToken);

            if (campaign == null)
            {
                return NotFound(new { message = "Campanha não encontrada ou link expirado." });
            }

            return Ok(new { status = "Treinamento concluído" });
        }

        [HttpPost("educational-view/{campaignId}/{targetId}")]
        public async Task<IActionResult> TrackEducationalView(
            Guid campaignId,
            Guid targetId,
            CancellationToken cancellationToken = default,
            [FromQuery(Name = TrackingContract.QueryToken)] string? trackingToken = null)
        {
            // A visualização é um evento separado da conclusão: permite mensurar quem
            // chegou ao conteúdo educativo, mas o abandonou antes do botão final.
            // RegistrarAcao garante vínculo campanha/alvo, ordem mínima do funil e
            // idempotência por (CampaignId, TargetId, Acao).
            var campaign = await RegistrarAcao(
                campaignId,
                targetId,
                SimulationActions.PaginaEducacionalVisualizada,
                trackingToken,
                cancellationToken);

            if (campaign == null)
            {
                return NotFound(new { message = "Campanha não encontrada ou link expirado." });
            }

            return Ok(new { status = "Página educacional visualizada" });
        }

        [HttpGet("educational/{campaignId}")]
        public async Task<IActionResult> GetEducational(
            Guid campaignId,
            [FromQuery(Name = TrackingContract.QueryTarget)] Guid targetId,
            [FromQuery(Name = TrackingContract.QueryToken)] string? trackingToken)
        {
            if (!_trackingTokenService.Validate(trackingToken, campaignId, targetId))
                return NotFound(new { message = "Campanha não encontrada ou link expirado." });

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
