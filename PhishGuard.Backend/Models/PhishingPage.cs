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
        public string Nome { get; set; } 

        [Required]
        public string ConteudoHtml { get; set; } 

        public DateTime CriadoEm { get; set; }
    }
}