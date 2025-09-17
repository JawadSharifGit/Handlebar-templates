using HandlebarsEmailHelper.Interfaces;
using HandlebarsEmailHelper.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace HandlebarsEmailHelper.Services
{
    public class ConsoleEmailService : IEmailService
    {
        private readonly IEmailTemplateService _templateService;
        private readonly ITemplateService _handlebarsService;
        private readonly EmailOptions _options;
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(
            IEmailTemplateService templateService,
            ITemplateService handlebarsService,
            IOptions<EmailOptions> options,
            ILogger<ConsoleEmailService> logger)
        {
            _templateService = templateService;
            _handlebarsService = handlebarsService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                // Simulate async operation
                await Task.Delay(100, cancellationToken);
                
                // Log email details to console
                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine("EMAIL SENT (SIMULATED)");
                Console.WriteLine(new string('=', 80));
                Console.WriteLine($"From: {emailMessage.FromName} <{emailMessage.FromEmail}>");
                Console.WriteLine($"To: {emailMessage.ToName} <{emailMessage.ToEmail}>");
                Console.WriteLine($"Subject: {emailMessage.Subject}");
                Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                
                if (emailMessage.Attachments?.Any() == true)
                {
                    Console.WriteLine($"Attachments: {emailMessage.Attachments.Count()}");
                    foreach (var attachment in emailMessage.Attachments)
                    {
                        Console.WriteLine($"  - {attachment.FileName} ({attachment.ContentType}, {attachment.Content.Length} bytes)");
                    }
                }
                
                Console.WriteLine("\nHTML Content:");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine(emailMessage.HtmlContent);
                Console.WriteLine(new string('=', 80) + "\n");
                
                _logger.LogInformation("Email sent successfully (simulated) to {ToEmail}", emailMessage.ToEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email to {ToEmail}", emailMessage.ToEmail);
                return false;
            }
        }

        public async Task<bool> SendTemplatedEmailAsync(int templateId, object templateData, string toEmail, string? toName = null, 
            IEnumerable<EmailAttachmentData>? attachments = null, string? culture = "en-US", CancellationToken cancellationToken = default)
        {
            try
            {
                var template = await _templateService.GetByIdAsync(templateId, cancellationToken);
                if (template == null)
                {
                    _logger.LogError("Template with ID {TemplateId} not found", templateId);
                    return false;
                }

                return await SendTemplatedEmailInternalAsync(template, templateData, toEmail, toName, attachments, culture, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending templated email using template ID {TemplateId}", templateId);
                return false;
            }
        }

        public async Task<bool> SendTemplatedEmailAsync(string templateName, object templateData, string toEmail, string? toName = null, 
            IEnumerable<EmailAttachmentData>? attachments = null, string? culture = "en-US", CancellationToken cancellationToken = default)
        {
            try
            {
                var template = await _templateService.GetByNameAsync(templateName, cancellationToken);
                if (template == null)
                {
                    _logger.LogError("Template with name {TemplateName} not found", templateName);
                    return false;
                }

                return await SendTemplatedEmailInternalAsync(template, templateData, toEmail, toName, attachments, culture, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending templated email using template name {TemplateName}", templateName);
                return false;
            }
        }

        private async Task<bool> SendTemplatedEmailInternalAsync(EmailTemplate template, object templateData, 
            string toEmail, string? toName, IEnumerable<EmailAttachmentData>? attachments, string? culture, CancellationToken cancellationToken)
        {
            try
            {
                // Set culture for template rendering if provided
                if (!string.IsNullOrEmpty(culture))
                {
                    try
                    {
                        var cultureInfo = new CultureInfo(culture);
                        Thread.CurrentThread.CurrentCulture = cultureInfo;
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    }
                    catch (CultureNotFoundException ex)
                    {
                        _logger.LogWarning(ex, "Invalid culture {Culture} provided, using default", culture);
                    }
                }

                // Get partials for this template
                var partials = await _templateService.GetPartialsAsync(cancellationToken);
                var partialsDictionary = partials.ToDictionary(p => p.Name, p => p.HtmlContent);

                // Render subject and body
                var renderedSubject = _handlebarsService.CompileTemplate(template.Subject, templateData, partialsDictionary);
                var renderedBody = _handlebarsService.CompileTemplate(template.HtmlBody, templateData, partialsDictionary);

                // Create email message
                var emailMessage = new EmailMessage
                {
                    ToEmail = toEmail,
                    ToName = toName,
                    FromEmail = _options.FromEmail,
                    FromName = _options.FromName,
                    Subject = renderedSubject,
                    HtmlContent = renderedBody,
                    Attachments = attachments
                };

                return await SendEmailAsync(emailMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing template {TemplateName}", template.Name);
                return false;
            }
        }
    }

    public class EmailOptions
    {
        public const string SectionName = "Email";
        
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
