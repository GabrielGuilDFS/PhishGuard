using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
    [StringLength(150, ErrorMessage = "O nome da empresa deve ter no máximo 150 caracteres.")]
    public string NomeEmpresa { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CNPJ é obrigatório.")]
    [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ deve conter exatamente 14 dígitos.")]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "O CNPJ deve conter somente dígitos.")]
    public string Cnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Plano escolhido no onboarding ("bronze" | "prata" | "ouro"). Opcional:
    /// quando ausente, o Tenant é criado no plano Bronze por padrão.
    /// </summary>
    public string? Plano { get; set; }
}
