using System.ComponentModel.DataAnnotations;

namespace Apivia.Services.Linting.DTOs;

/// <summary>
/// Request to lint an API specification
/// </summary>
public class LintApiSpecRequest
{
    [Required]
    public Guid ApiSpecId { get; set; }

    public Guid? RulesetId { get; set; }

    public bool SaveResults { get; set; } = true;
}

/// <summary>
/// Request to lint content directly without saving
/// </summary>
public class LintContentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public ApiSpecFormat Format { get; set; }

    public Guid? RulesetId { get; set; }
}

/// <summary>
/// API specification format
/// </summary>
public enum ApiSpecFormat
{
    OpenApiYaml,
    OpenApiJson,
    AsyncApiYaml,
    AsyncApiJson
}

/// <summary>
/// Response containing linting results
/// </summary>
public class LintResultResponse
{
    public Guid? Id { get; set; }
    public Guid? ApiSpecId { get; set; }
    public string? ApiSpecName { get; set; }
    public string? ApiSpecVersion { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public int HintCount { get; set; }
    public int TotalIssues => ErrorCount + WarningCount + InfoCount + HintCount;
    public LintStatus Status { get; set; }
    public List<LintIssue> Issues { get; set; } = new();
    public DateTime? ExecutedAt { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public string? RulesetName { get; set; }
}

/// <summary>
/// Individual linting issue
/// </summary>
public class LintIssue
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public LintSeverity Severity { get; set; }
    public string Path { get; set; } = string.Empty;
    public LintIssueRange? Range { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// Location of an issue in the source
/// </summary>
public class LintIssueRange
{
    public LintPosition Start { get; set; } = new();
    public LintPosition End { get; set; } = new();
}

/// <summary>
/// Position in source (line and character)
/// </summary>
public class LintPosition
{
    public int Line { get; set; }
    public int Character { get; set; }
}

/// <summary>
/// Severity of linting issue
/// </summary>
public enum LintSeverity
{
    Error = 0,
    Warning = 1,
    Info = 2,
    Hint = 3
}

/// <summary>
/// Overall lint status
/// </summary>
public enum LintStatus
{
    Passed,
    PassedWithWarnings,
    Failed
}

/// <summary>
/// List item for lint results
/// </summary>
public class LintResultListItemResponse
{
    public Guid Id { get; set; }
    public Guid ApiSpecId { get; set; }
    public string ApiSpecName { get; set; } = string.Empty;
    public string ApiSpecVersion { get; set; } = string.Empty;
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public int TotalIssues => ErrorCount + WarningCount + InfoCount;
    public LintStatus Status { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string? RulesetName { get; set; }
}

/// <summary>
/// Query parameters for listing lint results
/// </summary>
public class LintResultQueryParams
{
    public Guid? ApiSpecId { get; set; }
    public LintStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Request to create a custom ruleset
/// </summary>
public class CreateRulesetRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string Rules { get; set; } = string.Empty; // YAML/JSON Spectral ruleset

    public bool IsDefault { get; set; }
}

/// <summary>
/// Response for a ruleset
/// </summary>
public class RulesetResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Rules { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsBuiltIn { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// List item for rulesets
/// </summary>
public class RulesetListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsBuiltIn { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// Request to update a ruleset
/// </summary>
public class UpdateRulesetRequest
{
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public string? Rules { get; set; }

    public bool? IsDefault { get; set; }
}

/// <summary>
/// Lint statistics for an API spec
/// </summary>
public class LintStatsResponse
{
    public Guid ApiSpecId { get; set; }
    public int TotalRuns { get; set; }
    public int PassedRuns { get; set; }
    public int FailedRuns { get; set; }
    public double PassRate => TotalRuns > 0 ? (double)PassedRuns / TotalRuns * 100 : 0;
    public DateTime? LastRunAt { get; set; }
    public LintStatus? LastRunStatus { get; set; }
    public int? LastRunErrorCount { get; set; }
    public int? LastRunWarningCount { get; set; }
    public List<LintTrendData> Trend { get; set; } = new();
}

/// <summary>
/// Trend data point for linting over time
/// </summary>
public class LintTrendData
{
    public DateTime Date { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public LintStatus Status { get; set; }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
