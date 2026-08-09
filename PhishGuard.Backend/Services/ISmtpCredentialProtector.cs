namespace PhishGuard.Backend.Services
{
    /// <summary>
    /// Interface legada mantida para compatibilidade total com injecoes existentes.
    /// Delega para IEmailSecretProtector.
    /// </summary>
    public interface ISmtpCredentialProtector
    {
        string Protect(string? plaintext);
        string Unprotect(string? stored);
    }
}
