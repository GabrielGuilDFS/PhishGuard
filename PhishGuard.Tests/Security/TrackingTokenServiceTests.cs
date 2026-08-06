using PhishGuard.Backend.Security;

namespace PhishGuard.Tests.Security;

public class TrackingTokenServiceTests
{
    private const string Secret = "segredo-raiz-de-testes-com-pelo-menos-sessenta-e-quatro-bytes-123456789";

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;
        public override DateTimeOffset GetUtcNow() => UtcNow;
    }

    [Fact]
    public void TokenValido_FicaVinculadoAoParCampanhaAlvo()
    {
        var clock = new MutableTimeProvider(new DateTimeOffset(2026, 8, 5, 12, 0, 0, TimeSpan.Zero));
        var service = new TrackingTokenService(clock, Secret, TimeSpan.FromDays(30));
        var campaignId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var token = service.Create(campaignId, targetId);

        Assert.True(service.Validate(token, campaignId, targetId));
        Assert.False(service.Validate(token, Guid.NewGuid(), targetId));
        Assert.False(service.Validate(token, campaignId, Guid.NewGuid()));
    }

    [Fact]
    public void TokenAlteradoOuExpirado_ERejeitadoSemLancar()
    {
        var clock = new MutableTimeProvider(new DateTimeOffset(2026, 8, 5, 12, 0, 0, TimeSpan.Zero));
        var service = new TrackingTokenService(clock, Secret, TimeSpan.FromMinutes(5));
        var campaignId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var token = service.Create(campaignId, targetId);

        Assert.False(service.Validate(token + "x", campaignId, targetId));
        Assert.False(service.Validate(null, campaignId, targetId));

        clock.UtcNow = clock.UtcNow.AddMinutes(6);
        Assert.False(service.Validate(token, campaignId, targetId));
    }
}
