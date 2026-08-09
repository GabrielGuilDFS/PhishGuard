using System;
using System.Globalization;
using PhishGuard.Backend.Content;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Utilities;

namespace PhishGuard.Backend.Services.Delivery
{
    public interface IEmailMessageComposer
    {
        OutboundEmailMessage Compose(
            Campaign campaign,
            Target target,
            string senderEmail,
            string? senderNameOverride = null);
    }

    public class EmailMessageComposer : IEmailMessageComposer
    {
        private readonly ITrackingTokenService _trackingTokenService;
        private readonly string _baseTrackingUrl;

        public EmailMessageComposer(
            ITrackingTokenService trackingTokenService,
            string baseTrackingUrl)
        {
            _trackingTokenService = trackingTokenService;
            var normalized = baseTrackingUrl.TrimEnd('/');
            _baseTrackingUrl = normalized.EndsWith("/api/tracking", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : $"{normalized}/api/tracking";
        }

        public OutboundEmailMessage Compose(
            Campaign campaign,
            Target target,
            string senderEmail,
            string? senderNameOverride = null)
        {
            if (campaign.Template == null)
                throw new InvalidOperationException("Template de e-mail da campanha não encontrado.");

            var corpoBase = OfficialBaitCatalog.ResolveHtml(campaign.Template.CorpoHtml);

            var trackingToken = Uri.EscapeDataString(_trackingTokenService.Create(campaign.Id, target.Id));
            var linkClique = $"{_baseTrackingUrl}/click/{campaign.Id}/{target.Id}?k={trackingToken}";
            var linkPixel = $"{_baseTrackingUrl}/open/{campaign.Id}/{target.Id}?k={trackingToken}";

            var expiraEm = HorarioBrasilia.Converter(DateTime.UtcNow.AddHours(2));
            var dataExpiracao = expiraEm.ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"))
                + " às " + expiraEm.ToString("hh:mm tt", CultureInfo.GetCultureInfo("en-US"));

            var agora = HorarioBrasilia.Agora();
            var dataAcesso = agora.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                + " às " + agora.ToString("HH:mm", CultureInfo.InvariantCulture) + " (BRT)";

            var corpoPersonalizado = corpoBase
                .Replace("{{NOME}}", target.Nome)
                .Replace("{{LINK_PHISHING}}", linkClique)
                .Replace("{{LINK}}", linkClique)
                .Replace("{{DATA_EXPIRACAO}}", dataExpiracao)
                .Replace("{{DATA_ACESSO}}", dataAcesso);

            corpoPersonalizado += $"<img src=\"{linkPixel}\" width=\"1\" height=\"1\" style=\"display:block !important; width:1px !important; height:1px !important; max-width:1px !important; max-height:1px !important; border:0 !important; margin:0 !important; padding:0 !important; opacity:0.01 !important; overflow:hidden !important;\" alt=\"\" aria-hidden=\"true\" />";

            var message = new OutboundEmailMessage
            {
                FromName = !string.IsNullOrWhiteSpace(senderNameOverride) ? senderNameOverride : campaign.Template.RemetenteNome,
                FromEmail = senderEmail,
                ToName = target.Nome,
                ToEmail = target.Email,
                Subject = campaign.Template.Assunto,
                HtmlBody = corpoPersonalizado
            };

            foreach (var token in EmailLogoCatalog.Tokens)
            {
                if (corpoPersonalizado.Contains($"cid:{token}", StringComparison.OrdinalIgnoreCase)
                    && EmailLogoCatalog.TryGetLogo(token, out var logoFile, out var logoBytes))
                {
                    message.InlineAttachments.Add(new InlineAttachment
                    {
                        ContentId = token,
                        FileName = logoFile,
                        MediaType = "image/png",
                        Bytes = logoBytes
                    });
                }
            }

            return message;
        }
    }
}
