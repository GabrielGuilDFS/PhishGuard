using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PhishGuard.Backend.Models;

/// <summary>A tenant-scoped participant and its immutable activation-time identity.</summary>
public sealed class CampaignTarget
{
    public Guid TenantId { get; set; }
    public Guid CampaignsId { get; set; }
    public Guid TargetsId { get; set; }

    [MaxLength(150)] public string? TargetNameSnapshot { get; set; }
    [MaxLength(150)] public string? TargetEmailSnapshot { get; set; }
    [MaxLength(80)] public string? DepartmentSnapshot { get; set; }
    public DateTime? SnapshotCapturedAtUtc { get; set; }
    public bool IsLegacySnapshot { get; set; }

    [JsonIgnore] public Campaign Campaign { get; set; } = null!;
    [JsonIgnore] public Target Target { get; set; } = null!;
}
