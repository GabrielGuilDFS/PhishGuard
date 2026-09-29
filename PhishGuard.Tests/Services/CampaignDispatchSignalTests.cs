using PhishGuard.Backend.Services;

namespace PhishGuard.Tests.Services;

public class CampaignDispatchSignalTests
{
    [Fact]
    public async Task AvisosSaoAgrupadosEPreservadosAntesDaEspera()
    {
        using var signal = new CampaignDispatchSignal();
        for (var i = 0; i < 1000; i++) signal.Notify();
        Assert.True(await signal.WaitAsync(TimeSpan.Zero, CancellationToken.None));
        Assert.False(await signal.WaitAsync(TimeSpan.Zero, CancellationToken.None));
        var waiting = signal.WaitAsync(TimeSpan.FromMinutes(1), CancellationToken.None);
        signal.Notify();
        Assert.True(await waiting.WaitAsync(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task EncerramentoCancelaEspera()
    {
        using var signal = new CampaignDispatchSignal();
        using var cancellation = new CancellationTokenSource();
        var waiting = signal.WaitAsync(TimeSpan.FromMinutes(1), cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
    }
}
