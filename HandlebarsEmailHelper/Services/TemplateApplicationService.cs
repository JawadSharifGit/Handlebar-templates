using HandlebarsEmailHelper.Dto;
using HandlebarsEmailHelper.Models;
using HandlebarsEmailHelper.Interfaces;

namespace HandlebarsEmailHelper.Services;

public class TemplateApplicationService : ITemplateApplicationService
{
    private readonly IEmailTemplateService _repository;
    private readonly ITemplateService _templateService;

    public TemplateApplicationService(IEmailTemplateService repository, ITemplateService templateService)
    {
        _repository = repository;
        _templateService = templateService;
    }

    public async Task<EmailTemplateDto?> GenerateTemplateAsync(TemplateDto templateDto, CancellationToken ct = default)
    {
        var template = await _repository.GetFirstAsync(ct);
        if (template == null) return null;

        var resultHtml = Compile(templateDto?.HtmlContent, template.HtmlBody);

        return new EmailTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Subject = template.Subject,
            HtmlBody = resultHtml
        };
    }

    public async Task<EmailTemplateDto?> GenerateTemplateByIdAsync(int id, TemplateDto templateDto, CancellationToken ct = default)
    {
        var template = await _repository.GetByIdAsync(id, ct);
        if (template == null) return null;

        var resultHtml = Compile(templateDto?.HtmlContent, template.HtmlBody);

        return new EmailTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Subject = template.Subject,
            HtmlBody = resultHtml
        };
    }

    public async Task<IEnumerable<EmailTemplateDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        return items.Select(t => new EmailTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Subject = t.Subject,
            HtmlBody = t.HtmlBody
        });
    }

    private string Compile(string? preferredHtml, string fallbackHtml)
    {
        var body = !string.IsNullOrEmpty(preferredHtml) ? preferredHtml! : fallbackHtml;
        return _templateService.CompileTemplate(body, new { Empty = string.Empty });
    }
}


