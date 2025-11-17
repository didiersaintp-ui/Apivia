using Apivia.Services.Linting.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;
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
    /// Run linting on content using Spectral CLI (production implementation)
    /// Executes: npx @stoplight/spectral-cli lint <file> --ruleset <ruleset> --format json
    /// </summary>
    private async Task<List<LintIssue>> RunLintingAsync(
        string content,
        ApiSpecFormat format,
        Ruleset? ruleset)
    {
        var issues = new List<LintIssue>();
        string? specFilePath = null;
        string? rulesetFilePath = null;

        try
        {
            // Write spec content to temporary file
            var fileExtension = format == ApiSpecFormat.OpenApiYaml || format == ApiSpecFormat.AsyncApiYaml ? ".yaml" : ".json";
            specFilePath = Path.Combine(Path.GetTempPath(), $"spec_{Guid.NewGuid()}{fileExtension}");
            await File.WriteAllTextAsync(specFilePath, content);

            // Prepare Spectral arguments
            var arguments = $"@stoplight/spectral-cli lint \"{specFilePath}\" --format json";

            // Write custom ruleset to temporary file if provided
            if (ruleset != null && !string.IsNullOrEmpty(ruleset.Rules))
            {
                rulesetFilePath = Path.Combine(Path.GetTempPath(), $"ruleset_{Guid.NewGuid()}.yaml");
                await File.WriteAllTextAsync(rulesetFilePath, ruleset.Rules);
                arguments += $" --ruleset \"{rulesetFilePath}\"";
            }

            // Execute Spectral
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "npx",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetTempPath()
            };

            using var process = new Process { StartInfo = processStartInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for process to complete (max 30 seconds)
            if (!process.WaitForExit(30000))
            {
                process.Kill(true);
                throw new TimeoutException("Spectral linting timed out after 30 seconds");
            }

            var output = outputBuilder.ToString();
            var errors = errorBuilder.ToString();

            // Log Spectral execution
            _logger.LogInformation("Spectral exit code: {ExitCode}", process.ExitCode);
            if (!string.IsNullOrEmpty(errors))
            {
                _logger.LogWarning("Spectral stderr: {Errors}", errors);
            }

            // Parse Spectral JSON output
            if (!string.IsNullOrEmpty(output))
            {
                try
                {
                    var spectralIssues = JsonSerializer.Deserialize<List<SpectralIssue>>(output);
                    if (spectralIssues != null)
                    {
                        foreach (var spectralIssue in spectralIssues)
                        {
                            issues.Add(new LintIssue
                            {
                                Code = spectralIssue.Code ?? "unknown",
                                Message = spectralIssue.Message ?? "No message",
                                Severity = ConvertSpectralSeverity(spectralIssue.Severity),
                                Path = spectralIssue.Path != null ? string.Join(".", spectralIssue.Path) : "$",
                                Range = spectralIssue.Range != null ? new LintIssueRange
                                {
                                    Start = new LintPosition
                                    {
                                        Line = spectralIssue.Range.Start?.Line ?? 0,
                                        Character = spectralIssue.Range.Start?.Character ?? 0
                                    },
                                    End = new LintPosition
                                    {
                                        Line = spectralIssue.Range.End?.Line ?? 0,
                                        Character = spectralIssue.Range.End?.Character ?? 0
                                    }
                                } : null
                            });
                        }

                        _logger.LogInformation("Spectral found {IssueCount} issues", issues.Count);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse Spectral output: {Output}", output);
                    issues.Add(new LintIssue
                    {
                        Code = "spectral-parse-error",
                        Message = $"Failed to parse Spectral output: {ex.Message}",
                        Severity = LintSeverity.Error,
                        Path = "$"
                    });
                }
            }

            // If no issues found and process exited successfully, the spec is valid
            if (issues.Count == 0 && process.ExitCode == 0)
            {
                _logger.LogInformation("Spectral validation passed with no issues");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Spectral linting");
            issues.Add(new LintIssue
            {
                Code = "linting-error",
                Message = $"Linting failed: {ex.Message}",
                Severity = LintSeverity.Error,
                Path = "$"
            });
        }
        finally
        {
            // Clean up temporary files
            try
            {
                if (specFilePath != null && File.Exists(specFilePath))
                {
                    File.Delete(specFilePath);
                }
                if (rulesetFilePath != null && File.Exists(rulesetFilePath))
                {
                    File.Delete(rulesetFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up temporary files");
            }
        }

        return issues;
    }

    /// <summary>
    /// Convert Spectral severity (0=error, 1=warn, 2=info, 3=hint) to LintSeverity
    /// </summary>
    private LintSeverity ConvertSpectralSeverity(int severity)
    {
        return severity switch
        {
            0 => LintSeverity.Error,
            1 => LintSeverity.Warning,
            2 => LintSeverity.Info,
            3 => LintSeverity.Hint,
            _ => LintSeverity.Info
        };
    }

}

/// <summary>
/// Spectral issue output format
/// </summary>
internal class SpectralIssue
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public int Severity { get; set; } // 0=error, 1=warn, 2=info, 3=hint
    public List<string>? Path { get; set; }
    public SpectralRange? Range { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// Spectral range format
/// </summary>
internal class SpectralRange
{
    public SpectralPosition? Start { get; set; }
    public SpectralPosition? End { get; set; }
}

/// <summary>
/// Spectral position format
/// </summary>
internal class SpectralPosition
{
    public int Line { get; set; }
    public int Character { get; set; }
}
