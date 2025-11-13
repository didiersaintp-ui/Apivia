using Apivia.Services.ApiDesign.DTOs;
using Apivia.Services.ApiDesign.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.ApiDesign.Controllers;

/// <summary>
/// Controller for managing projects
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of projects
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProjectListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProjectListItemResponse>>> GetProjects(
        [FromQuery] ProjectQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _projectService.GetPagedAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving projects",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponse>> GetProject(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var project = await _projectService.GetByIdAsync(id, cancellationToken);
            return Ok(project);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Project Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving project",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectResponse>> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var project = await _projectService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetProject),
                new { id = project.Id },
                project);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create project");
            return BadRequest(new ProblemDetails
            {
                Title = "Project Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating project",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponse>> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var project = await _projectService.UpdateAsync(id, request, userId, cancellationToken);
            return Ok(project);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found for update", id);
            return NotFound(new ProblemDetails
            {
                Title = "Project Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update project {ProjectId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Project Update Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error updating project",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a project (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _projectService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "Project Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting project",
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
