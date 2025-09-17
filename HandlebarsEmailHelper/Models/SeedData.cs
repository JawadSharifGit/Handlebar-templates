using Microsoft.EntityFrameworkCore;

namespace HandlebarsEmailHelper.Models;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        // Seed Partials first
        if (!await db.Partials.AnyAsync())
        {
            await SeedPartials(db);
        }

        // Seed Email Templates
        if (!await db.EmailTemplates.AnyAsync())
        {
            await SeedEmailTemplates(db);
        }
    }

    private static async Task SeedPartials(AppDbContext db)
    {
        var partials = new[]
        {
            new Partial
            {
                Name = "header",
                HtmlContent = "<div style=\"background-color: #667eea; color: white; padding: 20px; text-align: center;\">" +
                             "<h1 style=\"margin: 0;\">{{Company}}</h1>" +
                             "<p style=\"margin: 5px 0 0 0;\">Professional Email Communications</p>" +
                             "</div>"
            },
            new Partial
            {
                Name = "footer",
                HtmlContent = "<div style=\"background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #dee2e6;\">" +
                             "<p style=\"margin: 0; color: #6c757d; font-size: 12px;\">" +
                             "  {{year}} {{Company}}. All rights reserved.<br>" +
                             "Generated on {{currentDate \"MMM dd, yyyy\" \"en-US\"}}" +
                             "</p></div>"
            },
            new Partial
            {
                Name = "button",
                HtmlContent = "<div style=\"text-align: center; margin: 20px 0;\">" +
                             "<a href=\"{{url}}\" style=\"background-color: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;\">{{text}}</a>" +
                             "</div>"
            }
        };
        
        db.Partials.AddRange(partials);
        await db.SaveChangesAsync();
    }

    private static async Task SeedEmailTemplates(AppDbContext db)
    {
        var templates = new[]
        {
            new EmailTemplate
            {
                Name = "Welcome Email",
                Subject = "Welcome to {{Company}}, {{Name}}!",
                HtmlBody = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Welcome</title></head>" +
                          "<body style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\">" +
                          "{{> header}}" +
                          "<div style=\"padding: 30px;\">" +
                          "<h2>Welcome {{titleCase Name}}!</h2>" +
                          "<p>We're excited to have you join {{Company}}. Your account has been successfully created.</p>" +
                          "{{#ifNotEmpty Email}}<p><strong>Your email:</strong> {{Email}}</p>{{/ifNotEmpty}}" +
                          "<p>Registration Date: {{formatDate Date \"MMM dd, yyyy\" \"en-US\"}}</p>" +
                          "</div>" +
                          "{{> footer}}" +
                          "</body></html>"
            },
            new EmailTemplate
            {
                Name = "Invoice",
                Subject = "Invoice #{{InvoiceNumber}} - {{currency Amount \"en-US\"}}",
                HtmlBody = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Invoice</title></head>" +
                          "<body style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\">" +
                          "{{> header}}" +
                          "<div style=\"padding: 30px;\">" +
                          "<h2>Invoice #{{InvoiceNumber}}</h2>" +
                          "<p><strong>Bill To:</strong><br>{{Name}}<br>{{Email}}</p>" +
                          "<p><strong>Invoice Date:</strong> {{formatDate InvoiceDate \"MMM dd, yyyy\" \"en-US\"}}</p>" +
                          "<p><strong>Amount:</strong> {{currency Amount \"en-US\"}}</p>" +
                          "{{#gt Amount 1000}}<div style=\"background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0;\">" +
                          "<strong>High Value Invoice:</strong> This invoice exceeds $1,000.</div>{{/gt}}" +
                          "<p>Thank you for your business!</p>" +
                          "</div>" +
                          "{{> footer}}" +
                          "</body></html>"
            },
            new EmailTemplate
            {
                Name = "Password Reset",
                Subject = "Reset Your Password - {{Company}}",
                HtmlBody = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Password Reset</title></head>" +
                          "<body style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\">" +
                          "{{> header}}" +
                          "<div style=\"padding: 30px;\">" +
                          "<h2>Password Reset Request</h2>" +
                          "<p>Hello {{Name}},</p>" +
                          "<p>We received a request to reset your password for your {{Company}} account.</p>" +
                          "<p style=\"color: #dc3545;\"><strong>Important:</strong> This link will expire in 24 hours.</p>" +
                          "<p>If you didn't request this, please ignore this email.</p>" +
                          "<p>Best regards,<br>The {{Company}} Team</p>" +
                          "</div>" +
                          "{{> footer}}" +
                          "</body></html>"
            }
        };
        
        db.EmailTemplates.AddRange(templates);
        await db.SaveChangesAsync();
    }
}
