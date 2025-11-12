namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Workspace entity - top-level container for projects
/// </summary>
public class Workspace : BaseEntity
{
    /// <summary>
    /// Workspace name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Workspace description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Owner user ID
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Workspace settings (JSON)
    /// </summary>
    public string? Settings { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<DataDictionary> DataDictionaries { get; set; } = new List<DataDictionary>();
}
