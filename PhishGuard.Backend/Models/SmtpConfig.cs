using System;

namespace PhishGuard.Backend.Models
{
    public class SmtpConfig
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public string Host { get; set; } = string.Empty;

        public int Porta { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        public Tenant Tenant { get; set; } = null!;
    }
}

