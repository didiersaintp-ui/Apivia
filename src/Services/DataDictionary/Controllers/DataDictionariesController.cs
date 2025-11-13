using Apivia.Services.DataDictionary.DTOs;
using Apivia.Services.DataDictionary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.DataDictionary.Controllers;

/// <summary>
/// Controller for managing data dictionaries
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataDictionariesController : ControllerBase
{
    private readonly IDataDictionaryService _dictionaryService;
    private readonly ILogger<DataDictionariesController> _logger;

    public DataDictionariesController(
        IDataDictionaryService dictionaryService,
        ILogger<DataDictionariesController> logger)
    {
        _dictionaryService = dictionaryService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of data dictionaries
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DataDictionaryListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<DataDictionaryListItemResponse>>> GetDictionaries(
        [FromQuery] DataDictionaryQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _dictionaryService.GetPagedAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data dictionaries");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving data dictionaries",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific data dictionary by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataDictionaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataDictionaryResponse>> GetDictionary(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var dictionary = await _dictionaryService.GetByIdAsync(id, cancellationToken);
            return Ok(dictionary);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data dictionary {DictionaryId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Dictionary Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data dictionary {DictionaryId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving data dictionary",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new data dictionary
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DataDictionaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataDictionaryResponse>> CreateDictionary(
        [FromBody] CreateDataDictionaryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var dictionary = await _dictionaryService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetDictionary),
                new { id = dictionary.Id },
                dictionary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create data dictionary");
            return BadRequest(new ProblemDetails
            {
                Title = "Data Dictionary Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data dictionary");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating data dictionary",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update an existing data dictionary
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DataDictionaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataDictionaryResponse>> UpdateDictionary(
        Guid id,
        [FromBody] UpdateDataDictionaryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var dictionary = await _dictionaryService.UpdateAsync(id, request, userId, cancellationToken);
            return Ok(dictionary);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data dictionary {DictionaryId} not found for update", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Dictionary Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update data dictionary {DictionaryId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Data Dictionary Update Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating data dictionary {DictionaryId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error updating data dictionary",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a data dictionary (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDictionary(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _dictionaryService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data dictionary {DictionaryId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "Data Dictionary Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data dictionary {DictionaryId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting data dictionary",
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
