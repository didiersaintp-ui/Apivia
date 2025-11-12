namespace Apivia.Shared.Data.Entities;

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Link,
    Unlink,
    Publish,
    Approve,
    Reject
}

/// <summary>
/// Audit log for tracking all changes
/// </summary>
public class AuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public AuditAction Action { get; set; }
    public Guid UserId { get; set; }
    public string? Changes { get; set; } // JSON diff
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
