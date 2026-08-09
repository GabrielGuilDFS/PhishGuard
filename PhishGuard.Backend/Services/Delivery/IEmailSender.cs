using System.Threading;
using System.Threading.Tasks;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services.Delivery
{
    public interface IEmailSender
    {
        EmailProviderType ProviderType { get; }
        Task<EmailSendResult> SendAsync(
            OutboundEmailMessage message,
            SmtpConfig configuration,
            string idempotencyKey,
            CancellationToken cancellationToken);

        Task<EmailProviderTestResult> TestAsync(
            SmtpConfig configuration,
            string destinationEmail,
            CancellationToken cancellationToken);
    }
}
