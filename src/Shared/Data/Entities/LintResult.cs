namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Linting result for API Spec
/// </summary>
public class LintResult : BaseEntity
{
    public Guid ApiSpecId { get; set; }
    public Guid? RulesetId { get; set; }
    public bool IsValid { get; set; }
    public string Errors { get; set; } = string.Empty; // JSON array
    public string Warnings { get; set; } = string.Empty; // JSON array
    public string Info { get; set; } = string.Empty; // JSON array
    public DateTime ExecutedAt { get; set; }

    // Navigation properties
    public ApiSpec ApiSpec { get; set; } = null!;
}
