namespace Apivia.Shared.Data.Entities;

public enum TeamRole
{
    Owner,
    Admin,
    Editor,
    Viewer,
    Guest
}

/// <summary>
/// Team member - many-to-many relationship between Team and User
/// </summary>
public class TeamMember : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public TeamRole Role { get; set; }

    // Navigation properties
    public Team Team { get; set; } = null!;
    public User User { get; set; } = null!;
}
