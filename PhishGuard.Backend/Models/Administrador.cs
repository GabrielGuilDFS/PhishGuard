using System;

namespace PhishGuard.Backend.Models
{
    public class Administrador
    {
        public Guid Id { get; set; } 

        public Guid TenantId { get; set; }

        public string Nome { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
    }
}