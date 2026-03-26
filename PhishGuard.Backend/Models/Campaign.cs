using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhishGuard.Backend.Models
{
    public class Campaign
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(150)]
        public string NomeCampanha { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Rascunho";

        [Required]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        [Required]
        public Guid EmailTemplateId { get; set; }
        [ForeignKey("EmailTemplateId")]
        public Template Template { get; set; }

        [Required]
        public Guid LandingPageId { get; set; }
        [ForeignKey("LandingPageId")]
        public PhishingPage PhishingPage { get; set; }

        [Required]
        public Guid EducationalPageId { get; set; }

[ForeignKey("EducationalPageId")]
        public EducationalPage EducationalPage { get; set; }

        public ICollection<Target> Targets { get; set; } = new List<Target>();
        
        public DateTime CriadoEm { get; set; }
    }
}