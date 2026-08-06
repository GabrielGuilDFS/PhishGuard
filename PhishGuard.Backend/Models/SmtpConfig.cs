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

        public DateTime? UltimoTesteEmUtc { get; set; }

        public bool? UltimoTesteSucesso { get; set; }

        public string? UltimoErroCodigo { get; set; }

        public Tenant Tenant { get; set; } = null!;
    }
}
