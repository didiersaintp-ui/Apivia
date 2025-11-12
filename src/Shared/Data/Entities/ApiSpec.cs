namespace Apivia.Shared.Data.Entities;

public enum ApiSpecStatus
{
    Draft,
    Published,
    Deprecated
}

/// <summary>
/// API Specification entity
/// </summary>
public class ApiSpec : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Version { get; set; } = "1.0.0";
    public string Content { get; set; } = string.Empty; // YAML/JSON content
    public string Format { get; set; } = "yaml"; // yaml or json
    public string OpenApiVersion { get; set; } = "3.0.0"; // 2.0, 3.0, 3.1
    public ApiSpecStatus Status { get; set; }
    public string? GitCommitSha { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<ApiSchemaElement> SchemaElements { get; set; } = new List<ApiSchemaElement>();
    public ICollection<LintResult> LintResults { get; set; } = new List<LintResult>();
    public ICollection<MockServer> MockServers { get; set; } = new List<MockServer>();
}
