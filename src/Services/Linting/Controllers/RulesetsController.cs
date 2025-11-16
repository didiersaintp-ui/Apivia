using Apivia.Services.Linting.DTOs;
using Apivia.Services.Linting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.Linting.Controllers;

/// <summary>
/// Controller for managing linting rulesets
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RulesetsController : ControllerBase
{
    private readonly IRulesetService _rulesetService;
    private readonly ILogger<RulesetsController> _logger;

    public RulesetsController(
        IRulesetService rulesetService,
        ILogger<RulesetsController> logger)
    {
        _rulesetService = rulesetService;
        _logger = logger;
    }

    /// <summary>
    /// Get all rulesets
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RulesetListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RulesetListItemResponse>>> GetRulesets(
        CancellationToken cancellationToken)
    {
        try
        {
            var rulesets = await _rulesetService.GetAllAsync(cancellationToken);
            return Ok(rulesets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rulesets");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving rulesets",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific ruleset by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RulesetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RulesetResponse>> GetRuleset(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var ruleset = await _rulesetService.GetByIdAsync(id, cancellationToken);
            return Ok(ruleset);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ruleset {RulesetId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Ruleset Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ruleset {RulesetId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving ruleset",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get the default ruleset
    /// </summary>
    [HttpGet("default")]
    [ProducesResponseType(typeof(RulesetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RulesetResponse>> GetDefaultRuleset(
        CancellationToken cancellationToken)
    {
        try
        {
            var ruleset = await _rulesetService.GetDefaultAsync(cancellationToken);
            if (ruleset == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Default Ruleset Not Found",
                    Detail = "No default ruleset is configured",
                    Status = StatusCodes.Status404NotFound
                });
            }
            return Ok(ruleset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default ruleset");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving default ruleset",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new ruleset
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RulesetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RulesetResponse>> CreateRuleset(
        [FromBody] CreateRulesetRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var ruleset = await _rulesetService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetRuleset),
                new { id = ruleset.Id },
                ruleset);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create ruleset");
            return BadRequest(new ProblemDetails
            {
                Title = "Ruleset Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ruleset");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating ruleset",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update a ruleset
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RulesetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RulesetResponse>> UpdateRuleset(
        Guid id,
        [FromBody] UpdateRulesetRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ruleset = await _rulesetService.UpdateAsync(id, request, cancellationToken);
            return Ok(ruleset);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ruleset {RulesetId} not found for update", id);
            return NotFound(new ProblemDetails
            {
                Title = "Ruleset Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update ruleset {RulesetId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Update Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ruleset {RulesetId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error updating ruleset",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Set a ruleset as default
    /// </summary>
    [HttpPost("{id}/set-default")]
    [ProducesResponseType(typeof(RulesetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RulesetResponse>> SetDefaultRuleset(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var ruleset = await _rulesetService.SetDefaultAsync(id, cancellationToken);
            return Ok(ruleset);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ruleset {RulesetId} not found for set default", id);
            return NotFound(new ProblemDetails
            {
                Title = "Ruleset Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting ruleset {RulesetId} as default", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error setting default ruleset",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a ruleset
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRuleset(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _rulesetService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ruleset {RulesetId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "Ruleset Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete ruleset {RulesetId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Delete Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ruleset {RulesetId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting ruleset",
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
