namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Data Dictionary - centralized data definitions
/// </summary>
public class DataDictionary : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public Workspace Workspace { get; set; } = null!;
    public ICollection<DataEntity> Entities { get; set; } = new List<DataEntity>();
}
