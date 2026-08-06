using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.Models
{
    public class Tenant
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
        public string NomeEmpresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CNPJ é obrigatório.")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ deve conter exatamente 14 dígitos.")]
        public string Cnpj { get; set; } = string.Empty;

        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }

        // Plano comercial contratado. Define as cotas de recursos (ex.: limite de
        // alvos). Novos tenants iniciam no Bronze até concluírem o checkout.
        public PlanoTenant Plano { get; set; } = PlanoTenant.Bronze;

        public ICollection<Administrador> Administradores { get; set; } = [];
        public ICollection<Target> Alvos { get; set; } = [];

        public SmtpConfig? SmtpConfig { get; set; }
    }
}
