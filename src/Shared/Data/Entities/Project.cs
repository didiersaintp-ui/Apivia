namespace Apivia.Shared.Data.Entities;

public enum ProjectVisibility
{
    Private,
    Internal,
    Public
}

/// <summary>
/// Project entity - container for API specifications
/// </summary>
public class Project : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectVisibility Visibility { get; set; }
    public string? GitRepoUrl { get; set; }
    public string? GitBranch { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }

    // Navigation properties
    public Workspace Workspace { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<ApiSpec> ApiSpecs { get; set; } = new List<ApiSpec>();
    public ICollection<ProjectTeam> ProjectTeams { get; set; } = new List<ProjectTeam>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
