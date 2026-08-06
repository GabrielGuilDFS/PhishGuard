using System;
using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.Models
{
    public class PhishingPage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string HtmlCaptura { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; }
    }
}
