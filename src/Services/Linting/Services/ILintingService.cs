using Apivia.Services.Linting.DTOs;

namespace Apivia.Services.Linting.Services;

/// <summary>
/// Interface for linting API specifications
/// </summary>
public interface ILintingService
{
    /// <summary>
    /// Lint an API specification by ID
    /// </summary>
    Task<LintResultResponse> LintApiSpecAsync(
        LintApiSpecRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lint content directly without saving
    /// </summary>
    Task<LintResultResponse> LintContentAsync(
        LintContentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated list of lint results
    /// </summary>
    Task<PagedResponse<LintResultListItemResponse>> GetPagedAsync(
        LintResultQueryParams queryParams,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific lint result by ID
    /// </summary>
    Task<LintResultResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get lint statistics for an API spec
    /// </summary>
    Task<LintStatsResponse> GetStatsAsync(Guid apiSpecId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a lint result
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
