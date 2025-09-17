using HandlebarsEmailHelper.Models;

namespace HandlebarsEmailHelper.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
        Task<bool> SendTemplatedEmailAsync(int templateId, object templateData, string toEmail, string? toName = null, 
            IEnumerable<EmailAttachmentData>? attachments = null, string? culture = "en-US", CancellationToken cancellationToken = default);
        Task<bool> SendTemplatedEmailAsync(string templateName, object templateData, string toEmail, string? toName = null, 
            IEnumerable<EmailAttachmentData>? attachments = null, string? culture = "en-US", CancellationToken cancellationToken = default);
    }

    public class EmailMessage
    {
        public string ToEmail { get; set; } = string.Empty;
        public string? ToName { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string? FromName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string? PlainTextContent { get; set; }
        public IEnumerable<EmailAttachmentData>? Attachments { get; set; }
    }

    public class EmailAttachmentData
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
        public string? ContentId { get; set; } // For inline attachments
    }
}
