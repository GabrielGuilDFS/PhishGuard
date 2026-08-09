using System;
using System.Collections.Generic;
using System.Linq;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services.Delivery
{
    public interface IEmailSenderResolver
    {
        IEmailSender Resolve(SmtpConfig config);
    }

    public class EmailSenderResolver : IEmailSenderResolver
    {
        private readonly IEnumerable<IEmailSender> _senders;

        public EmailSenderResolver(IEnumerable<IEmailSender> senders)
        {
            _senders = senders;
        }

        public IEmailSender Resolve(SmtpConfig config)
        {
            var providerType = config.ProviderType;

            return _senders.FirstOrDefault(sender => sender.ProviderType == providerType)
                ?? throw new InvalidOperationException(
                    $"Nenhum transporte de e-mail foi registrado para {providerType}.");
        }
    }
}
