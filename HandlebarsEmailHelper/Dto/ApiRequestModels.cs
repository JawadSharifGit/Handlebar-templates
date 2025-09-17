using System.ComponentModel.DataAnnotations;

namespace HandlebarsEmailHelper.Dto
{
    public class CreateTemplateRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string HtmlBody { get; set; } = string.Empty;
    }

    public class UpdateTemplateRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string HtmlBody { get; set; } = string.Empty;
    }

    public class RenderTemplateRequest
    {
        [Required]
        public object Data { get; set; } = new();

        public string? Culture { get; set; } = "en-US";
    }

    public class SendTestEmailRequest
    {
        [Required]
        [EmailAddress]
        public string ToEmail { get; set; } = string.Empty;

        public string? ToName { get; set; }

        [Required]
        public object TestData { get; set; } = new();

        public string? Culture { get; set; } = "en-US";
    }
}
