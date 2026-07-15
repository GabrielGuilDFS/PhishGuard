using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using MimeKit;

namespace PhishGuard.Backend.Services
{
    /// <summary>
    /// Abstração fina sobre o cliente SMTP (MailKit). Existe para DESACOPLAR o
    /// <see cref="CampaignDispatchService"/> do <c>SmtpClient</c> concreto, permitindo
    /// injetar um duplo de teste que simula falhas transitórias/definitivas de rede e,
    /// assim, cobrir o caminho de resiliência (retry Polly + registro de "Falha") sem
    /// depender de um servidor SMTP real.
    /// </summary>
    public interface ISmtpClient : IDisposable
    {
        bool IsConnected { get; }
        bool IsAuthenticated { get; }

        Task ConnectAsync(string host, int port, SecureSocketOptions options, CancellationToken cancellationToken);
        Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken);
        Task SendAsync(MimeMessage message, CancellationToken cancellationToken);
        Task DisconnectAsync(bool quit, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Fábrica de <see cref="ISmtpClient"/>. Cada disparo cria um cliente de vida curta
    /// (conexão SMTP não deve ser mantida por longos períodos no pool) — espelha o antigo
    /// <c>using var client = new SmtpClient()</c>, agora injetável.
    /// </summary>
    public interface ISmtpClientFactory
    {
        ISmtpClient Create();
    }
}
