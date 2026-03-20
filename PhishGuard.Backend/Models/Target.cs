using System.Text.Json.Serialization;

namespace PhishGuard.Backend.Models
{
    public class Target
    {
        public Guid Id { get; set; } 

        [JsonIgnore]
        public Guid TenantId { get; set; }

        public string Nome { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
    }
}