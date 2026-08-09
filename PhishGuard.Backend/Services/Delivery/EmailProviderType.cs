using System;
using System.Collections.Generic;

namespace PhishGuard.Backend.Services.Delivery
{
    public enum EmailProviderType
    {
        Smtp = 0,
        ProviderApi = 1
    }

    public enum ApiProviderName
    {
        AwsSes = 0,
        Postmark = 1,
        Brevo = 2,
        SendGrid = 3,
        MailtrapSandbox = 4
    }

    public static class EmailProviderPolicy
    {
        private static readonly IReadOnlySet<string> AllowedAwsRegions = new HashSet<string>(
            new[]
            {
                "af-south-1", "ap-east-1", "ap-northeast-1", "ap-northeast-2",
                "ap-northeast-3", "ap-south-1", "ap-south-2", "ap-southeast-1",
                "ap-southeast-2", "ap-southeast-3", "ap-southeast-4", "ca-central-1",
                "ca-west-1", "eu-central-1", "eu-central-2", "eu-north-1", "eu-south-1",
                "eu-south-2", "eu-west-1", "eu-west-2", "eu-west-3", "il-central-1",
                "me-central-1", "me-south-1", "sa-east-1", "us-east-1", "us-east-2",
                "us-west-1", "us-west-2"
            },
            StringComparer.Ordinal);

        public static bool IsSupportedAwsRegion(string? region)
            => !string.IsNullOrWhiteSpace(region)
                && AllowedAwsRegions.Contains(region.Trim().ToLowerInvariant());

        public static bool TryParseMailtrapSandboxId(string? value, out long sandboxId)
            => long.TryParse(value?.Trim(), out sandboxId) && sandboxId > 0;
    }
}
