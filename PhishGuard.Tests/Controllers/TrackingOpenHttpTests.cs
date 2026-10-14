using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Services.Delivery;
using PhishGuard.Tests.TestDoubles;

namespace PhishGuard.Tests.Controllers;

// These tests run the real anonymous MVC endpoint over loopback HTTP. The database,
// signing key and server are owned by each test; no existing database or SMTP is used.
public sealed class TrackingOpenHttpTests
{
    [Fact]
    public async Task PixelHttp_WithRealSignedTokenAndSend_RecordsOpenWithoutClick()
    {
        await using var harness = await OpenHttpHarness.CreateAsync(withSend: true);
        var pixelUrl = harness.ComposePixelUrl();

        using var response = await harness.Client.GetAsync(pixelUrl);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/gif", response.Content.Headers.ContentType?.MediaType);
        var image = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal("GIF89a", System.Text.Encoding.ASCII.GetString(image, 0, 6));
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.NoCache);

        using var repeated = await harness.Client.GetAsync(pixelUrl);
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        await harness.AssertLogCountsAsync(opened: 1, clicked: 0);

        await using var dashboardContext = harness.CreateTenantContext();
        var overview = await new DashboardOverviewService(dashboardContext,
            new DashboardReportingTime(TimeProvider.System, TimeZoneInfo.Utc))
            .GetOverviewAsync(harness.TenantId, "30d", null);
        Assert.Equal(1, overview.Kpis.Sent.Total);
        Assert.Equal(1, overview.Kpis.OpenRate.ObservedTotal);
        Assert.Equal(0, overview.Kpis.OpenRate.InferredTotal);
        Assert.Equal(100d, overview.Kpis.OpenRate.Rate);
        Assert.Equal(0d, overview.Kpis.ClickRate.Rate);
    }

    [Fact]
    public async Task PixelHttp_WithInvalidToken_ReturnsGifWithoutRecordingOpen()
    {
        await using var harness = await OpenHttpHarness.CreateAsync(withSend: true);
        var pixelUrl = harness.ComposePixelUrl();
        // Alter the signed payload, rather than omit authentication altogether.
        var tokenStart = pixelUrl.IndexOf("?k=", StringComparison.Ordinal) + 3;
        pixelUrl = pixelUrl[..tokenStart] + "!" + pixelUrl[(tokenStart + 1)..];

        using var response = await harness.Client.GetAsync(pixelUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/gif", response.Content.Headers.ContentType?.MediaType);
        await harness.AssertLogCountsAsync(opened: 0, clicked: 0);
    }

    [Fact]
    public async Task PixelHttp_BeforeSendLog_IsDiscarded_ThenAcceptedAfterSendLog()
    {
        await using var harness = await OpenHttpHarness.CreateAsync(withSend: false);
        var pixelUrl = harness.ComposePixelUrl();

        using var beforeSend = await harness.Client.GetAsync(pixelUrl);
        Assert.Equal(HttpStatusCode.OK, beforeSend.StatusCode);
        await harness.AssertLogCountsAsync(opened: 0, clicked: 0);

        await using (var context = harness.CreateTenantContext())
        {
            context.SimulationsLogs.Add(new SimulationLog
            {
                Id = Guid.NewGuid(),
                CampaignId = harness.Campaign.Id,
                TargetId = harness.Target.Id,
                Acao = SimulationActions.Envio,
                DataHora = DateTime.UtcNow,
                IpOrigem = "SISTEMA"
            });
            await context.SaveChangesAsync();
        }

        using var afterSend = await harness.Client.GetAsync(pixelUrl);
        Assert.Equal(HttpStatusCode.OK, afterSend.StatusCode);
        await harness.AssertLogCountsAsync(opened: 1, clicked: 0);
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid GetTenantId() => tenantId;
        public Guid GetCurrentTenantId() => tenantId;
    }

    private sealed class OpenHttpHarness : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WebApplication _app;
        private readonly ITrackingTokenService _trackingTokens;

        private OpenHttpHarness(SqliteConnection connection, WebApplication app,
            Guid tenantId, Campaign campaign)
        {
            _connection = connection;
            _app = app;
            TenantId = tenantId;
            Campaign = campaign;
            _trackingTokens = app.Services.GetRequiredService<ITrackingTokenService>();
            Client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(app.Urls.Single()),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        internal HttpClient Client { get; }
        internal Guid TenantId { get; }
        internal Campaign Campaign { get; }
        internal Target Target => Campaign.Targets.Single();

        internal static async Task<OpenHttpHarness> CreateAsync(bool withSend)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Development",
                ApplicationName = typeof(TrackingController).Assembly.FullName
            });
            // Suppress access logs: tracking capabilities must not appear in test output.
            builder.Logging.ClearProviders();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:Token"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ["AppSettings:PublicAppBaseUrl"] = "http://127.0.0.1"
            });
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton<ITrackingTokenService, TrackingTokenService>();
            builder.Services.AddScoped<TrackingCampaignScope>();
            builder.Services.AddScoped<ITenantProvider>(_ => new FixedTenantProvider(Guid.Empty));
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
            builder.Services.AddControllers().AddApplicationPart(typeof(TrackingController).Assembly);
            var app = builder.Build();
            app.MapControllers();
            var tenantId = Guid.NewGuid();
            try
            {
                await using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(connection).Options, new FixedTenantProvider(tenantId));
                await context.Database.EnsureCreatedAsync();
                var campaign = await MvpGraph.SeedGraphAsync(context, tenantId,
                    CampaignStatus.EmAndamento, history: withSend);
                campaign.Template!.CorpoHtml = "<html><body><p>Olá {{NOME}}</p>"
                    + "<a href=\"{{LINK}}\">Acessar</a></body></html>";
                await context.SaveChangesAsync();
                await app.StartAsync();
                return new OpenHttpHarness(connection, app, tenantId, campaign);
            }
            catch
            {
                await app.DisposeAsync();
                await connection.DisposeAsync();
                throw;
            }
        }

        internal string ComposePixelUrl()
        {
            var composer = new EmailMessageComposer(_trackingTokens, Client.BaseAddress!.ToString());
            var email = composer.Compose(Campaign, Target, "sender@example.test");
            var pixel = Regex.Match(email.HtmlBody,
                "<img\\b[^>]*\\bsrc=\"(?<url>[^\"]+/open/[^\"]+)\"", RegexOptions.IgnoreCase);
            Assert.True(pixel.Success, "The composed message must contain its tracking pixel.");
            return pixel.Groups["url"].Value;
        }

        internal AppDbContext CreateTenantContext() => new(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options,
            new FixedTenantProvider(TenantId));

        internal async Task AssertLogCountsAsync(int opened, int clicked)
        {
            await using var context = CreateTenantContext();
            var logs = context.SimulationsLogs.Where(log => log.CampaignId == Campaign.Id
                && log.TargetId == Target.Id);
            Assert.Equal(opened, await logs.CountAsync(log => log.Acao == SimulationActions.Abertura));
            Assert.Equal(clicked, await logs.CountAsync(log => log.Acao == SimulationActions.Clique));
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.StopAsync();
            await _app.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
