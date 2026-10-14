using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // 🎯 Importante para as validações funcionarem!
using System.Text.Json.Serialization;

namespace PhishGuard.Backend.Models
{
    public class Target
    {
        public Guid Id { get; set; } 

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public string EmailNormalizado { get; set; } = string.Empty;

        public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        public string Departamento { get; set; } = string.Empty;

        [JsonIgnore]
        public Guid TenantId { get; set; }

        [JsonIgnore]    
        public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

        [JsonIgnore]
        public ICollection<CampaignTarget> CampaignTargets { get; set; } = new List<CampaignTarget>();
    }
}
