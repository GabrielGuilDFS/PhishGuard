using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace PhishGuard.Backend.Services
{
    /// <summary>
    /// Cifra/decifra a senha do SMTP do tenant para proteção de dados EM REPOUSO.
    /// Centraliza o "purpose" do protetor e a compatibilidade com registros legados
    /// (senhas gravadas em texto puro antes desta mudança).
    /// </summary>
    public interface ISmtpCredentialProtector
    {
        /// <summary>Cifra a senha para persistência. Entrada vazia é devolvida como está.</summary>
        string Protect(string? plaintext);

        /// <summary>
        /// Decifra a senha lida do banco. Se o valor não estiver cifrado (registro
        /// legado em texto puro) ou as chaves de proteção não baterem, devolve o valor
        /// original em vez de lançar — evita derrubar o disparo por dado legado.
        /// </summary>
        string Unprotect(string? stored);
    }

    public sealed class SmtpCredentialProtector : ISmtpCredentialProtector
    {
        // Versionado no purpose: permite rotacionar o esquema no futuro sem ambiguidade.
        private const string Purpose = "PhishGuard.SmtpConfig.Senha.v1";

        private readonly IDataProtector _protector;

        public SmtpCredentialProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector(Purpose);
        }

        public string Protect(string? plaintext)
            => string.IsNullOrEmpty(plaintext) ? (plaintext ?? string.Empty) : _protector.Protect(plaintext);

        public string Unprotect(string? stored)
        {
            if (string.IsNullOrEmpty(stored))
                return stored ?? string.Empty;

            try
            {
                return _protector.Unprotect(stored);
            }
            catch (CryptographicException)
            {
                // Registro legado em texto puro (ou cifrado com chave indisponível):
                // devolve como está para não quebrar o fluxo. Ao salvar de novo pela
                // tela de Configurações, o valor passa a ser cifrado.
                return stored;
            }
        }
    }
}
