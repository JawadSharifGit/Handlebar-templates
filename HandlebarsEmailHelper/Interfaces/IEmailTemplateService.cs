using HandlebarsEmailHelper.Models;

namespace HandlebarsEmailHelper.Interfaces
{
    public interface IEmailTemplateService
    {
        Task<List<EmailTemplate>> GetAllAsync(CancellationToken ct = default);
        Task<EmailTemplate?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<EmailTemplate?> GetFirstAsync(CancellationToken ct = default);
        Task AddAsync(EmailTemplate template, CancellationToken ct = default);
        Task UpdateAsync(EmailTemplate template, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        
        // Partials management
        Task<List<Partial>> GetPartialsAsync(CancellationToken ct = default);
        Task<Partial?> GetPartialByIdAsync(int id, CancellationToken ct = default);
        Task<Partial?> GetPartialByNameAsync(string name, CancellationToken ct = default);
        Task AddPartialAsync(Partial partial, CancellationToken ct = default);
        Task UpdatePartialAsync(Partial partial, CancellationToken ct = default);
        Task DeletePartialAsync(int id, CancellationToken ct = default);
        
        // Template attachments
        Task<List<EmailAttachment>> GetTemplateAttachmentsAsync(int templateId, CancellationToken ct = default);
        Task AddAttachmentAsync(EmailAttachment attachment, CancellationToken ct = default);
        Task DeleteAttachmentAsync(int attachmentId, CancellationToken ct = default);
    }
}


