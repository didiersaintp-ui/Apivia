namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Linting result for API Spec validation
/// </summary>
public class LintResult : BaseEntity
{
    public Guid ApiSpecId { get; set; }
    public Guid? RulesetId { get; set; }
    public string Status { get; set; } = "Passed"; // Passed, PassedWithWarnings, Failed
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public int HintCount { get; set; }
    public string Issues { get; set; } = string.Empty; // JSON array of all issues
    public int ExecutionTimeMs { get; set; }
    public DateTime ExecutedAt { get; set; }

    // Navigation properties
    public ApiSpec ApiSpec { get; set; } = null!;
    public Ruleset? Ruleset { get; set; }
}
