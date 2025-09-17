using HandlebarsEmailHelper.Models;
using Microsoft.EntityFrameworkCore;
using HandlebarsEmailHelper.Interfaces;

namespace HandlebarsEmailHelper.Services;

public class EmailTemplateRepository : IEmailTemplateService
{
    private readonly AppDbContext _db;
    public EmailTemplateRepository(AppDbContext db) { _db = db; }

    // Email Template methods
    public Task<List<EmailTemplate>> GetAllAsync(CancellationToken ct = default)
        => _db.EmailTemplates.ToListAsync(ct);

    public Task<EmailTemplate?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.EmailTemplates.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken ct = default)
        => _db.EmailTemplates.FirstOrDefaultAsync(t => t.Name == name, ct);

    public Task<EmailTemplate?> GetFirstAsync(CancellationToken ct = default)
        => _db.EmailTemplates.OrderBy(t => t.Id).FirstOrDefaultAsync(ct);

    public async Task AddAsync(EmailTemplate template, CancellationToken ct = default)
    {
        _db.EmailTemplates.Add(template);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(EmailTemplate template, CancellationToken ct = default)
    {
        _db.EmailTemplates.Update(template);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(id, ct);
        if (template != null)
        {
            _db.EmailTemplates.Remove(template);
            await _db.SaveChangesAsync(ct);
        }
    }

    // Partials methods
    public Task<List<Partial>> GetPartialsAsync(CancellationToken ct = default)
        => _db.Partials.ToListAsync(ct);

    public Task<Partial?> GetPartialByIdAsync(int id, CancellationToken ct = default)
        => _db.Partials.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Partial?> GetPartialByNameAsync(string name, CancellationToken ct = default)
        => _db.Partials.FirstOrDefaultAsync(p => p.Name == name, ct);

    public async Task AddPartialAsync(Partial partial, CancellationToken ct = default)
    {
        _db.Partials.Add(partial);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdatePartialAsync(Partial partial, CancellationToken ct = default)
    {
        _db.Partials.Update(partial);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeletePartialAsync(int id, CancellationToken ct = default)
    {
        var partial = await GetPartialByIdAsync(id, ct);
        if (partial != null)
        {
            _db.Partials.Remove(partial);
            await _db.SaveChangesAsync(ct);
        }
    }

    // Attachment methods
    public Task<List<EmailAttachment>> GetTemplateAttachmentsAsync(int templateId, CancellationToken ct = default)
        => _db.EmailAttachments.Where(a => a.EmailTemplateId == templateId).ToListAsync(ct);

    public async Task AddAttachmentAsync(EmailAttachment attachment, CancellationToken ct = default)
    {
        _db.EmailAttachments.Add(attachment);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAttachmentAsync(int attachmentId, CancellationToken ct = default)
    {
        var attachment = await _db.EmailAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (attachment != null)
        {
            _db.EmailAttachments.Remove(attachment);
            await _db.SaveChangesAsync(ct);
        }
    }
}


