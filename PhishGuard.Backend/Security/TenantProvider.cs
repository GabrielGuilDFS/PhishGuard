using System;
using Microsoft.AspNetCore.Http;
using PhishGuard.Backend.Data;

namespace PhishGuard.Backend.Security
{
    public sealed class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentTenantId()
        {
            var tenantIdValue = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantIdValue, out var tenantId) ? tenantId : Guid.Empty;
        }

        public Guid GetTenantId()
        {
            return GetCurrentTenantId();
        }
    }
}

