using Apivia.Services.ApiDesign.DTOs;
using Apivia.Services.ApiDesign.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.ApiDesign.Controllers;

/// <summary>
/// Controller for managing API specifications
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiSpecsController : ControllerBase
{
    private readonly IApiSpecService _apiSpecService;
    private readonly IValidationService _validationService;
    private readonly IImportExportService _importExportService;
    private readonly ILogger<ApiSpecsController> _logger;

    public ApiSpecsController(
        IApiSpecService apiSpecService,
        IValidationService validationService,
        IImportExportService importExportService,
        ILogger<ApiSpecsController> logger)
    {
        _apiSpecService = apiSpecService;
        _validationService = validationService;
        _importExportService = importExportService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of API specifications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ApiSpecListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ApiSpecListItemResponse>>> GetApiSpecs(
        [FromQuery] ApiSpecQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiSpecService.GetPagedAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API specs");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving API specifications",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific API specification by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiSpecResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSpecResponse>> GetApiSpec(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var apiSpec = await _apiSpecService.GetByIdAsync(id, cancellationToken);
            return Ok(apiSpec);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API spec {ApiSpecId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving API specification",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new API specification
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiSpecResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiSpecResponse>> CreateApiSpec(
        [FromBody] CreateApiSpecRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var apiSpec = await _apiSpecService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetApiSpec),
                new { id = apiSpec.Id },
                apiSpec);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create API spec");
            return BadRequest(new ProblemDetails
            {
                Title = "API Spec Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API spec");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating API specification",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update an existing API specification
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiSpecResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSpecResponse>> UpdateApiSpec(
        Guid id,
        [FromBody] UpdateApiSpecRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var apiSpec = await _apiSpecService.UpdateAsync(id, request, userId, cancellationToken);
            return Ok(apiSpec);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found for update", id);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update API spec {ApiSpecId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "API Spec Update Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating API spec {ApiSpecId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error updating API specification",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete an API specification (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteApiSpec(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiSpecService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete API spec {ApiSpecId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Deletion Not Allowed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting API spec {ApiSpecId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting API specification",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Validate an API specification without saving
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ValidationResponse>> ValidateApiSpec(
        [FromBody] ValidateApiSpecRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _validationService.ValidateAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API spec");
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Publish an API specification
    /// </summary>
    [HttpPost("{id}/publish")]
    [ProducesResponseType(typeof(ApiSpecResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSpecResponse>> PublishApiSpec(
        Guid id,
        [FromBody] PublishApiSpecRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var apiSpec = await _apiSpecService.PublishAsync(id, request, userId, cancellationToken);
            return Ok(apiSpec);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found for publishing", id);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot publish API spec {ApiSpecId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Publishing Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing API spec {ApiSpecId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error publishing API specification",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new version of an API specification
    /// </summary>
    [HttpPost("{id}/versions")]
    [ProducesResponseType(typeof(ApiSpecResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSpecResponse>> CreateVersion(
        Guid id,
        [FromBody] CreateVersionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var apiSpec = await _apiSpecService.CreateVersionAsync(id, request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetApiSpec),
                new { id = apiSpec.Id },
                apiSpec);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec {ApiSpecId} not found for versioning", id);
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating version for API spec {ApiSpecId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating version",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Import an API specification from various formats
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportResponse>> ImportApiSpec(
        [FromBody] ImportApiSpecRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _importExportService.ImportAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetApiSpec),
                new { id = result.ApiSpecId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to import API spec");
            return BadRequest(new ProblemDetails
            {
                Title = "Import Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing API spec");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error importing API specification",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Export an API specification to various formats
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportApiSpec(
        [FromBody] ExportApiSpecRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _importExportService.ExportAsync(request, cancellationToken);

            return File(
                System.Text.Encoding.UTF8.GetBytes(result.Content),
                result.ContentType,
                result.FileName);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API Spec not found for export");
            return NotFound(new ProblemDetails
            {
                Title = "API Spec Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to export API spec");
            return BadRequest(new ProblemDetails
            {
                Title = "Export Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting API spec");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error exporting API specification",
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
