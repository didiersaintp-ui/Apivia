namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Project-Team relationship for permissions
/// </summary>
public class ProjectTeam : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid TeamId { get; set; }
    public TeamRole Role { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
