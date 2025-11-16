using Apivia.Services.Linting.DTOs;
using Apivia.Services.Linting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.Linting.Controllers;

/// <summary>
/// Controller for linting API specifications
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LintingController : ControllerBase
{
    private readonly ILintingService _lintingService;
    private readonly ILogger<LintingController> _logger;

    public LintingController(
        ILintingService lintingService,
        ILogger<LintingController> logger)
    {
        _lintingService = lintingService;
        _logger = logger;
    }

    /// <summary>
    /// Lint an API specification by ID
    /// </summary>
    [HttpPost("api-spec")]
    [ProducesResponseType(typeof(LintResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LintResultResponse>> LintApiSpec(
        [FromBody] LintApiSpecRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var result = await _lintingService.LintApiSpecAsync(request, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found for linting", request.ApiSpecId);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linting API Spec {ApiSpecId}", request.ApiSpecId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error linting API specification",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Lint content directly without saving
    /// </summary>
    [HttpPost("content")]
    [ProducesResponseType(typeof(LintResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LintResultResponse>> LintContent(
        [FromBody] LintContentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _lintingService.LintContentAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linting content");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error linting content",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get paginated list of lint results
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<LintResultListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<LintResultListItemResponse>>> GetLintResults(
        [FromQuery] LintResultQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _lintingService.GetPagedAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lint results");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving lint results",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific lint result by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LintResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LintResultResponse>> GetLintResult(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _lintingService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Lint result {LintResultId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Lint Result Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lint result {LintResultId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving lint result",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get lint statistics for an API spec
    /// </summary>
    [HttpGet("stats/{apiSpecId}")]
    [ProducesResponseType(typeof(LintStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LintStatsResponse>> GetStats(
        Guid apiSpecId,
        CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _lintingService.GetStatsAsync(apiSpecId, cancellationToken);
            return Ok(stats);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found for stats", apiSpecId);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lint stats for API Spec {ApiSpecId}", apiSpecId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving statistics",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a lint result
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLintResult(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _lintingService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Lint result {LintResultId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "Lint Result Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lint result {LintResultId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting lint result",
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
