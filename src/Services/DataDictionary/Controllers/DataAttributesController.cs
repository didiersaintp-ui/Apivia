using Apivia.Services.DataDictionary.DTOs;
using Apivia.Services.DataDictionary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.DataDictionary.Controllers;

/// <summary>
/// Controller for managing data attributes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataAttributesController : ControllerBase
{
    private readonly IDataAttributeService _attributeService;
    private readonly ILogger<DataAttributesController> _logger;

    public DataAttributesController(
        IDataAttributeService attributeService,
        ILogger<DataAttributesController> logger)
    {
        _attributeService = attributeService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of data attributes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DataAttributeListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<DataAttributeListItemResponse>>> GetAttributes(
        [FromQuery] DataAttributeQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _attributeService.GetPagedAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data attributes");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving data attributes",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific data attribute by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataAttributeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataAttributeResponse>> GetAttribute(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var attribute = await _attributeService.GetByIdAsync(id, cancellationToken);
            return Ok(attribute);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data attribute {AttributeId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Attribute Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data attribute {AttributeId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving data attribute",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new data attribute
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DataAttributeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataAttributeResponse>> CreateAttribute(
        [FromBody] CreateDataAttributeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var attribute = await _attributeService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetAttribute),
                new { id = attribute.Id },
                attribute);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create data attribute");
            return BadRequest(new ProblemDetails
            {
                Title = "Data Attribute Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data attribute");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating data attribute",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update an existing data attribute
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DataAttributeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataAttributeResponse>> UpdateAttribute(
        Guid id,
        [FromBody] UpdateDataAttributeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var attribute = await _attributeService.UpdateAsync(id, request, userId, cancellationToken);
            return Ok(attribute);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data attribute {AttributeId} not found for update", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Attribute Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update data attribute {AttributeId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Data Attribute Update Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating data attribute {AttributeId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error updating data attribute",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a data attribute (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttribute(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _attributeService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data attribute {AttributeId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Attribute Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete data attribute {AttributeId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Deletion Not Allowed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data attribute {AttributeId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting data attribute",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Bulk create attributes for an entity
    /// </summary>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BulkOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkOperationResponse>> BulkCreateAttributes(
        [FromBody] BulkCreateAttributesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var result = await _attributeService.BulkCreateAsync(request, userId, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to bulk create attributes");
            return BadRequest(new ProblemDetails
            {
                Title = "Bulk Create Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk creating attributes");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error bulk creating attributes",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
