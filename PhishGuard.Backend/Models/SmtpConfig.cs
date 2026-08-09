using System;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Backend.Models
{
    public class SmtpConfig
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        // Provedor ativo (0 = Smtp, 1 = ProviderApi)
        public EmailProviderType ProviderType { get; set; } = EmailProviderType.Smtp;

        // Provedor por API HTTP selecionado quando ProviderType == ProviderApi
        public ApiProviderName ApiProvider { get; set; } = ApiProviderName.AwsSes;

        // E-mail e Nome do remetente (usado prioritariamente em ProviderApi ou override SMTP)
        public string SenderEmail { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        // API Key/secret cifrado com Data Protection quando ProviderType == ProviderApi.
        public string EncryptedApiKey { get; set; } = string.Empty;

        // Identificador público exigido pelo provedor. No AWS SES, armazena o Access Key ID;
        // nos demais provedores permanece vazio. Nunca contém segredo.
        public string ApiAccountIdentifier { get; set; } = string.Empty;

        // Região AWS selecionada a partir de allow-list estrita. Não é usada por outros
        // provedores e nunca é interpolada sem validação no host da API.
        public string ApiRegion { get; set; } = "us-east-1";

        // Campos SMTP tradicionais
        public string Host { get; set; } = string.Empty;

        public int Porta { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        public DateTime? UltimoTesteEmUtc { get; set; }

        public bool? UltimoTesteSucesso { get; set; }

        public string? UltimoErroCodigo { get; set; }

        public Tenant Tenant { get; set; } = null!;
    }

}
