namespace PhishGuard.Backend.DTOs
{
    public record RegisterDto(
        string NomeEmpresa,
        string Cnpj,
        string Nome,
        string Email,
        string Password
    );
    public record LoginDto(string Email, string Password);
}