using Apivia.Services.Linting.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using YamlDotNet.Serialization;

namespace Apivia.Services.Linting.Services;

/// <summary>
/// Service for linting API specifications using Spectral
/// </summary>
public class LintingService : ILintingService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<LintingService> _logger;
    private readonly IDeserializer _yamlDeserializer;
    private readonly ISerializer _yamlSerializer;

    public LintingService(
        ApiviaDbContext context,
        ILogger<LintingService> logger)
    {
        _context = context;
        _logger = logger;
        _yamlDeserializer = new DeserializerBuilder().Build();
        _yamlSerializer = new SerializerBuilder().Build();
    }

    public async Task<LintResultResponse> LintApiSpecAsync(
        LintApiSpecRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Get API spec
        var apiSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == request.ApiSpecId && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"API Spec {request.ApiSpecId} not found");

        // Determine format
        var format = apiSpec.Format.ToLower() switch
        {
            "yaml" or "yml" => ApiSpecFormat.OpenApiYaml,
            "json" => ApiSpecFormat.OpenApiJson,
            _ => ApiSpecFormat.OpenApiYaml
        };

        // Get ruleset if specified
        Ruleset? ruleset = null;
        if (request.RulesetId.HasValue)
        {
            ruleset = await _context.Rulesets
                .FirstOrDefaultAsync(r => r.Id == request.RulesetId.Value && r.IsActive, cancellationToken);
        }
        else
        {
            // Get default ruleset
            ruleset = await _context.Rulesets
                .FirstOrDefaultAsync(r => r.IsDefault && r.IsActive, cancellationToken);
        }

        // Run linting
        var issues = await RunLintingAsync(apiSpec.Content, format, ruleset);

        // Calculate counts
        var errorCount = issues.Count(i => i.Severity == LintSeverity.Error);
        var warningCount = issues.Count(i => i.Severity == LintSeverity.Warning);
        var infoCount = issues.Count(i => i.Severity == LintSeverity.Info);
        var hintCount = issues.Count(i => i.Severity == LintSeverity.Hint);

        // Determine status
        var status = errorCount > 0 ? LintStatus.Failed :
                    warningCount > 0 ? LintStatus.PassedWithWarnings :
                    LintStatus.Passed;

        stopwatch.Stop();

        // Save results if requested
        LintResult? savedResult = null;
        if (request.SaveResults)
        {
            savedResult = new LintResult
            {
                Id = Guid.NewGuid(),
                ApiSpecId = apiSpec.Id,
                RulesetId = ruleset?.Id,
                Status = status.ToString(),
                ErrorCount = errorCount,
                WarningCount = warningCount,
                InfoCount = infoCount,
                HintCount = hintCount,
                Issues = JsonSerializer.Serialize(issues),
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutedAt = DateTime.UtcNow,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.LintResults.Add(savedResult);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Linting completed for API Spec {ApiSpecId} with {ErrorCount} errors, {WarningCount} warnings",
                apiSpec.Id, errorCount, warningCount);
        }

        return new LintResultResponse
        {
            Id = savedResult?.Id,
            ApiSpecId = apiSpec.Id,
            ApiSpecName = apiSpec.Name,
            ApiSpecVersion = apiSpec.Version,
            ErrorCount = errorCount,
            WarningCount = warningCount,
            InfoCount = infoCount,
            HintCount = hintCount,
            Status = status,
            Issues = issues,
            ExecutedAt = savedResult?.ExecutedAt,
            ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
            RulesetName = ruleset?.Name
        };
    }

    public async Task<LintResultResponse> LintContentAsync(
        LintContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Get ruleset if specified
        Ruleset? ruleset = null;
        if (request.RulesetId.HasValue)
        {
            ruleset = await _context.Rulesets
                .FirstOrDefaultAsync(r => r.Id == request.RulesetId.Value && r.IsActive, cancellationToken);
        }

        // Run linting
        var issues = await RunLintingAsync(request.Content, request.Format, ruleset);

        // Calculate counts
        var errorCount = issues.Count(i => i.Severity == LintSeverity.Error);
        var warningCount = issues.Count(i => i.Severity == LintSeverity.Warning);
        var infoCount = issues.Count(i => i.Severity == LintSeverity.Info);
        var hintCount = issues.Count(i => i.Severity == LintSeverity.Hint);

        // Determine status
        var status = errorCount > 0 ? LintStatus.Failed :
                    warningCount > 0 ? LintStatus.PassedWithWarnings :
                    LintStatus.Passed;

        stopwatch.Stop();

        return new LintResultResponse
        {
            ErrorCount = errorCount,
            WarningCount = warningCount,
            InfoCount = infoCount,
            HintCount = hintCount,
            Status = status,
            Issues = issues,
            ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
            RulesetName = ruleset?.Name
        };
    }

    public async Task<PagedResponse<LintResultListItemResponse>> GetPagedAsync(
        LintResultQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LintResults
            .Include(l => l.ApiSpec)
            .Where(l => l.IsActive)
            .AsQueryable();

        // Apply filters
        if (queryParams.ApiSpecId.HasValue)
        {
            query = query.Where(l => l.ApiSpecId == queryParams.ApiSpecId.Value);
        }

        if (queryParams.Status.HasValue)
        {
            var statusString = queryParams.Status.Value.ToString();
            query = query.Where(l => l.Status == statusString);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(l => l.ExecutedAt)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(l => new LintResultListItemResponse
            {
                Id = l.Id,
                ApiSpecId = l.ApiSpecId,
                ApiSpecName = l.ApiSpec.Name,
                ApiSpecVersion = l.ApiSpec.Version,
                ErrorCount = l.ErrorCount,
                WarningCount = l.WarningCount,
                InfoCount = l.InfoCount,
                Status = Enum.Parse<LintStatus>(l.Status),
                ExecutedAt = l.ExecutedAt,
                RulesetName = l.Ruleset != null ? l.Ruleset.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<LintResultListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<LintResultResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lintResult = await _context.LintResults
            .Include(l => l.ApiSpec)
            .Include(l => l.Ruleset)
            .FirstOrDefaultAsync(l => l.Id == id && l.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Lint result {id} not found");

        var issues = string.IsNullOrEmpty(lintResult.Issues)
            ? new List<LintIssue>()
            : JsonSerializer.Deserialize<List<LintIssue>>(lintResult.Issues) ?? new List<LintIssue>();

        return new LintResultResponse
        {
            Id = lintResult.Id,
            ApiSpecId = lintResult.ApiSpecId,
            ApiSpecName = lintResult.ApiSpec.Name,
            ApiSpecVersion = lintResult.ApiSpec.Version,
            ErrorCount = lintResult.ErrorCount,
            WarningCount = lintResult.WarningCount,
            InfoCount = lintResult.InfoCount,
            HintCount = lintResult.HintCount,
            Status = Enum.Parse<LintStatus>(lintResult.Status),
            Issues = issues,
            ExecutedAt = lintResult.ExecutedAt,
            ExecutionTimeMs = lintResult.ExecutionTimeMs,
            RulesetName = lintResult.Ruleset?.Name
        };
    }

    public async Task<LintStatsResponse> GetStatsAsync(Guid apiSpecId, CancellationToken cancellationToken = default)
    {
        // Verify API spec exists
        var apiSpecExists = await _context.ApiSpecs
            .AnyAsync(a => a.Id == apiSpecId && a.IsActive, cancellationToken);

        if (!apiSpecExists)
        {
            throw new KeyNotFoundException($"API Spec {apiSpecId} not found");
        }

        var results = await _context.LintResults
            .Where(l => l.ApiSpecId == apiSpecId && l.IsActive)
            .OrderByDescending(l => l.ExecutedAt)
            .ToListAsync(cancellationToken);

        var totalRuns = results.Count;
        var passedRuns = results.Count(l => l.Status == LintStatus.Passed.ToString());
        var failedRuns = results.Count(l => l.Status == LintStatus.Failed.ToString());

        var lastRun = results.FirstOrDefault();

        // Get trend data (last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var trendData = results
            .Where(l => l.ExecutedAt >= thirtyDaysAgo)
            .GroupBy(l => l.ExecutedAt.Date)
            .Select(g => new LintTrendData
            {
                Date = g.Key,
                ErrorCount = g.Sum(l => l.ErrorCount),
                WarningCount = g.Sum(l => l.WarningCount),
                Status = Enum.Parse<LintStatus>(g.OrderByDescending(l => l.ExecutedAt).First().Status)
            })
            .OrderBy(t => t.Date)
            .ToList();

        return new LintStatsResponse
        {
            ApiSpecId = apiSpecId,
            TotalRuns = totalRuns,
            PassedRuns = passedRuns,
            FailedRuns = failedRuns,
            LastRunAt = lastRun?.ExecutedAt,
            LastRunStatus = lastRun != null ? Enum.Parse<LintStatus>(lastRun.Status) : null,
            LastRunErrorCount = lastRun?.ErrorCount,
            LastRunWarningCount = lastRun?.WarningCount,
            Trend = trendData
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lintResult = await _context.LintResults
            .FirstOrDefaultAsync(l => l.Id == id && l.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Lint result {id} not found");

        // Soft delete
        lintResult.IsActive = false;
        lintResult.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Lint result {LintResultId} deleted", id);
    }

    /// <summary>
    /// Run linting on content (simulated Spectral implementation)
    /// In production, this would execute: spectral lint <file> --ruleset <ruleset>
    /// </summary>
    private async Task<List<LintIssue>> RunLintingAsync(
        string content,
        ApiSpecFormat format,
        Ruleset? ruleset)
    {
        var issues = new List<LintIssue>();

        try
        {
            // Parse the content
            dynamic? spec = null;
            if (format == ApiSpecFormat.OpenApiYaml || format == ApiSpecFormat.AsyncApiYaml)
            {
                spec = _yamlDeserializer.Deserialize<dynamic>(content);
            }
            else
            {
                spec = JsonSerializer.Deserialize<dynamic>(content);
            }

            if (spec == null)
            {
                issues.Add(new LintIssue
                {
                    Code = "parser-error",
                    Message = "Failed to parse specification",
                    Severity = LintSeverity.Error,
                    Path = "$",
                    Range = new LintIssueRange
                    {
                        Start = new LintPosition { Line = 1, Character = 0 },
                        End = new LintPosition { Line = 1, Character = 0 }
                    }
                });
                return issues;
            }

            // Run basic validation rules (simulated Spectral rules)
            issues.AddRange(await ValidateBasicStructureAsync(content, format));
            issues.AddRange(await ValidateOpenAPIRulesAsync(content, format));

            // In production, you would execute Spectral CLI:
            // var processInfo = new ProcessStartInfo
            // {
            //     FileName = "npx",
            //     Arguments = $"@stoplight/spectral-cli lint {specPath} --ruleset {rulesetPath} --format json",
            //     UseShellExecute = false,
            //     RedirectStandardOutput = true,
            //     RedirectStandardError = true
            // };
            // var process = Process.Start(processInfo);
            // var output = await process.StandardOutput.ReadToEndAsync();
            // var spectralResults = JsonSerializer.Deserialize<List<SpectralIssue>>(output);
            // Convert spectralResults to LintIssue format
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during linting");
            issues.Add(new LintIssue
            {
                Code = "linting-error",
                Message = $"Linting failed: {ex.Message}",
                Severity = LintSeverity.Error,
                Path = "$"
            });
        }

        return issues;
    }

    /// <summary>
    /// Validate basic structure (simulated rules)
    /// </summary>
    private async Task<List<LintIssue>> ValidateBasicStructureAsync(string content, ApiSpecFormat format)
    {
        var issues = new List<LintIssue>();

        // Check for required fields
        if (!content.Contains("openapi") && !content.Contains("swagger"))
        {
            issues.Add(new LintIssue
            {
                Code = "openapi-tags-alphabetical",
                Message = "OpenAPI object must have 'openapi' or 'swagger' field",
                Severity = LintSeverity.Error,
                Path = "$"
            });
        }

        if (!content.Contains("info"))
        {
            issues.Add(new LintIssue
            {
                Code = "info-required",
                Message = "Info object is required",
                Severity = LintSeverity.Error,
                Path = "$"
            });
        }

        if (!content.Contains("paths"))
        {
            issues.Add(new LintIssue
            {
                Code = "paths-required",
                Message = "Paths object is required",
                Severity = LintSeverity.Error,
                Path = "$"
            });
        }

        await Task.CompletedTask;
        return issues;
    }

    /// <summary>
    /// Validate OpenAPI specific rules (simulated)
    /// </summary>
    private async Task<List<LintIssue>> ValidateOpenAPIRulesAsync(string content, ApiSpecFormat format)
    {
        var issues = new List<LintIssue>();

        // Check for description
        if (!content.Contains("description"))
        {
            issues.Add(new LintIssue
            {
                Code = "info-description",
                Message = "Info object should contain description",
                Severity = LintSeverity.Warning,
                Path = "$.info"
            });
        }

        // Check for contact info
        if (!content.Contains("contact"))
        {
            issues.Add(new LintIssue
            {
                Code = "info-contact",
                Message = "Info object should contain contact information",
                Severity = LintSeverity.Info,
                Path = "$.info"
            });
        }

        // Check for license
        if (!content.Contains("license"))
        {
            issues.Add(new LintIssue
            {
                Code = "info-license",
                Message = "Info object should contain license information",
                Severity = LintSeverity.Info,
                Path = "$.info"
            });
        }

        await Task.CompletedTask;
        return issues;
    }
}
