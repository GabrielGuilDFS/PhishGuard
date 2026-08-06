namespace PhishGuard.Backend.DTOs
{
    public class SmtpConfigDto
    {
        public string Host { get; set; } = string.Empty;
        public int Porta { get; set; }
        public string Usuario { get; set; } = string.Empty;

        // Na RESPOSTA do GET a senha nunca é devolvida (vem vazia) para não expor a
        // credencial nem sobrescrevê-la sem querer no próximo save. Use
        // SenhaConfigurada como indicador de que já existe uma senha cifrada salva.
        public string Senha { get; set; } = string.Empty;

        public bool SenhaConfigurada { get; set; }
    }
}

