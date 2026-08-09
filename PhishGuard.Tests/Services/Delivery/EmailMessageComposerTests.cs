using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Tests.Services.Delivery;

public sealed class EmailMessageComposerTests
{
    private sealed class TrackingTokenService : ITrackingTokenService
    {
        public string Create(Guid campaignId, Guid targetId) => "signed-token";
        public bool Validate(string? token, Guid campaignId, Guid targetId) => token == "signed-token";
    }

    [Fact]
    public void Compose_PersonalizaLinksPixelEAnexoCidUmaUnicaVez()
    {
        var target = new Target { Id = Guid.NewGuid(), Nome = "Álvaro", Email = "alvaro@example.com" };
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Template = new Template
            {
                Assunto = "Ação necessária",
                RemetenteNome = "Equipe",
                CorpoHtml = "<p>Olá {{NOME}} <a href=\"{{LINK}}\">abrir</a></p><img src=\"cid:logo-bhomax\">"
            }
        };
        var composer = new EmailMessageComposer(new TrackingTokenService(), "https://api.example.test");

        var message = composer.Compose(campaign, target, "sender@example.com");

        Assert.Equal("sender@example.com", message.FromEmail);
        Assert.Contains("Olá Álvaro", message.HtmlBody);
        Assert.Contains($"https://api.example.test/api/tracking/click/{campaign.Id}/{target.Id}?k=signed-token", message.HtmlBody);
        Assert.Contains($"https://api.example.test/api/tracking/open/{campaign.Id}/{target.Id}?k=signed-token", message.HtmlBody);
        Assert.Single(message.InlineAttachments);
        Assert.Equal("logo-bhomax", message.InlineAttachments[0].ContentId);
    }
}
