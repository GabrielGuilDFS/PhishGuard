using System;
using System.Collections.Generic;

namespace PhishGuard.Backend.Models
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string NomeEmpresa { get; set; }
        public string Cnpj { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }

        public ICollection<Administrador> Administradores { get; set; }
        public ICollection<Alvo> Alvos { get; set; }

        public SmtpConfig? SmtpConfig { get; set; }
    }
}