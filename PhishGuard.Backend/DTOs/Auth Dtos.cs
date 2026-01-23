namespace PhishGuard.Backend.DTOs
{
    public record RegisterDto(string Nome, string Email, string Senha);
    public record LoginDto(string Email, string Senha);
}