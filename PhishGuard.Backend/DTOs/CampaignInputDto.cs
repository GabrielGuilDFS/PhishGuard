using System;
using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.DTOs
{
    public class CampaignInputDto
    {
        [Required]
        [MaxLength(150)]
        public string NomeCampanha { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        [Required]
        public Guid EmailTemplateId { get; set; }

        [Required]
        public Guid LandingPageId { get; set; }

        [Required]
        public Guid EducationalPageId { get; set; }
        
        public List<Guid> TargetIds { get; set; } = new List<Guid>();
    }
}
