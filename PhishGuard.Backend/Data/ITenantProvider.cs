using System;

namespace PhishGuard.Backend.Data
{
    public interface ITenantProvider
    {
        Guid GetTenantId();
        Guid GetCurrentTenantId();
    }
}
