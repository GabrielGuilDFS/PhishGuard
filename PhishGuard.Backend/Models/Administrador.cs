using System;
using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.Models
{
    public class Administrador
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        public string Email { get; set; }

        public string PasswordHash { get; set; }
    }
}