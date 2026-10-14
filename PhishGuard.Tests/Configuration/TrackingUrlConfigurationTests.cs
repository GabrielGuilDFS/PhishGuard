using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using PhishGuard.Backend.Configuration;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Tests.Configuration;

public sealed class TrackingUrlConfigurationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Auto")]
    [InlineData(" auto ")]
    public void Apply_Development_IgnoraDominioAntigoEUsaProxyLocal(string? mode)
    {
        var configuration = CreateConfiguration(mode);

        TrackingUrlConfiguration.Apply(configuration, new HostingEnvironment { EnvironmentName = "Development" });

        Assert.Equal("http://localhost:5173", configuration["AppSettings:PublicApiBaseUrl"]);
        Assert.Equal("http://localhost:5173", configuration["AppSettings:PublicAppBaseUrl"]);
        AssertMessageUrls(configuration, "http://localhost:5173");
    }

    [Theory]
    [InlineData("Production", "Auto")]
    [InlineData("Staging", "Auto")]
    [InlineData("Development", "Public")]
    public void Apply_Public_PreservaUrlsMesmoComHttpInterno(string environmentName, string mode)
    {
        var configuration = CreateConfiguration(mode);
        configuration["ASPNETCORE_URLS"] = "http://0.0.0.0:10000";

        TrackingUrlConfiguration.Apply(configuration, new HostingEnvironment { EnvironmentName = environmentName });

        Assert.Equal("https://old-tunnel.ngrok-free.app", configuration["AppSettings:PublicApiBaseUrl"]);
        Assert.Equal("https://app.example.test", configuration["AppSettings:PublicAppBaseUrl"]);
        AssertMessageUrls(configuration, "https://old-tunnel.ngrok-free.app");
    }

    [Theory]
    [InlineData("http://127.0.0.1:5000/", "https://localhost:5173/", "http://127.0.0.1:5000", "https://localhost:5173")]
    [InlineData("http://[::1]:5000", "http://localhost:5174", "http://[::1]:5000", "http://localhost:5174")]
    public void Apply_Local_RespeitaPortasEProtocoloConfigurados(
        string api, string app, string expectedApi, string expectedApp)
    {
        var configuration = CreateConfiguration("Local");
        configuration["AppSettings:LocalApiBaseUrl"] = api;
        configuration["AppSettings:LocalAppBaseUrl"] = app;

        TrackingUrlConfiguration.Apply(configuration, new HostingEnvironment { EnvironmentName = "Development" });

        Assert.Equal(expectedApi, configuration["AppSettings:PublicApiBaseUrl"]);
        Assert.Equal(expectedApp, configuration["AppSettings:PublicAppBaseUrl"]);
    }

    [Theory]
    [InlineData("LocalApiBaseUrl", "https://old-tunnel.ngrok-free.app")]
    [InlineData("LocalAppBaseUrl", "http://localhost.evil.example")]
    [InlineData("LocalApiBaseUrl", "/api")]
    [InlineData("LocalApiBaseUrl", "http://localhost:5173/api")]
    [InlineData("LocalAppBaseUrl", "http://localhost:5173?redirect=evil")]
    [InlineData("LocalAppBaseUrl", "http://localhost:5173#fragment")]
    [InlineData("LocalApiBaseUrl", "ftp://localhost")]
    [InlineData("LocalAppBaseUrl", "javascript:alert(1)")]
    [InlineData("LocalAppBaseUrl", "http://user:secret@localhost:5173")]
    public void Apply_Local_RejeitaUrlsInvalidasOuExternas(string key, string url)
    {
        var configuration = CreateConfiguration("Auto");
        configuration[$"AppSettings:{key}"] = url;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            TrackingUrlConfiguration.Apply(configuration, new HostingEnvironment { EnvironmentName = "Development" }));

        Assert.Contains(key, exception.Message);
        Assert.DoesNotContain(url, exception.Message);
    }

    [Theory]
    [InlineData("PublicApiBaseUrl", null)]
    [InlineData("PublicAppBaseUrl", "")]
    [InlineData("PublicApiBaseUrl", "http://localhost:5000")]
    [InlineData("PublicAppBaseUrl", "https://127.0.0.1:5173")]
    [InlineData("PublicApiBaseUrl", "https://user:secret@api.example.test")]
    public void Apply_Production_RejeitaConfiguracaoAusenteOuLocal(string key, string? url)
    {
        var configuration = CreateConfiguration("Auto");
        configuration[$"AppSettings:{key}"] = url;

        Assert.Throws<InvalidOperationException>(() =>
            TrackingUrlConfiguration.Apply(configuration, new HostingEnvironment { EnvironmentName = "Production" }));
    }

    [Fact]
    public void Apply_RejeitaModoDesconhecido()
    {
        var configuration = CreateConfiguration("Publci");

        Assert.Throws<InvalidOperationException>(() =>
            TrackingUrlConfiguration.Apply(configuration, new HostingEnvironment { EnvironmentName = "Development" }));
    }

    private static ConfigurationManager CreateConfiguration(string? mode)
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AppSettings:TrackingUrlMode"] = mode,
            ["AppSettings:PublicApiBaseUrl"] = "https://old-tunnel.ngrok-free.app/",
            ["AppSettings:PublicAppBaseUrl"] = "https://app.example.test/",
            ["NGROK_DOMAIN"] = "old-tunnel.ngrok-free.app"
        });
        return configuration;
    }

    private static void AssertMessageUrls(IConfiguration configuration, string expectedBase)
    {
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Template = new Template { CorpoHtml = "<a href=\"{{LINK}}\">Abrir</a>" }
        };
        var target = new Target { Id = Guid.NewGuid(), Email = "recipient@example.test", Nome = "Teste" };
        var composer = new EmailMessageComposer(new TrackingTokens(), configuration["AppSettings:PublicApiBaseUrl"]!);

        var message = composer.Compose(campaign, target, "sender@example.test");

        Assert.Contains($"{expectedBase}/api/tracking/open/{campaign.Id}/{target.Id}?k=signed-token", message.HtmlBody);
        Assert.Contains($"{expectedBase}/api/tracking/click/{campaign.Id}/{target.Id}?k=signed-token", message.HtmlBody);
    }

    private sealed class TrackingTokens : ITrackingTokenService
    {
        public string Create(Guid campaignId, Guid targetId) => "signed-token";
        public bool Validate(string? token, Guid campaignId, Guid targetId) => token == "signed-token";
    }
}
