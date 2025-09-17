using HandlebarsEmailHelper.Interfaces;
using HandlebarsEmailHelper.Services;

namespace HandlebarsEmailHelper.Models;

public class EmailTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    // Method to render template with data
    public string RenderSubject(object data, ITemplateService templateService)
    {
        return templateService.CompileTemplate(Subject, data);
    }

    public string RenderBody(object data, ITemplateService templateService)
    {
        return templateService.CompileTemplate(HtmlBody, data);
    }
}

public class Partial
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
}

public class EmailAttachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public int EmailTemplateId { get; set; }
}


