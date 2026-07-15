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

        /// <summary>
        /// Tentativas consecutivas de senha incorreta. Zerado a cada login bem-sucedido.
        /// Ao atingir o limite, dispara o lockout temporário (<see cref="BloqueioFim"/>).
        /// </summary>
        public int AcessoFalhasContador { get; set; }

        /// <summary>
        /// Instante (UTC) até o qual a conta permanece bloqueada por brute force.
        /// <c>null</c> = conta liberada. Comparar sempre contra <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public DateTime? BloqueioFim { get; set; }
    }
}