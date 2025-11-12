namespace Apivia.Shared.Data.Entities;

public enum ChangeType
{
    TypeChange,
    Rename,
    Delete,
    AddAttribute,
    ModifyAttribute,
    RequiredChange
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum ImpactAnalysisStatus
{
    Pending,
    Approved,
    Rejected,
    Applied
}

/// <summary>
/// Impact Analysis for data entity changes
/// </summary>
public class ImpactAnalysis : BaseEntity
{
    public Guid? DataEntityId { get; set; }
    public ChangeType ChangeType { get; set; }
    public string ProposedChanges { get; set; } = string.Empty; // JSON
    public string AffectedApis { get; set; } = string.Empty; // JSON array
    public RiskLevel OverallRiskLevel { get; set; }
    public ImpactAnalysisStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }

    // Navigation properties
    public DataEntity? DataEntity { get; set; }
}
