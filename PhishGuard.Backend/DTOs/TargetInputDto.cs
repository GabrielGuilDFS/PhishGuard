using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Backend.DTOs;

public sealed class TargetInputDto
{
    public Guid? Id { get; set; }

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Departamento { get; set; } = string.Empty;
}
