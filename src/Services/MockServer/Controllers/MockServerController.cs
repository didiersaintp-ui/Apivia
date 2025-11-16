using Apivia.Services.MockServer.DTOs;
using Apivia.Services.MockServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apivia.Services.MockServer.Controllers;

/// <summary>
/// Controller for managing mock servers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MockServerController : ControllerBase
{
    private readonly IMockServerService _mockServerService;
    private readonly ILogger<MockServerController> _logger;

    public MockServerController(
        IMockServerService mockServerService,
        ILogger<MockServerController> logger)
    {
        _mockServerService = mockServerService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of mock servers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MockServerListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<MockServerListItemResponse>>> GetMockServers(
        [FromQuery] MockServerQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mockServerService.GetPagedAsync(queryParams, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving mock servers");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving mock servers",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get a specific mock server by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MockServerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MockServerResponse>> GetMockServer(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var mockServer = await _mockServerService.GetByIdAsync(id, cancellationToken);
            return Ok(mockServer);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Mock server {MockServerId} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Mock Server Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving mock server {MockServerId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving mock server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create and start a new mock server
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MockServerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MockServerResponse>> CreateMockServer(
        [FromBody] CreateMockServerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var mockServer = await _mockServerService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetMockServer),
                new { id = mockServer.Id },
                mockServer);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create mock server");
            return BadRequest(new ProblemDetails
            {
                Title = "Mock Server Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating mock server");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error creating mock server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update mock server configuration
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MockServerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MockServerResponse>> UpdateMockServer(
        Guid id,
        [FromBody] UpdateMockServerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var mockServer = await _mockServerService.UpdateAsync(id, request, cancellationToken);
            return Ok(mockServer);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Mock server {MockServerId} not found for update", id);
            return NotFound(new ProblemDetails
            {
                Title = "Mock Server Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mock server {MockServerId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error updating mock server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Start a stopped mock server
    /// </summary>
    [HttpPost("{id}/start")]
    [ProducesResponseType(typeof(MockServerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MockServerResponse>> StartMockServer(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var mockServer = await _mockServerService.StartAsync(id, cancellationToken);
            return Ok(mockServer);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Mock server {MockServerId} not found for start", id);
            return NotFound(new ProblemDetails
            {
                Title = "Mock Server Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to start mock server {MockServerId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Start Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting mock server {MockServerId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error starting mock server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Stop a running mock server
    /// </summary>
    [HttpPost("{id}/stop")]
    [ProducesResponseType(typeof(MockServerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MockServerResponse>> StopMockServer(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var mockServer = await _mockServerService.StopAsync(id, cancellationToken);
            return Ok(mockServer);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Mock server {MockServerId} not found for stop", id);
            return NotFound(new ProblemDetails
            {
                Title = "Mock Server Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to stop mock server {MockServerId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Stop Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping mock server {MockServerId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error stopping mock server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete a mock server
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMockServer(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mockServerService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Mock server {MockServerId} not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Title = "Mock Server Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting mock server {MockServerId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error deleting mock server",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get mock server logs
    /// </summary>
    [HttpGet("{id}/logs")]
    [ProducesResponseType(typeof(MockServerLogsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MockServerLogsResponse>> GetLogs(
        Guid id,
        [FromQuery] int maxLines = 100,
        CancellationToken cancellationToken)
    {
        try
        {
            var logs = await _mockServerService.GetLogsAsync(id, maxLines, cancellationToken);
            return Ok(logs);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Mock server {MockServerId} not found for logs", id);
            return NotFound(new ProblemDetails
            {
                Title = "Mock Server Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for mock server {MockServerId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving logs",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get mock server metrics
    /// </summary>
    [HttpGet("{id}/metrics")]
    [ProducesResponseType(typeof(MockServerMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MockServerMetrics>> GetMetrics(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var metrics = await _mockServerService.GetMetricsAsync(id, cancellationToken);
            return Ok(metrics);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Mock server {MockServerId} not found for metrics", id);
            return NotFound(new ProblemDetails
            {
                Title = "Mock Server Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics for mock server {MockServerId}", id);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error retrieving metrics",
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
