using System;

namespace PhishGuard.Backend.Data
{
    public interface ITenantProvider
    {
        Guid GetCurrentTenantId();
    }
}
