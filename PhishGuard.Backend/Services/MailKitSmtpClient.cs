using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using MimeKit;
using SmtpClientImpl = MailKit.Net.Smtp.SmtpClient;

namespace PhishGuard.Backend.Services
{
    /// <summary>
    /// Implementação de produção de <see cref="ISmtpClient"/> que apenas delega ao
    /// <c>MailKit.Net.Smtp.SmtpClient</c>. Sem lógica própria: toda a resiliência
    /// (retry/backoff) vive no <see cref="CampaignDispatchService"/>.
    /// </summary>
    public sealed class MailKitSmtpClient : ISmtpClient
    {
        private readonly SmtpClientImpl _inner = new();

        public bool IsConnected => _inner.IsConnected;
        public bool IsAuthenticated => _inner.IsAuthenticated;

        public Task ConnectAsync(string host, int port, SecureSocketOptions options, CancellationToken cancellationToken)
            => _inner.ConnectAsync(host, port, options, cancellationToken);

        public Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
            => _inner.AuthenticateAsync(userName, password, cancellationToken);

        public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
            => await _inner.SendAsync(message, cancellationToken);

        public Task DisconnectAsync(bool quit, CancellationToken cancellationToken)
            => _inner.DisconnectAsync(quit, cancellationToken);

        public void Dispose() => _inner.Dispose();
    }

    /// <summary>Fábrica de produção: cria um <see cref="MailKitSmtpClient"/> por disparo.</summary>
    public sealed class MailKitSmtpClientFactory : ISmtpClientFactory
    {
        public ISmtpClient Create() => new MailKitSmtpClient();
    }
}
