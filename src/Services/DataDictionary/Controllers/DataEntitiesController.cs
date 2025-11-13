using Apivia.Services.DataDictionary.DTOs;
using Apivia.Services.DataDictionary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.DataDictionary.Controllers;

/// <summary>
/// Controller for managing data entities
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataEntitiesController : ControllerBase
{
    private readonly IDataEntityService _entityService;
    private readonly ILogger<DataEntitiesController> _logger;

    public DataEntitiesController(
        IDataEntityService entityService,
        ILogger<DataEntitiesController> logger)
    {
        _entityService = entityService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of data entities
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DataEntityListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<DataEntityListItemResponse>>> GetEntities(
        [FromQuery] DataEntityQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _entityService.GetPagedAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data entities");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving data entities",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific data entity by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataEntityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataEntityResponse>> GetEntity(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _entityService.GetByIdAsync(id, cancellationToken);
            return Ok(entity);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data entity {EntityId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Entity Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data entity {EntityId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving data entity",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new data entity
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DataEntityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataEntityResponse>> CreateEntity(
        [FromBody] CreateDataEntityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var entity = await _entityService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetEntity),
                new { id = entity.Id },
                entity);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create data entity");
            return BadRequest(new ProblemDetails
            {
                Title = "Data Entity Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data entity");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating data entity",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update an existing data entity
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DataEntityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataEntityResponse>> UpdateEntity(
        Guid id,
        [FromBody] UpdateDataEntityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var entity = await _entityService.UpdateAsync(id, request, userId, cancellationToken);
            return Ok(entity);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data entity {EntityId} not found for update", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Entity Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update data entity {EntityId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Data Entity Update Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating data entity {EntityId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error updating data entity",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a data entity (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEntity(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _entityService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data entity {EntityId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Entity Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete data entity {EntityId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Deletion Not Allowed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data entity {EntityId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting data entity",
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
