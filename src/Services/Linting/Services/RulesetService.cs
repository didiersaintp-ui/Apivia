using Apivia.Services.Linting.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apivia.Services.Linting.Services;

/// <summary>
/// Service for managing custom linting rulesets
/// </summary>
public class RulesetService : IRulesetService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<RulesetService> _logger;

    public RulesetService(
        ApiviaDbContext context,
        ILogger<RulesetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<RulesetListItemResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rulesets = await _context.Rulesets
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.IsDefault)
            .ThenByDescending(r => r.IsBuiltIn)
            .ThenBy(r => r.Name)
            .Select(r => new RulesetListItemResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsDefault = r.IsDefault,
                IsBuiltIn = r.IsBuiltIn,
                UsageCount = r.LintResults.Count(l => l.IsActive)
            })
            .ToListAsync(cancellationToken);

        return rulesets;
    }

    public async Task<RulesetResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ruleset = await _context.Rulesets
            .Include(r => r.LintResults)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Ruleset {id} not found");

        return new RulesetResponse
        {
            Id = ruleset.Id,
            Name = ruleset.Name,
            Description = ruleset.Description,
            Rules = ruleset.Rules,
            IsDefault = ruleset.IsDefault,
            IsBuiltIn = ruleset.IsBuiltIn,
            CreatedAt = ruleset.CreatedAt,
            UsageCount = ruleset.LintResults.Count(l => l.IsActive)
        };
    }

    public async Task<RulesetResponse> CreateAsync(
        CreateRulesetRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate name
        var nameExists = await _context.Rulesets
            .AnyAsync(r => r.Name == request.Name && r.IsActive, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException($"A ruleset with name '{request.Name}' already exists");
        }

        // If this should be default, unset other defaults
        if (request.IsDefault)
        {
            await UnsetAllDefaultsAsync(cancellationToken);
        }

        // Create ruleset
        var ruleset = new Ruleset
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Rules = request.Rules,
            IsDefault = request.IsDefault,
            IsBuiltIn = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsActive = true
        };

        _context.Rulesets.Add(ruleset);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ruleset {RulesetId} '{Name}' created", ruleset.Id, ruleset.Name);

        return new RulesetResponse
        {
            Id = ruleset.Id,
            Name = ruleset.Name,
            Description = ruleset.Description,
            Rules = ruleset.Rules,
            IsDefault = ruleset.IsDefault,
            IsBuiltIn = ruleset.IsBuiltIn,
            CreatedAt = ruleset.CreatedAt,
            UsageCount = 0
        };
    }

    public async Task<RulesetResponse> UpdateAsync(
        Guid id,
        UpdateRulesetRequest request,
        CancellationToken cancellationToken = default)
    {
        var ruleset = await _context.Rulesets
            .Include(r => r.LintResults)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Ruleset {id} not found");

        if (ruleset.IsBuiltIn)
        {
            throw new InvalidOperationException("Cannot modify built-in rulesets");
        }

        // Check for duplicate name if name is being changed
        if (!string.IsNullOrEmpty(request.Name) && request.Name != ruleset.Name)
        {
            var nameExists = await _context.Rulesets
                .AnyAsync(r => r.Name == request.Name && r.Id != id && r.IsActive, cancellationToken);

            if (nameExists)
            {
                throw new InvalidOperationException($"A ruleset with name '{request.Name}' already exists");
            }

            ruleset.Name = request.Name;
        }

        if (request.Description != null)
            ruleset.Description = request.Description;

        if (!string.IsNullOrEmpty(request.Rules))
            ruleset.Rules = request.Rules;

        if (request.IsDefault.HasValue && request.IsDefault.Value && !ruleset.IsDefault)
        {
            await UnsetAllDefaultsAsync(cancellationToken);
            ruleset.IsDefault = true;
        }
        else if (request.IsDefault.HasValue && !request.IsDefault.Value && ruleset.IsDefault)
        {
            ruleset.IsDefault = false;
        }

        ruleset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ruleset {RulesetId} '{Name}' updated", ruleset.Id, ruleset.Name);

        return new RulesetResponse
        {
            Id = ruleset.Id,
            Name = ruleset.Name,
            Description = ruleset.Description,
            Rules = ruleset.Rules,
            IsDefault = ruleset.IsDefault,
            IsBuiltIn = ruleset.IsBuiltIn,
            CreatedAt = ruleset.CreatedAt,
            UsageCount = ruleset.LintResults.Count(l => l.IsActive)
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ruleset = await _context.Rulesets
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Ruleset {id} not found");

        if (ruleset.IsBuiltIn)
        {
            throw new InvalidOperationException("Cannot delete built-in rulesets");
        }

        if (ruleset.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete the default ruleset. Please set another ruleset as default first.");
        }

        // Soft delete
        ruleset.IsActive = false;
        ruleset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ruleset {RulesetId} '{Name}' deleted", id, ruleset.Name);
    }

    public async Task<RulesetResponse?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        var ruleset = await _context.Rulesets
            .Include(r => r.LintResults)
            .FirstOrDefaultAsync(r => r.IsDefault && r.IsActive, cancellationToken);

        if (ruleset == null)
            return null;

        return new RulesetResponse
        {
            Id = ruleset.Id,
            Name = ruleset.Name,
            Description = ruleset.Description,
            Rules = ruleset.Rules,
            IsDefault = ruleset.IsDefault,
            IsBuiltIn = ruleset.IsBuiltIn,
            CreatedAt = ruleset.CreatedAt,
            UsageCount = ruleset.LintResults.Count(l => l.IsActive)
        };
    }

    public async Task<RulesetResponse> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ruleset = await _context.Rulesets
            .Include(r => r.LintResults)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Ruleset {id} not found");

        // Unset all other defaults
        await UnsetAllDefaultsAsync(cancellationToken);

        // Set this one as default
        ruleset.IsDefault = true;
        ruleset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ruleset {RulesetId} '{Name}' set as default", id, ruleset.Name);

        return new RulesetResponse
        {
            Id = ruleset.Id,
            Name = ruleset.Name,
            Description = ruleset.Description,
            Rules = ruleset.Rules,
            IsDefault = ruleset.IsDefault,
            IsBuiltIn = ruleset.IsBuiltIn,
            CreatedAt = ruleset.CreatedAt,
            UsageCount = ruleset.LintResults.Count(l => l.IsActive)
        };
    }

    /// <summary>
    /// Unset all default flags
    /// </summary>
    private async Task UnsetAllDefaultsAsync(CancellationToken cancellationToken)
    {
        var defaults = await _context.Rulesets
            .Where(r => r.IsDefault && r.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var ruleset in defaults)
        {
            ruleset.IsDefault = false;
            ruleset.UpdatedAt = DateTime.UtcNow;
        }

        if (defaults.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
