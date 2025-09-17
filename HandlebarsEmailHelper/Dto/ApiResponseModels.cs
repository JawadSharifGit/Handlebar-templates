namespace HandlebarsEmailHelper.Dto
{
    public class RenderedTemplateResponse
    {
        public int TemplateId { get; set; }
        public string RenderedSubject { get; set; } = string.Empty;
        public string RenderedHtmlBody { get; set; } = string.Empty;
        public string Culture { get; set; } = string.Empty;
        public DateTime RenderedAt { get; set; }
    }

    public class SendTestEmailResponse
    {
        public bool Success { get; set; }
        public int TemplateId { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class HelpersResponse
    {
        public Dictionary<string, string> DateTimeHelpers { get; set; } = new();
        public Dictionary<string, string> NumberHelpers { get; set; } = new();
        public Dictionary<string, string> TextHelpers { get; set; } = new();
        public Dictionary<string, string> LogicHelpers { get; set; } = new();
        public Dictionary<string, string> PartialHelpers { get; set; } = new();
    }

    public class ApiErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
    }
}
