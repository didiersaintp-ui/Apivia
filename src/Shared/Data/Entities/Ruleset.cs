namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Custom linting ruleset for Spectral validation
/// </summary>
public class Ruleset : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Rules { get; set; } = string.Empty; // YAML/JSON Spectral ruleset content
    public bool IsDefault { get; set; }
    public bool IsBuiltIn { get; set; }

    // Navigation properties
    public ICollection<LintResult> LintResults { get; set; } = new List<LintResult>();
}
