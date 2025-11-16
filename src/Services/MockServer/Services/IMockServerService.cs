using Apivia.Services.MockServer.DTOs;

namespace Apivia.Services.MockServer.Services;

/// <summary>
/// Interface for managing mock servers
/// </summary>
public interface IMockServerService
{
    /// <summary>
    /// Get paginated list of mock servers
    /// </summary>
    Task<PagedResponse<MockServerListItemResponse>> GetPagedAsync(
        MockServerQueryParams queryParams,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mock server by ID
    /// </summary>
    Task<MockServerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create and start a new mock server
    /// </summary>
    Task<MockServerResponse> CreateAsync(
        CreateMockServerRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update mock server configuration (requires restart)
    /// </summary>
    Task<MockServerResponse> UpdateAsync(
        Guid id,
        UpdateMockServerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Start a stopped mock server
    /// </summary>
    Task<MockServerResponse> StartAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop a running mock server
    /// </summary>
    Task<MockServerResponse> StopAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a mock server
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mock server logs
    /// </summary>
    Task<MockServerLogsResponse> GetLogsAsync(
        Guid id,
        int maxLines = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mock server metrics
    /// </summary>
    Task<MockServerMetrics> GetMetricsAsync(Guid id, CancellationToken cancellationToken = default);
}
