using System;
using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.Models
{
    public class EducationalPage
    {
        [Key]
        public Guid Id { get; set; }


        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string HtmlEducacional { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; }
    }
}
