using Apivia.Services.Governance.DTOs;
using Apivia.Services.Governance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.Governance.Controllers;

/// <summary>
/// Controller for managing impact analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImpactAnalysisController : ControllerBase
{
    private readonly IImpactAnalysisService _impactAnalysisService;
    private readonly ILogger<ImpactAnalysisController> _logger;

    public ImpactAnalysisController(
        IImpactAnalysisService impactAnalysisService,
        ILogger<ImpactAnalysisController> logger)
    {
        _impactAnalysisService = impactAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of impact analyses
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ImpactAnalysisListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ImpactAnalysisListItemResponse>>> GetImpactAnalyses(
        [FromQuery] ImpactAnalysisQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _impactAnalysisService.GetPagedAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving impact analyses");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving impact analyses",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific impact analysis by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ImpactAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImpactAnalysisResponse>> GetImpactAnalysis(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var analysis = await _impactAnalysisService.GetByIdAsync(id, cancellationToken);
            return Ok(analysis);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Impact Analysis {AnalysisId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Impact Analysis Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving impact analysis {AnalysisId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving impact analysis",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new impact analysis
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ImpactAnalysisResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImpactAnalysisResponse>> CreateImpactAnalysis(
        [FromBody] CreateImpactAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var analysis = await _impactAnalysisService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetImpactAnalysis),
                new { id = analysis.Id },
                analysis);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create impact analysis");
            return BadRequest(new ProblemDetails
            {
                Title = "Impact Analysis Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating impact analysis");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating impact analysis",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Resolve an impact analysis
    /// </summary>
    [HttpPost("{id}/resolve")]
    [ProducesResponseType(typeof(ImpactAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImpactAnalysisResponse>> ResolveImpactAnalysis(
        Guid id,
        [FromBody] ResolveImpactAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var analysis = await _impactAnalysisService.ResolveAsync(id, request, userId, cancellationToken);
            return Ok(analysis);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Impact Analysis {AnalysisId} not found for resolution", id);
            return NotFound(new ProblemDetails
            {
                Title = "Impact Analysis Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to resolve impact analysis {AnalysisId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Resolution Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving impact analysis {AnalysisId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error resolving impact analysis",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Preview impact without creating analysis (dry-run)
    /// Performs real analysis but does not persist results to database
    /// </summary>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(ImpactPreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImpactPreviewResponse>> PreviewImpact(
        [FromBody] PreviewImpactRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _impactAnalysisService.PreviewImpactAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to preview impact");
            return BadRequest(new ProblemDetails
            {
                Title = "Preview Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing impact");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error previewing impact",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get statistics for impact analyses
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ImpactAnalysisStatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ImpactAnalysisStatsResponse>> GetStats(
        [FromQuery] Guid? dictionaryId,
        CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _impactAnalysisService.GetStatsAsync(dictionaryId, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving impact analysis stats");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving statistics",
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
