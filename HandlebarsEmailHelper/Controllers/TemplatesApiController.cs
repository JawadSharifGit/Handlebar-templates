using HandlebarsEmailHelper.Dto;
using HandlebarsEmailHelper.Services;
using HandlebarsEmailHelper.Interfaces;
using HandlebarsEmailHelper.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HandlebarsEmailHelper.Controllers;

[ApiController]
[Route("api/templates")]
[Produces("application/json")]
public class TemplatesApiController : ControllerBase
{
    private readonly ITemplateApplicationService _appService;
    private readonly IEmailTemplateService _templateService;
    private readonly IEmailService _emailService;
    private readonly ILogger<TemplatesApiController> _logger;

    public TemplatesApiController(
        ITemplateApplicationService appService,
        IEmailTemplateService templateService,
        IEmailService emailService,
        ILogger<TemplatesApiController> logger)
    {
        _appService = appService;
        _templateService = templateService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Get all email templates
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmailTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetAllTemplates(CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _appService.GetAllAsync();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific template by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmailTemplate), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailTemplate>> GetTemplate(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateService.GetByIdAsync(id, cancellationToken);
            if (template == null)
                return NotFound(new { error = "Template not found", templateId = id });
            
            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {TemplateId}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new email template
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmailTemplate), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmailTemplate>> CreateTemplate([FromBody] CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = new EmailTemplate
            {
                Name = request.Name,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody
            };

            await _templateService.AddAsync(template, cancellationToken);
            
            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing email template
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingTemplate = await _templateService.GetByIdAsync(id, cancellationToken);
            if (existingTemplate == null)
                return NotFound(new { error = "Template not found", templateId = id });

            existingTemplate.Name = request.Name;
            existingTemplate.Subject = request.Subject;
            existingTemplate.HtmlBody = request.HtmlBody;

            await _templateService.UpdateAsync(existingTemplate, cancellationToken);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete an email template
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateService.GetByIdAsync(id, cancellationToken);
            if (template == null)
                return NotFound(new { error = "Template not found", templateId = id });

            await _templateService.DeleteAsync(id, cancellationToken);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Render a template with provided data for preview/testing
    /// </summary>
    [HttpPost("{id:int}/render")]
    [ProducesResponseType(typeof(RenderedTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RenderedTemplateResponse>> RenderTemplate(int id, [FromBody] RenderTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _appService.GenerateTemplateByIdAsync(id, new TemplateDto { Data = request.Data }, cancellationToken);
            if (dto == null)
                return NotFound(new { error = "Template not found", templateId = id });
            
            return Ok(new RenderedTemplateResponse
            {
                TemplateId = id,
                RenderedSubject = dto.Subject,
                RenderedHtmlBody = dto.HtmlBody,
                Culture = request.Culture ?? "en-US",
                RenderedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateId}", id);
            return BadRequest(new { error = "Template rendering failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Send a test email using a template
    /// </summary>
    [HttpPost("{id:int}/send-test")]
    [ProducesResponseType(typeof(SendTestEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SendTestEmailResponse>> SendTestEmail(int id, [FromBody] SendTestEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _emailService.SendTemplatedEmailAsync(
                id, 
                request.TestData, 
                request.ToEmail, 
                request.ToName, 
                null, // No attachments for test emails
                request.Culture ?? "en-US", 
                cancellationToken);
            
            return Ok(new SendTestEmailResponse
            {
                Success = success,
                TemplateId = id,
                ToEmail = request.ToEmail,
                SentAt = DateTime.UtcNow,
                Message = success ? "Test email sent successfully (simulated)" : "Failed to send test email"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email for template {TemplateId}", id);
            return BadRequest(new { error = "Failed to send test email", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available Handlebars helpers and their usage
    /// </summary>
    [HttpGet("helpers")]
    [ProducesResponseType(typeof(HelpersResponse), StatusCodes.Status200OK)]
    public ActionResult<HelpersResponse> GetHelpers()
    {
        var helpers = new HelpersResponse
        {
            DateTimeHelpers = new Dictionary<string, string>
            {
                { "{{currentDate \"yyyy-MM-dd\" \"en-US\"}}", "Current date with format and culture" },
                { "{{formatDate dateValue \"MMM dd, yyyy\" \"en-US\"}}", "Format any date value" },
                { "{{year \"en-US\"}}", "Current year with culture" }
            },
            NumberHelpers = new Dictionary<string, string>
            {
                { "{{currency amount \"en-US\"}}", "Format as currency" },
                { "{{formatNumber value \"N2\" \"en-US\"}}", "Format number with culture" },
                { "{{percentage value \"en-US\"}}", "Format as percentage" }
            },
            TextHelpers = new Dictionary<string, string>
            {
                { "{{uppercase text \"en-US\"}}", "Convert to uppercase" },
                { "{{lowercase text \"en-US\"}}", "Convert to lowercase" },
                { "{{titleCase text \"en-US\"}}", "Convert to title case" },
                { "{{htmlEncode text}}", "HTML encode text" },
                { "{{urlEncode text}}", "URL encode text" }
            },
            LogicHelpers = new Dictionary<string, string>
            {
                { "{{#eq value1 value2}}...{{/eq}}", "If values are equal" },
                { "{{#neq value1 value2}}...{{/neq}}", "If values are not equal" },
                { "{{#gt value1 value2}}...{{/gt}}", "If value1 > value2" },
                { "{{#lt value1 value2}}...{{/lt}}", "If value1 < value2" },
                { "{{#ifNotEmpty value}}...{{/ifNotEmpty}}", "If value is not empty" }
            },
            PartialHelpers = new Dictionary<string, string>
            {
                { "{{> partialName}}", "Include a partial template" },
                { "{{> header}}", "Include header partial" },
                { "{{> footer}}", "Include footer partial" }
            }
        };
        
        return Ok(helpers);
    }
}