namespace PhishGuard.Backend.Models
{
    public class Alvo
    {
        public Guid Id { get; set; } 
        
        public Guid TenantId { get; set; }

        public string Nome { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
    }
}