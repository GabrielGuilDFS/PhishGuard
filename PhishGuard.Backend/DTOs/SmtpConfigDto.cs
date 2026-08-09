using System;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Backend.DTOs
{
    public class SmtpConfigDto
    {
        public EmailProviderType ProviderType { get; set; } = EmailProviderType.Smtp;
        public ApiProviderName ApiProvider { get; set; } = ApiProviderName.AwsSes;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public bool ApiKeyConfigured { get; set; }
        public string ApiAccountIdentifier { get; set; } = string.Empty;
        public string ApiRegion { get; set; } = "us-east-1";

        public string Host { get; set; } = string.Empty;
        public int Porta { get; set; }
        public string Usuario { get; set; } = string.Empty;

        // Na RESPOSTA do GET a senha nunca é devolvida (vem vazia) para não expor a
        // credencial nem sobrescrevê-la sem querer no próximo save.
        public string Senha { get; set; } = string.Empty;

        public bool SenhaConfigurada { get; set; }
    }

    public sealed class SmtpStatusDto
    {
        public bool Configurado { get; init; }
        public EmailProviderType ProviderType { get; init; } = EmailProviderType.Smtp;
        public ApiProviderName ApiProvider { get; init; } = ApiProviderName.AwsSes;
        public bool SenhaConfigurada { get; init; }
        public bool ApiKeyConfigured { get; init; }
        public bool TransporteDisponivel { get; init; }
        public string? TransporteIndisponivelMotivo { get; init; }
        public DateTime? UltimoTesteEmUtc { get; init; }
        public bool? UltimoTesteSucesso { get; init; }
        public string? UltimoErroCodigo { get; init; }
    }

    public class TestarSmtpDto
    {
        public string? EmailDestino { get; set; }
        public Guid? TargetId { get; set; }
    }
}
