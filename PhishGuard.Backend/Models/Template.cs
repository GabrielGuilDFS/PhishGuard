using System;
using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.Models
{
    public class Template
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Assunto { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RemetenteNome { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RemetenteEmail { get; set; } = string.Empty;

        [Required]
        public string CorpoHtml { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; }
    }
}
