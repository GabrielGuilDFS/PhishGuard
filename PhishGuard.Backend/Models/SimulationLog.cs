using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhishGuard.Backend.Models
{
    [Table("simulations_logs")]
    public class SimulationLog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [Column("campaign_id")]
        public Guid CampaignId { get; set; }

        [Required]
        [Column("target_id")]
        public Guid TargetId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("acao")]
        public string Acao { get; set; }

        [Required]
        [Column("data_hora")]
        public DateTime DataHora { get; set; }

        [Required]
        [MaxLength(45)]
        [Column("ip_origem")]
        public string IpOrigem { get; set; }
    }
}
