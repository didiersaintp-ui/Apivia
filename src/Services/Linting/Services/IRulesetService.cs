using Apivia.Services.Linting.DTOs;

namespace Apivia.Services.Linting.Services;

/// <summary>
/// Interface for managing custom linting rulesets
/// </summary>
public interface IRulesetService
{
    /// <summary>
    /// Get all rulesets
    /// </summary>
    Task<List<RulesetListItemResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific ruleset by ID
    /// </summary>
    Task<RulesetResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new ruleset
    /// </summary>
    Task<RulesetResponse> CreateAsync(
        CreateRulesetRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing ruleset
    /// </summary>
    Task<RulesetResponse> UpdateAsync(
        Guid id,
        UpdateRulesetRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a ruleset
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the default ruleset
    /// </summary>
    Task<RulesetResponse?> GetDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set a ruleset as default
    /// </summary>
    Task<RulesetResponse> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default);
}
