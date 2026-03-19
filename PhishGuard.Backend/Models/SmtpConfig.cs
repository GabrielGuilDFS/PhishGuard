using System;

namespace PhishGuard.Backend.Models
{
    public class SmtpConfig
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public string Host { get; set; }

        public int Porta { get; set; }

        public string Usuario { get; set; }

        public string Senha { get; set; }

        public Tenant Tenant { get; set; }
    }
}

