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
        public string Nome { get; set; } 

        [Required]
        public string HtmlEducacional { get; set; } 

        public DateTime CriadoEm { get; set; }
    }
}