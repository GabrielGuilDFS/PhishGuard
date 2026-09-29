namespace PhishGuard.Backend.Services;

// Apenas um aviso de trabalho: o banco continua sendo a fila durável.
// Avisos simultâneos são agrupados; nenhum ID/tenant fica em memória.
public sealed class CampaignDispatchSignal : IDisposable
{
    private readonly SemaphoreSlim _pending = new(0, 1);

    public void Notify()
    {
        try { _pending.Release(); }
        catch (SemaphoreFullException) { /* Já existe um ciclo pendente. */ }
    }

    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) =>
        _pending.WaitAsync(timeout, cancellationToken);

    public void Dispose() => _pending.Dispose();
}
