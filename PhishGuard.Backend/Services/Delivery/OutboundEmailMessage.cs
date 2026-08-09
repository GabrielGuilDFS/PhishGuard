using System.Collections.Generic;

namespace PhishGuard.Backend.Services.Delivery
{
    public class InlineAttachment
    {
        public string ContentId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MediaType { get; set; } = "image/png";
        public byte[] Bytes { get; set; } = System.Array.Empty<byte>();
    }

    public class OutboundEmailMessage
    {
        public string FromName { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public List<InlineAttachment> InlineAttachments { get; set; } = new();
    }

    public class EmailSendResult
    {
        public bool Success { get; set; }
        public string? ProviderMessageId { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public static EmailSendResult Ok(string? messageId = null) => new()
        {
            Success = true,
            ProviderMessageId = messageId
        };

        public static EmailSendResult Fail(string code, string message) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }

    public class EmailProviderTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }
}
