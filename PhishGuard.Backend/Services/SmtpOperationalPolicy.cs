using Microsoft.Extensions.Configuration;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services;

public sealed class SmtpOperationalException : InvalidOperationException
{
    public SmtpOperationalException(string code, string message) : base(message) => Code = code;
    public string Code { get; }
}

public static class SmtpOperationalPolicy
{
    public const string NotConfiguredCode = "SMTP_NOT_CONFIGURED";
    public const string CredentialUnreadableCode = "SMTP_CREDENTIAL_UNREADABLE";
    public const string TransportUnavailableCode = "SMTP_TRANSPORT_UNAVAILABLE";
    public const string AuthenticationFailedCode = "SMTP_AUTHENTICATION_FAILED";
    public const string ConnectionTimeoutCode = "SMTP_CONNECTION_TIMEOUT";
    public const string ConnectionFailedCode = "SMTP_CONNECTION_FAILED";

    public static bool IsConfigured(SmtpConfig? config) =>
        config is not null
        && !string.IsNullOrWhiteSpace(config.Host)
        && config.Porta is > 0 and <= 65535
        && !string.IsNullOrWhiteSpace(config.Usuario)
        && !string.IsNullOrWhiteSpace(config.Senha);

    public static string? ValidateCredential(
        SmtpConfig? config,
        ISmtpCredentialProtector credentialProtector)
    {
        if (!IsConfigured(config)) return NotConfiguredCode;

        try
        {
            return string.IsNullOrWhiteSpace(credentialProtector.Unprotect(config!.Senha))
                ? CredentialUnreadableCode
                : null;
        }
        catch (SmtpOperationalException ex)
        {
            return ex.Code;
        }
    }

    public static bool IsTransportEnabled(IConfiguration configuration) =>
        configuration.GetValue<bool?>("AppSettings:SmtpTransportEnabled") ?? true;

    public static string? GetTransportDisabledReason(IConfiguration configuration) =>
        IsTransportEnabled(configuration)
            ? null
            : configuration["AppSettings:SmtpTransportDisabledReason"]
                ?? "O transporte SMTP está indisponível neste ambiente.";

    public static SmtpStatusDto ToStatus(
        SmtpConfig? config,
        IConfiguration configuration,
        string? credentialErrorCode = null) => new()
    {
        Configurado = IsConfigured(config) && credentialErrorCode is null,
        SenhaConfigurada = !string.IsNullOrWhiteSpace(config?.Senha),
        TransporteDisponivel = IsTransportEnabled(configuration),
        TransporteIndisponivelMotivo = GetTransportDisabledReason(configuration),
        UltimoTesteEmUtc = config?.UltimoTesteEmUtc,
        UltimoTesteSucesso = config?.UltimoTesteSucesso,
        UltimoErroCodigo = credentialErrorCode ?? config?.UltimoErroCodigo,
    };

    public static (string Code, string Message) Classify(Exception exception) => exception switch
    {
        SmtpOperationalException operational => (operational.Code, operational.Message),
        MailKit.Security.AuthenticationException =>
            (AuthenticationFailedCode, "O servidor SMTP recusou o usuário ou a senha. Para Gmail, use uma senha de aplicativo."),
        TimeoutException =>
            (ConnectionTimeoutCode, "A conexão com o servidor SMTP expirou. Verifique host, porta e as restrições de rede do provedor de hospedagem."),
        System.Net.Sockets.SocketException =>
            (ConnectionFailedCode, "Não foi possível conectar ao servidor SMTP. Verifique host, porta e as restrições de rede do ambiente."),
        MailKit.Security.SslHandshakeException =>
            (ConnectionFailedCode, "Falha ao negociar uma conexão segura com o servidor SMTP. Verifique a porta e o modo TLS."),
        _ => (ConnectionFailedCode, "Não foi possível concluir a operação SMTP. Verifique a configuração e tente novamente."),
    };
}
