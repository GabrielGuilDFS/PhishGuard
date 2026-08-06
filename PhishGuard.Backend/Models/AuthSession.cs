namespace PhishGuard.Backend.Models;

public sealed class AuthSession
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AdministratorId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? LastRotatedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
}
