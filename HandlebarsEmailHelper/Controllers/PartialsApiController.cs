using HandlebarsEmailHelper.Interfaces;
using HandlebarsEmailHelper.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HandlebarsEmailHelper.Controllers
{
    [ApiController]
    [Route("api/partials")]
    [Produces("application/json")]
    public class PartialsApiController : ControllerBase
    {
        private readonly IEmailTemplateService _templateService;
        private readonly ILogger<PartialsApiController> _logger;

        public PartialsApiController(
            IEmailTemplateService templateService,
            ILogger<PartialsApiController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        /// <summary>
        /// Get all partials
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Partial>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Partial>>> GetAllPartials(CancellationToken cancellationToken = default)
        {
            try
            {
                var partials = await _templateService.GetPartialsAsync(cancellationToken);
                return Ok(partials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partials");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific partial by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Partial), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Partial>> GetPartial(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var partial = await _templateService.GetPartialByIdAsync(id, cancellationToken);
                if (partial == null)
                    return NotFound(new { error = "Partial not found", partialId = id });
                
                return Ok(partial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partial {PartialId}", id);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get a partial by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        [ProducesResponseType(typeof(Partial), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Partial>> GetPartialByName(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var partial = await _templateService.GetPartialByNameAsync(name, cancellationToken);
                if (partial == null)
                    return NotFound(new { error = "Partial not found", partialName = name });
                
                return Ok(partial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partial by name {PartialName}", name);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new partial
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Partial), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Partial>> CreatePartial([FromBody] CreatePartialRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var partial = new Partial
                {
                    Name = request.Name,
                    HtmlContent = request.HtmlContent
                };

                await _templateService.AddPartialAsync(partial, cancellationToken);
                
                return CreatedAtAction(nameof(GetPartial), new { id = partial.Id }, partial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partial");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing partial
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] UpdatePartialRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingPartial = await _templateService.GetPartialByIdAsync(id, cancellationToken);
                if (existingPartial == null)
                    return NotFound(new { error = "Partial not found", partialId = id });

                existingPartial.Name = request.Name;
                existingPartial.HtmlContent = request.HtmlContent;

                await _templateService.UpdatePartialAsync(existingPartial, cancellationToken);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partial {PartialId}", id);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a partial
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePartial(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var partial = await _templateService.GetPartialByIdAsync(id, cancellationToken);
                if (partial == null)
                    return NotFound(new { error = "Partial not found", partialId = id });

                await _templateService.DeletePartialAsync(id, cancellationToken);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting partial {PartialId}", id);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }

    public class CreatePartialRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string HtmlContent { get; set; } = string.Empty;
    }

    public class UpdatePartialRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string HtmlContent { get; set; } = string.Empty;
    }
}
