using Microsoft.Extensions.Caching.Memory;

namespace PhishGuard.Backend.Security;

/// <summary>
/// Cache opt-in para uma única instância. Escritas externas/múltiplas instâncias
/// exigem invalidação distribuída; nesse cenário mantenha-o desabilitado.
/// </summary>
public sealed class AuthSessionCache(TimeProvider timeProvider) : IDisposable
{
    private readonly MemoryCache _entries = new(new MemoryCacheOptions { SizeLimit = 10_000 });
    private readonly object _gate = new();
    private long _generation;
    private bool _disabled;

    public long Generation { get { lock (_gate) return _generation; } }

    public bool Contains(Guid administratorId, Guid tenantId, Guid sessionId)
    {
        lock (_gate)
            return !_disabled && _entries.TryGetValue((administratorId, tenantId, sessionId), out Entry? entry)
                && entry is not null && entry.Generation == _generation
                && entry.ExpiresAt > timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Store(Guid administratorId, Guid tenantId, Guid sessionId, DateTime sessionExpiry, long generation)
    {
        lock (_gate)
        {
            if (_disabled || generation != _generation) return;
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var expiry = sessionExpiry < now.AddSeconds(10) ? sessionExpiry : now.AddSeconds(10);
            if (expiry <= now) return;
            _entries.Set((administratorId, tenantId, sessionId), new Entry(generation, expiry),
                new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = expiry - now });
        }
    }

    public void Invalidate(bool explicitTransaction = false)
    {
        lock (_gate)
        {
            _generation++;
            // SaveChanges pode anteceder o COMMIT de uma transação explícita.
            // Sem observar esse commit, a opção segura é desligar o cache.
            _disabled |= explicitTransaction;
            _entries.Clear();
        }
    }

    public void Dispose() => _entries.Dispose();
    private sealed record Entry(long Generation, DateTime ExpiresAt);
}
