using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace PhishGuard.Backend.Security;

public interface ITrackingTokenService
{
    string Create(Guid campaignId, Guid targetId);
    bool Validate(string? token, Guid campaignId, Guid targetId);
}

/// <summary>
/// Capability token assinado para os endpoints anônimos do funil. A chave é
/// derivada com separação de domínio para não reutilizar diretamente a chave JWT.
/// </summary>
public sealed class TrackingTokenService : ITrackingTokenService
{
    private const string Version = "v1";
    private const string KeyDomain = "PhishGuard.TrackingToken.v1\0";
    private readonly TimeProvider _timeProvider;
    private readonly byte[] _signingKey;
    private readonly TimeSpan _lifetime;

    public TrackingTokenService(TimeProvider timeProvider, IConfiguration configuration)
        : this(
            timeProvider,
            configuration["AppSettings:Token"]
                ?? throw new InvalidOperationException("Segredo do token de rastreamento ausente."),
            TimeSpan.FromDays(configuration.GetValue<int?>("AppSettings:TrackingTokenLifetimeDays") ?? 90))
    {
    }

    internal TrackingTokenService(TimeProvider timeProvider, string rootSecret, TimeSpan lifetime)
    {
        if (Encoding.UTF8.GetByteCount(rootSecret) < 64)
            throw new InvalidOperationException("O segredo raiz deve ter pelo menos 64 bytes.");
        if (lifetime <= TimeSpan.Zero || lifetime > TimeSpan.FromDays(365))
            throw new InvalidOperationException("A validade do token de rastreamento deve estar entre 1 e 365 dias.");

        _timeProvider = timeProvider;
        _lifetime = lifetime;
        _signingKey = SHA256.HashData(Encoding.UTF8.GetBytes(KeyDomain + rootSecret));
    }

    public string Create(Guid campaignId, Guid targetId)
    {
        var expiresAt = _timeProvider.GetUtcNow().Add(_lifetime).ToUnixTimeSeconds();
        var payload = Encoding.UTF8.GetBytes($"{Version}|{campaignId:N}|{targetId:N}|{expiresAt}");
        var signature = HMACSHA256.HashData(_signingKey, payload);
        return $"{WebEncoders.Base64UrlEncode(payload)}.{WebEncoders.Base64UrlEncode(signature)}";
    }

    public bool Validate(string? token, Guid campaignId, Guid targetId)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 512)
            return false;

        var parts = token.Split('.', 2);
        if (parts.Length != 2)
            return false;

        try
        {
            var payload = WebEncoders.Base64UrlDecode(parts[0]);
            var providedSignature = WebEncoders.Base64UrlDecode(parts[1]);
            var expectedSignature = HMACSHA256.HashData(_signingKey, payload);
            if (!CryptographicOperations.FixedTimeEquals(providedSignature, expectedSignature))
                return false;

            var fields = Encoding.UTF8.GetString(payload).Split('|');
            return fields.Length == 4
                && fields[0] == Version
                && Guid.TryParseExact(fields[1], "N", out var tokenCampaignId)
                && tokenCampaignId == campaignId
                && Guid.TryParseExact(fields[2], "N", out var tokenTargetId)
                && tokenTargetId == targetId
                && long.TryParse(fields[3], out var expiresAt)
                && expiresAt >= _timeProvider.GetUtcNow().ToUnixTimeSeconds();
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
