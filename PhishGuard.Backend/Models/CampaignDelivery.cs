using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhishGuard.Backend.Models
{
    public enum CampaignDeliveryStatus
    {
        Processing = 0,
        Sent = 1,
        Failed = 2
    }

    [Table("campaign_deliveries")]
    public sealed class CampaignDelivery
    {
        [Key]
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public Guid CampaignId { get; set; }

        public Guid TargetId { get; set; }

        [MaxLength(200)]
        public string IdempotencyKey { get; set; } = string.Empty;

        public CampaignDeliveryStatus Status { get; set; }

        public int AttemptCount { get; set; }

        // Token portátil de concorrência otimista. É renovado em cada claim/resultado.
        public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();

        public DateTime LastAttemptAtUtc { get; set; }

        public DateTime? LeaseExpiresAtUtc { get; set; }

        public DateTime? SentAtUtc { get; set; }

        [MaxLength(200)]
        public string? ProviderMessageId { get; set; }

        [MaxLength(100)]
        public string? LastErrorCode { get; set; }

        public Campaign Campaign { get; set; } = null!;

        public Target Target { get; set; } = null!;
    }
}
