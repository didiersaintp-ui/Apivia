namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Comment on a project or API spec
/// </summary>
public class Comment : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public Guid? ApiSpecId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Position { get; set; } // JSON position in document
    public Guid? ParentCommentId { get; set; }
    public bool IsResolved { get; set; }

    // Navigation properties
    public Project? Project { get; set; }
    public ApiSpec? ApiSpec { get; set; }
    public User User { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
