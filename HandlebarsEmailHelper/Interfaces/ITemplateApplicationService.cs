using HandlebarsEmailHelper.Dto;

namespace HandlebarsEmailHelper.Interfaces
{
    public interface ITemplateApplicationService
    {
        Task<IEnumerable<EmailTemplateDto>> GetAllAsync(CancellationToken ct = default);
        Task<EmailTemplateDto?> GenerateTemplateByIdAsync(int id, TemplateDto templateDto, CancellationToken ct = default);
        Task<EmailTemplateDto?> GenerateTemplateAsync(TemplateDto templateDto, CancellationToken ct = default);
    }
}


