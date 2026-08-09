using System;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using PhishGuard.Backend.Services.Delivery;

namespace PhishGuard.Backend.Services
{
    public enum EmailSecretType
    {
        SmtpPassword = 0,
        ApiKey = 1
    }

    public interface IEmailSecretProtector : ISmtpCredentialProtector
    {
        string ProtectSecret(
            Guid tenantId,
            EmailProviderType providerType,
            EmailSecretType secretType,
            string? plaintext);

        string UnprotectSecret(
            Guid tenantId,
            EmailProviderType providerType,
            EmailSecretType secretType,
            string? stored);
    }

    public class EmailSecretProtector : IEmailSecretProtector
    {
        private const string ContextualPurpose = "PhishGuard.EmailDelivery.Secret.v2";
        private const string LegacySmtpPurpose = "PhishGuard.SmtpConfig.Senha.v1";
        private const string LegacyApiKeyPurpose = "PhishGuard.EmailDelivery.ApiKey.v1";

        private readonly IDataProtector _contextualRoot;
        private readonly IDataProtector _legacySmtpProtector;
        private readonly IDataProtector _legacyApiKeyProtector;

        public EmailSecretProtector(IDataProtectionProvider provider)
        {
            _contextualRoot = provider.CreateProtector(ContextualPurpose);
            _legacySmtpProtector = provider.CreateProtector(LegacySmtpPurpose);
            _legacyApiKeyProtector = provider.CreateProtector(LegacyApiKeyPurpose);
        }

        // Contrato legado mantido exclusivamente para configurações SMTP antigas e testes.
        public string Protect(string? plaintext)
            => string.IsNullOrEmpty(plaintext)
                ? plaintext ?? string.Empty
                : _legacySmtpProtector.Protect(plaintext);

        public string Unprotect(string? stored)
            => UnprotectLegacy(stored, _legacySmtpProtector);

        public string ProtectSecret(
            Guid tenantId,
            EmailProviderType providerType,
            EmailSecretType secretType,
            string? plaintext)
        {
            if (string.IsNullOrEmpty(plaintext)) return plaintext ?? string.Empty;
            if (tenantId == Guid.Empty)
                throw new ArgumentException("O tenant é obrigatório para proteger credenciais de e-mail.", nameof(tenantId));

            return ContextualProtector(tenantId, providerType, secretType).Protect(plaintext);
        }

        public string UnprotectSecret(
            Guid tenantId,
            EmailProviderType providerType,
            EmailSecretType secretType,
            string? stored)
        {
            if (string.IsNullOrEmpty(stored)) return stored ?? string.Empty;
            if (tenantId == Guid.Empty)
                throw new SmtpOperationalException(
                    SmtpOperationalPolicy.CredentialUnreadableCode,
                    "A credencial salva não possui um tenant válido.");

            try
            {
                return ContextualProtector(tenantId, providerType, secretType).Unprotect(stored);
            }
            catch (CryptographicException)
            {
                // Compatibilidade transparente com dados criados antes da proteção contextual.
                // Um próximo PUT regrava a credencial no propósito v2 ligado ao tenant.
                var legacyProtector = secretType == EmailSecretType.ApiKey
                    ? _legacyApiKeyProtector
                    : _legacySmtpProtector;
                return UnprotectLegacy(stored, legacyProtector);
            }
        }

        private IDataProtector ContextualProtector(
            Guid tenantId,
            EmailProviderType providerType,
            EmailSecretType secretType)
            => _contextualRoot.CreateProtector(
                tenantId.ToString("N", CultureInfo.InvariantCulture),
                providerType.ToString(),
                secretType.ToString(),
                "v2");

        private static string UnprotectLegacy(string? stored, IDataProtector protector)
        {
            if (string.IsNullOrEmpty(stored)) return stored ?? string.Empty;

            try
            {
                return protector.Unprotect(stored);
            }
            catch (CryptographicException)
            {
                // Linhas históricas em texto puro continuam legíveis para migração gradual.
                // Payloads do Data Protection começam por CfDJ; esses nunca viram plaintext.
                if (!stored.StartsWith("CfDJ", StringComparison.Ordinal)) return stored;

                throw new SmtpOperationalException(
                    SmtpOperationalPolicy.CredentialUnreadableCode,
                    "A credencial salva não pode ser decifrada. Informe-a novamente e salve a configuração.");
            }
        }
    }

    public sealed class SmtpCredentialProtector : EmailSecretProtector
    {
        public SmtpCredentialProtector(IDataProtectionProvider provider) : base(provider)
        {
        }
    }
}
