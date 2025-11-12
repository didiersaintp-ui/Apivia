using Microsoft.AspNetCore.Identity;

namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Application user entity extending IdentityUser
/// </summary>
public class User : IdentityUser<Guid>
{
    /// <summary>
    /// User's full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Profile picture URL
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Indicates if the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// Workspaces owned by this user
    /// </summary>
    public ICollection<Workspace> OwnedWorkspaces { get; set; } = new List<Workspace>();

    /// <summary>
    /// Team memberships
    /// </summary>
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

    /// <summary>
    /// Projects created by this user
    /// </summary>
    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();

    /// <summary>
    /// API Specs created by this user
    /// </summary>
    public ICollection<ApiSpec> CreatedApiSpecs { get; set; } = new List<ApiSpec>();

    /// <summary>
    /// Comments made by this user
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public User()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }
}
