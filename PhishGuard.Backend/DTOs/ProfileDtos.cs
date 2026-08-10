using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.DTOs;

public sealed class ProfileResponseDto
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public sealed class UpdateProfileDto
{
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
    public string? Nome { get; init; }

    [StringLength(100, ErrorMessage = "A senha atual excede o tamanho permitido.")]
    public string? SenhaAtual { get; init; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter entre 6 e 100 caracteres.")]
    public string? NovaSenha { get; init; }
}

public sealed class ProfileUpdateResponseDto
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
