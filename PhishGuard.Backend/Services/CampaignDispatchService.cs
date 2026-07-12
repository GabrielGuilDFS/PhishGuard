using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using PhishGuard.Backend.Content;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services
{
    public interface ICampaignDispatchService
    {
        /// <summary>
        /// Realiza o disparo em lote dos e-mails de uma campanha e move seu status
        /// para "Em Andamento". O SMTP é resolvido pelo TenantId da PRÓPRIA campanha
        /// (não pelo ITenantProvider), para funcionar tanto no contexto de requisição
        /// quanto no worker de agendamento em segundo plano (sem HttpContext).
        /// </summary>
        Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default);
    }

    public class CampaignDispatchService : ICampaignDispatchService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CampaignDispatchService> _logger;

        private const string BaseTrackingUrl = "http://localhost:5000/api/tracking";

        public CampaignDispatchService(AppDbContext context, ILogger<CampaignDispatchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            // Garante que Template e Targets estejam carregados (o worker já os inclui;
            // este fallback cobre chamadas que passem a campanha sem navegações).
            if (campaign.Template == null)
                await _context.Entry(campaign).Reference(c => c.Template).LoadAsync(cancellationToken);
            if (campaign.Targets == null || !campaign.Targets.Any())
                await _context.Entry(campaign).Collection(c => c.Targets).LoadAsync(cancellationToken);

            if (campaign.Template == null)
                throw new InvalidOperationException("Template de e-mail da campanha não encontrado.");
            if (campaign.Targets == null || !campaign.Targets.Any())
                throw new InvalidOperationException("A campanha não possui alvos selecionados.");

            // SMTP é por-tenant. IgnoreQueryFilters + filtro explícito pelo TenantId da
            // campanha para funcionar sem contexto de tenant (worker em segundo plano).
            var smtpConfig = await _context.SmtpConfigs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.TenantId == campaign.TenantId, cancellationToken);

            if (smtpConfig == null)
                throw new InvalidOperationException("Configuração SMTP não encontrada para o tenant da campanha.");

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpConfig.Host, smtpConfig.Porta, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(smtpConfig.Usuario, smtpConfig.Senha, cancellationToken);

            // Persiste apenas o IDENTIFICADOR da isca em CorpoHtml; resolve para o HTML real.
            var corpoBase = OfficialBaitCatalog.ResolveHtml(campaign.Template.CorpoHtml);

            foreach (var target in campaign.Targets)
            {
                // Resiliência (PASSO 2): um alvo com e-mail inválido não pode derrubar o
                // disparo inteiro. Loga e segue para o próximo destinatário.
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(campaign.Template.RemetenteNome, campaign.Template.RemetenteEmail));
                    message.To.Add(new MailboxAddress(target.Nome, target.Email));
                    message.Subject = campaign.Template.Assunto;

                    var linkClique = $"{BaseTrackingUrl}/click/{campaign.Id}/{target.Id}";
                    var linkPixel = $"{BaseTrackingUrl}/open/{campaign.Id}/{target.Id}";

                    var corpoPersonalizado = corpoBase
                        .Replace("{{NOME}}", target.Nome)
                        .Replace("{{LINK_PHISHING}}", linkClique)
                        .Replace("{{LINK}}", linkClique);
                    corpoPersonalizado += $"<img src='{linkPixel}' width='1' height='1' style='display:none;' />";

                    message.Body = new BodyBuilder { HtmlBody = corpoPersonalizado }.ToMessageBody();

                    await client.SendAsync(message, cancellationToken);

                    // THROTTLING: pausa para não ser banido por spam pelo provedor SMTP.
                    await Task.Delay(1000, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Falha ao enviar e-mail para {Email} na campanha {CampaignId}.", target.Email, campaign.Id);
                }
            }

            await client.DisconnectAsync(true, cancellationToken);

            campaign.Status = CampaignStatus.EmAndamento;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
