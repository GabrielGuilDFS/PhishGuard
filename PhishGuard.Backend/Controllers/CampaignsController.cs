using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishGuard.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TenantMutationFilter))]
    public class CampaignsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IConfiguration _configuration;
        private readonly ISmtpCredentialProtector? _smtpCredentialProtector;
        private readonly ILogger<CampaignsController>? _logger;
        private readonly CampaignDispatchSignal? _dispatchSignal;
        private readonly IHtmlSanitizationService? _htmlSanitizer;

        // O disparo de e-mails NÃO é responsabilidade deste controller: ele apenas
        // transiciona o estado da campanha. O envio assíncrono fica a cargo do
        // CampaignSchedulerWorker (baixo acoplamento + resiliência).
        public CampaignsController(
            AppDbContext context,
            ITenantProvider tenantProvider,
            IConfiguration? configuration = null,
            ISmtpCredentialProtector? smtpCredentialProtector = null,
            ILogger<CampaignsController>? logger = null,
            CampaignDispatchSignal? dispatchSignal = null,
            IHtmlSanitizationService? htmlSanitizer = null)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _configuration = configuration ?? new ConfigurationBuilder().Build();
            _smtpCredentialProtector = smtpCredentialProtector;
            _logger = logger;
            _dispatchSignal = dispatchSignal;
            _htmlSanitizer = htmlSanitizer;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCampaigns()
        {
            var campaigns = await _context.Campaigns
                .Include(c => c.Template)
                .Include(c => c.PhishingPage)
                .Include(c => c.EducationalPage)
                .OrderByDescending(c => c.CriadoEm)
                .Select(c => new
                {
                    c.Id,
                    nomeCampanha = c.NomeCampanha,
                    status = c.Status,
                    dataInicio = c.DataInicio,
                    // Data de Encerramento da Coleta: incluída na LISTAGEM (antes só o GET
                    // por id a trazia), por isso a coluna aparecia vazia mesmo em campanhas
                    // que já tinham o prazo definido. Nula = coleta sem prazo (indefinida).
                    dataFim = c.DataFim,
                    dispatchErrorCode = c.DispatchErrorCode,
                    dispatchErrorMessage = c.DispatchErrorMessage,
                    dispatchFailedAtUtc = c.DispatchFailedAtUtc,
                    snapshotCapturedAtUtc = c.SnapshotCapturedAtUtc,
                    isLegacySnapshot = c.IsLegacySnapshot,
                    hasHistory = _context.SimulationsLogs.Any(l => l.CampaignId == c.Id && l.TenantId == c.TenantId)
                        || _context.CampaignDeliveries.Any(d => d.CampaignId == c.Id && d.TenantId == c.TenantId),
                    templateNome = c.TemplateNameSnapshot ?? c.Template.Nome,
                    landingPageNome = c.LandingNameSnapshot ?? c.PhishingPage.Nome,
                    educationalPageNome = c.EducationalNameSnapshot ?? c.EducationalPage.Nome
                })
                .ToListAsync();

            return Ok(campaigns);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCampaign(Guid id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.Template)
                .Include(c => c.PhishingPage)
                .Include(c => c.EducationalPage)
                .Include(c => c.Targets) 
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    nomeCampanha = c.NomeCampanha,
                    status = c.Status,
                    dataInicio = c.DataInicio,
                    dataFim = c.DataFim,
                    emailTemplateId = c.EmailTemplateId,
                    landingPageId = c.LandingPageId,
                    educationalPageId = c.EducationalPageId,
                    snapshotCapturedAtUtc = c.SnapshotCapturedAtUtc,
                    isLegacySnapshot = c.IsLegacySnapshot,
                    hasHistory = _context.SimulationsLogs.Any(l => l.CampaignId == c.Id && l.TenantId == c.TenantId)
                        || _context.CampaignDeliveries.Any(d => d.CampaignId == c.Id && d.TenantId == c.TenantId),
                    templateNome = c.TemplateNameSnapshot ?? c.Template.Nome,
                    landingPageNome = c.LandingNameSnapshot ?? c.PhishingPage.Nome,
                    educationalPageNome = c.EducationalNameSnapshot ?? c.EducationalPage.Nome,
                    targetIds = c.Targets.Select(t => t.Id).ToList(),
                    targetsSnapshot = c.CampaignTargets.Select(p => new
                    {
                        targetId = p.TargetsId, name = p.TargetNameSnapshot,
                        email = p.TargetEmailSnapshot, department = p.DepartmentSnapshot,
                        snapshotCapturedAtUtc = p.SnapshotCapturedAtUtc, isLegacySnapshot = p.IsLegacySnapshot
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (campaign == null) return NotFound();

            return Ok(campaign);
        }

        [HttpPost]
        public async Task<ActionResult> PostCampaign([FromBody] CampaignInputDto input, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            if (!await ResourcesExistAndBelongToTenant(input, tenantId, cancellationToken))
            {
                return BadRequest("Um ou mais recursos referenciados não existem ou não pertencem ao tenant atual.");
            }

            var targetsSelecionados = await _context.Targets
                .Where(t => input.TargetIds.Contains(t.Id) && t.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            var novaCampanha = new Campaign
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                NomeCampanha = input.NomeCampanha,
                DataInicio = ParaUtc(input.DataInicio),
                DataFim = ParaUtc(input.DataFim),
                EmailTemplateId = input.EmailTemplateId,
                LandingPageId = input.LandingPageId,
                EducationalPageId = input.EducationalPageId,
                Status = CampaignStatus.Rascunho,
                CriadoEm = DateTime.UtcNow,
                Targets = targetsSelecionados 
            };

            _context.Campaigns.Add(novaCampanha);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetCampaign), new { id = novaCampanha.Id }, new 
            {
                novaCampanha.Id,
                novaCampanha.NomeCampanha,
                novaCampanha.Status
            });
        }

        // Ativa (homologa) uma campanha em Rascunho. NÃO dispara e-mails na thread HTTP:
        // apenas transiciona o estado e delega o envio (assíncrono) ao worker.
        //  - DataInicio no FUTURO  → "Agendada" (o worker dispara no horário).
        //  - DataInicio já atingida → CLAIM "Processando" (o worker envia no próximo ciclo).
        [HttpPost("{id}/ativar")]
        public async Task<IActionResult> AtivarCampanha(Guid id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant.
            var campaign = await _context.Campaigns
                .Include(c => c.Targets)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);

            if (campaign == null) return NotFound("Campanha não encontrada.");
            if (campaign.Status != CampaignStatus.Rascunho)
                return BadRequest("Apenas campanhas em Rascunho podem ser ativadas.");
            if (campaign.SnapshotCapturedAtUtc.HasValue || await HasHistoryAsync(campaign.Id, tenantId, cancellationToken))
                return Conflict(new { message = "Esta campanha já possui dados preservados ou histórico. Crie uma nova campanha para um novo disparo." });
            if (!campaign.Targets.Any()) return BadRequest("A campanha não possui alvos selecionados.");

            var smtpConfig = await _context.SmtpConfigs
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
            if (!SmtpOperationalPolicy.IsConfigured(smtpConfig))
            {
                return Conflict(new
                {
                    code = SmtpOperationalPolicy.NotConfiguredCode,
                    message = "Configure e salve o servidor SMTP antes de ativar a campanha.",
                    settingsPath = "/admin/settings?tab=smtp"
                });
            }

            var credentialError = _smtpCredentialProtector is null
                ? null
                : SmtpOperationalPolicy.ValidateCredential(smtpConfig, _smtpCredentialProtector);
            if (credentialError is not null)
            {
                return Conflict(new
                {
                    code = credentialError,
                    message = "A senha SMTP salva não pode ser utilizada. Informe a senha novamente e salve a configuração.",
                    settingsPath = "/admin/settings?tab=smtp"
                });
            }

            // A flag de infraestrutura desabilita apenas conexões SMTP (portas de saída
            // bloqueadas no Render). Provedores por API HTTPS, como Mailtrap Sandbox,
            // permanecem disponíveis e não podem ser barrados por essa política.
            if (!SmtpOperationalPolicy.IsTransportAvailable(smtpConfig, _configuration))
            {
                _logger?.LogWarning(
                    "Ativação da campanha {CampaignId} bloqueada: transporte SMTP indisponível para o tenant {TenantId}.",
                    campaign.Id,
                    tenantId);
                return Conflict(new
                {
                    code = SmtpOperationalPolicy.TransportUnavailableCode,
                    message = SmtpOperationalPolicy.GetTransportDisabledReason(_configuration),
                    settingsPath = "/admin/settings?tab=smtp"
                });
            }

            if (_htmlSanitizer is null)
                throw new InvalidOperationException("O serviço de sanitização é obrigatório para ativar campanhas.");
            await CampaignSnapshotCapture.CaptureAsync(_context, campaign, smtpConfig!, _htmlSanitizer, cancellationToken);

            campaign.DispatchErrorCode = null;
            campaign.DispatchErrorMessage = null;
            campaign.DispatchFailedAtUtc = null;

            // Início no FUTURO: apenas agenda. O CampaignSchedulerWorker faz o disparo
            // automaticamente quando a DataInicio for atingida.
            if (campaign.DataInicio > DateTime.UtcNow)
            {
                campaign.Status = CampaignStatus.Agendada;
                await _context.SaveChangesAsync(cancellationToken);
                _logger?.LogInformation(
                    "Campanha {CampaignId} agendada pelo tenant {TenantId} usando {ProviderType}/{ApiProvider}.",
                    campaign.Id,
                    tenantId,
                    smtpConfig!.ProviderType,
                    smtpConfig.ApiProvider);
                return Ok(new { message = "Campanha agendada. O disparo ocorrerá no horário de início.", status = campaign.Status });
            }

            // Início já alcançado: faz o CLAIM ("Processando") e RETORNA na hora. O envio
            // em lote roda de forma assíncrona e idempotente no worker — a thread HTTP não
            // fica bloqueada por minutos nem segura a conexão do banco durante o disparo.
            campaign.Status = CampaignStatus.Processando;
            await _context.SaveChangesAsync(cancellationToken);
            _dispatchSignal?.Notify();

            _logger?.LogInformation(
                "Campanha {CampaignId} enfileirada pelo tenant {TenantId} usando {ProviderType}/{ApiProvider}.",
                campaign.Id,
                tenantId,
                smtpConfig!.ProviderType,
                smtpConfig.ApiProvider);

            return Accepted(new
            {
                message = "Disparo enfileirado. Os e-mails serão enviados em instantes.",
                status = campaign.Status
            });
        }

        [HttpPost("{id}/retry-dispatch")]
        public async Task<IActionResult> RetryDispatch(Guid id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var campaign = await _context.Campaigns
                .Include(c => c.Targets)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);

            if (campaign == null) return NotFound("Campanha não encontrada.");
            if (campaign.Status != CampaignStatus.FalhaNoDisparo)
                return BadRequest("Apenas campanhas com falha de disparo podem ser reenfileiradas.");

            var smtpConfig = await _context.SmtpConfigs.FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
            if (!SmtpOperationalPolicy.IsConfigured(smtpConfig))
                return Conflict(new { code = SmtpOperationalPolicy.NotConfiguredCode, message = "Configure e salve a entrega de e-mail antes de tentar novamente.", settingsPath = "/admin/settings?tab=smtp" });
            var credentialError = _smtpCredentialProtector is null
                ? null
                : SmtpOperationalPolicy.ValidateCredential(smtpConfig, _smtpCredentialProtector);
            if (credentialError is not null)
                return Conflict(new { code = credentialError, message = "A senha SMTP salva não pode ser utilizada. Informe a senha novamente e salve a configuração.", settingsPath = "/admin/settings?tab=smtp" });
            if (!SmtpOperationalPolicy.IsTransportAvailable(smtpConfig, _configuration))
                return Conflict(new { code = SmtpOperationalPolicy.TransportUnavailableCode, message = SmtpOperationalPolicy.GetTransportDisabledReason(_configuration), settingsPath = "/admin/settings?tab=smtp" });

            campaign.Status = campaign.DataInicio > DateTime.UtcNow
                ? CampaignStatus.Agendada
                : CampaignStatus.Processando;
            campaign.DispatchErrorCode = null;
            campaign.DispatchErrorMessage = null;
            campaign.DispatchFailedAtUtc = null;
            await _context.SaveChangesAsync(cancellationToken);
            if (campaign.Status == CampaignStatus.Processando) _dispatchSignal?.Notify();

            _logger?.LogInformation(
                "Campanha {CampaignId} reenfileirada pelo tenant {TenantId} usando {ProviderType}/{ApiProvider}.",
                campaign.Id,
                tenantId,
                smtpConfig!.ProviderType,
                smtpConfig.ApiProvider);

            return Accepted(new { message = "Campanha reenfileirada para disparo.", status = campaign.Status });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCampaign(Guid id, [FromBody] CampaignInputDto input, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            
            var campaignExistente = await _context.Campaigns
                .Include(c => c.Targets)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);
                
            if (campaignExistente == null) return NotFound();

            if (campaignExistente.Status != CampaignStatus.Rascunho || campaignExistente.SnapshotCapturedAtUtc.HasValue
                || await HasHistoryAsync(id, tenantId, cancellationToken))
                return Conflict(new { message = "Apenas campanhas em Rascunho podem ser editadas. Crie uma nova campanha para alterar o cenário ou os destinatários." });

            if (!await ResourcesExistAndBelongToTenant(input, tenantId, cancellationToken))
            {
                return BadRequest("Um ou mais recursos referenciados não existem ou não pertencem ao tenant atual.");
            }

            var novosTargets = await _context.Targets
                .Where(t => input.TargetIds.Contains(t.Id) && t.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            campaignExistente.NomeCampanha = input.NomeCampanha;
            campaignExistente.DataInicio = ParaUtc(input.DataInicio);
            campaignExistente.DataFim = ParaUtc(input.DataFim);
            campaignExistente.EmailTemplateId = input.EmailTemplateId;
            campaignExistente.LandingPageId = input.LandingPageId;
            campaignExistente.EducationalPageId = input.EducationalPageId;

            campaignExistente.Targets.Clear(); 
            foreach (var target in novosTargets)
            {
                campaignExistente.Targets.Add(target); 
            }

            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCampaign(Guid id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Anti-IDOR: escopo explícito ao tenant. FindAsync busca só por PK e
            // contornaria o Global Query Filter, permitindo apagar recurso de outro tenant.
            var campaign = await _context.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);
            if (campaign == null) return NotFound();

            if (campaign.Status != CampaignStatus.Rascunho
                || campaign.SnapshotCapturedAtUtc.HasValue || await HasHistoryAsync(id, tenantId, cancellationToken))
                return Conflict(new { message = "Somente campanhas em Rascunho e sem histórico podem ser excluídas." });

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        private async Task<bool> HasHistoryAsync(Guid campaignId, Guid tenantId, CancellationToken cancellationToken)
            => await _context.SimulationsLogs.AnyAsync(l => l.CampaignId == campaignId && l.TenantId == tenantId, cancellationToken)
                || await _context.CampaignDeliveries.AnyAsync(d => d.CampaignId == campaignId && d.TenantId == tenantId, cancellationToken);

        // TIMEZONE (blindagem de agendamento): as colunas de data são 'timestamp with time
        // zone' (timestamptz) e o worker compara o agendamento contra DateTime.UtcNow. Por
        // isso normalizamos toda data recebida para UTC AQUI, na fronteira da API:
        //  - Utc         → mantém (o front envia ISO 8601 com 'Z');
        //  - Local       → converte para UTC (preserva o instante);
        //  - Unspecified → ASSUME UTC (contrato explícito da API). Sem isto, o Npgsql
        //                  REJEITA um DateTime 'Unspecified' ao gravar em timestamptz, e a
        //                  criação/edição da campanha falharia (ex.: chamada via Swagger sem
        //                  fuso). Garante também que a comparação do worker seja UTC × UTC.
        private static DateTime ParaUtc(DateTime valor) => valor.Kind switch
        {
            DateTimeKind.Utc => valor,
            DateTimeKind.Local => valor.ToUniversalTime(),
            _ => DateTime.SpecifyKind(valor, DateTimeKind.Utc)
        };

        private static DateTime? ParaUtc(DateTime? valor) => valor.HasValue ? ParaUtc(valor.Value) : null;

        private async Task<bool> ResourcesExistAndBelongToTenant(CampaignInputDto input, Guid tenantId, CancellationToken cancellationToken = default)
        {
            var templateExists = await _context.Templates.AnyAsync(t => t.Id == input.EmailTemplateId && t.TenantId == tenantId, cancellationToken);
            var landingPageExists = await _context.PhishingPages.AnyAsync(p => p.Id == input.LandingPageId && p.TenantId == tenantId, cancellationToken);
            var educationalPageExists = await _context.EducationalPages.AnyAsync(e => e.Id == input.EducationalPageId && e.TenantId == tenantId, cancellationToken);

            return templateExists && landingPageExists && educationalPageExists;
        }
    }
}
