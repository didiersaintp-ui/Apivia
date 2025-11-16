using Apivia.Services.Governance.DTOs;
using Apivia.Services.Governance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.Governance.Controllers;

/// <summary>
/// Controller for managing linkage between API specs and data dictionary
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LinkageController : ControllerBase
{
    private readonly ILinkageService _linkageService;
    private readonly ILogger<LinkageController> _logger;

    public LinkageController(ILinkageService linkageService, ILogger<LinkageController> logger)
    {
        _linkageService = linkageService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of links
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<LinkListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<LinkListItemResponse>>> GetLinks(
        [FromQuery] LinkQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _linkageService.GetLinksAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving links");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving links",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific link by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LinkResponse>> GetLink(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _linkageService.GetLinkByIdAsync(id, cancellationToken);
            return Ok(link);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Link {LinkId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Link Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving link {LinkId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving link",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new link
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LinkResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LinkResponse>> CreateLink(
        [FromBody] CreateLinkRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var link = await _linkageService.CreateLinkAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetLink),
                new { id = link.Id },
                link);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create link");
            return BadRequest(new ProblemDetails
            {
                Title = "Link Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating link");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating link",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Bulk create links
    /// </summary>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BulkOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkOperationResponse>> BulkCreateLinks(
        [FromBody] BulkCreateLinksRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var result = await _linkageService.BulkCreateLinksAsync(request, userId, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to bulk create links");
            return BadRequest(new ProblemDetails
            {
                Title = "Bulk Create Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk creating links");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error bulk creating links",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a link
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLink(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _linkageService.DeleteLinkAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Link {LinkId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "Link Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting link {LinkId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting link",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get all links for an API spec
    /// </summary>
    [HttpGet("api-spec/{apiSpecId}")]
    [ProducesResponseType(typeof(ApiSpecLinksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSpecLinksResponse>> GetApiSpecLinks(
        Guid apiSpecId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _linkageService.GetApiSpecLinksAsync(apiSpecId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found", apiSpecId);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving links for API Spec {ApiSpecId}", apiSpecId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving API spec links",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get all links for a data entity
    /// </summary>
    [HttpGet("entity/{dataEntityId}")]
    [ProducesResponseType(typeof(DataEntityLinksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataEntityLinksResponse>> GetDataEntityLinks(
        Guid dataEntityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _linkageService.GetDataEntityLinksAsync(dataEntityId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Data Entity {DataEntityId} not found", dataEntityId);
            return NotFound(new ProblemDetails
            {
                Title = "Data Entity Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving links for Data Entity {DataEntityId}", dataEntityId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving data entity links",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Analyze API spec and suggest potential links
    /// </summary>
    [HttpPost("analyze/{apiSpecId}")]
    [ProducesResponseType(typeof(LinkageAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LinkageAnalysisResponse>> AnalyzeLinkage(
        Guid apiSpecId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _linkageService.AnalyzeLinkageAsync(apiSpecId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found for analysis", apiSpecId);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing linkage for API Spec {ApiSpecId}", apiSpecId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error analyzing linkage",
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
