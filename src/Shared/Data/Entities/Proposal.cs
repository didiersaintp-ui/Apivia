namespace Apivia.Shared.Data.Entities;

public enum ProposalStatus
{
    Draft,
    Open,
    Approved,
    Rejected,
    Merged
}

/// <summary>
/// Proposal for API changes
/// </summary>
public class Proposal : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Changes { get; set; } = string.Empty; // JSON diff
    public ProposalStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public User Creator { get; set; } = null!;
}
