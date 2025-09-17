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

        var data = templateDto?.Data ?? new { Empty = string.Empty };
        var resultHtml = await CompileAsync(templateDto?.HtmlContent, template.HtmlBody, data, ct);
        var resultSubject = await CompileAsync(null, template.Subject, data, ct);

        return new EmailTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Subject = resultSubject,
            HtmlBody = resultHtml
        };
    }

    public async Task<EmailTemplateDto?> GenerateTemplateByIdAsync(int id, TemplateDto templateDto, CancellationToken ct = default)
    {
        var template = await _repository.GetByIdAsync(id, ct);
        if (template == null) return null;

        var data = templateDto?.Data ?? new { Empty = string.Empty };
        var resultHtml = await CompileAsync(templateDto?.HtmlContent, template.HtmlBody, data, ct);
        var resultSubject = await CompileAsync(null, template.Subject, data, ct);

        return new EmailTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Subject = resultSubject,
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

    private async Task<string> CompileAsync(string? preferredHtml, string fallbackHtml, object data, CancellationToken ct = default)
    {
        var body = !string.IsNullOrEmpty(preferredHtml) ? preferredHtml! : fallbackHtml;
        
        // Fetch all partials from the database
        var partials = await _repository.GetPartialsAsync(ct);
        var partialsDictionary = partials.ToDictionary(p => p.Name, p => p.HtmlContent);
        
        return _templateService.CompileTemplate(body, data, partialsDictionary);
    }
}


