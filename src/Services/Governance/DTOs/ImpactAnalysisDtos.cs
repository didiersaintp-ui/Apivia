using Apivia.Shared.Data.Enums;

namespace Apivia.Services.Governance.DTOs;

/// <summary>
/// Request to create an impact analysis
/// </summary>
public class CreateImpactAnalysisRequest
{
    public Guid? DataEntityId { get; set; }
    public Guid? DataAttributeId { get; set; }
    public ChangeType ChangeType { get; set; }
    public string ProposedChanges { get; set; } = string.Empty; // JSON description
    public string? Notes { get; set; }
}

/// <summary>
/// Response containing impact analysis details
/// </summary>
public class ImpactAnalysisResponse
{
    public Guid Id { get; set; }
    public Guid? DataEntityId { get; set; }
    public string? DataEntityName { get; set; }
    public Guid? DataAttributeId { get; set; }
    public string? DataAttributeName { get; set; }
    public ChangeType ChangeType { get; set; }
    public string ProposedChanges { get; set; } = string.Empty;
    public List<AffectedApiInfo> AffectedApis { get; set; } = new();
    public RiskLevel OverallRiskLevel { get; set; }
    public ImpactAnalysisStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string? ResolvedByName { get; set; }
}

/// <summary>
/// Simplified impact analysis response for list views
/// </summary>
public class ImpactAnalysisListItemResponse
{
    public Guid Id { get; set; }
    public Guid? DataEntityId { get; set; }
    public string? DataEntityName { get; set; }
    public Guid? DataAttributeId { get; set; }
    public string? DataAttributeName { get; set; }
    public ChangeType ChangeType { get; set; }
    public RiskLevel OverallRiskLevel { get; set; }
    public ImpactAnalysisStatus Status { get; set; }
    public int AffectedApiCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Information about an affected API
/// </summary>
public class AffectedApiInfo
{
    public Guid ApiSpecId { get; set; }
    public string ApiSpecName { get; set; } = string.Empty;
    public string ApiSpecVersion { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public List<string> AffectedPaths { get; set; } = new();
    public RiskLevel RiskLevel { get; set; }
    public bool IsPublished { get; set; }
}

/// <summary>
/// Query parameters for filtering impact analyses
/// </summary>
public class ImpactAnalysisQueryParams
{
    public Guid? DataEntityId { get; set; }
    public Guid? DataAttributeId { get; set; }
    public Guid? DictionaryId { get; set; }
    public ChangeType? ChangeType { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public ImpactAnalysisStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Request to resolve an impact analysis
/// </summary>
public class ResolveImpactAnalysisRequest
{
    public string? Resolution { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to preview impact before creating analysis (dry-run)
/// </summary>
public class PreviewImpactRequest
{
    public Guid? DataEntityId { get; set; }
    public Guid? DataAttributeId { get; set; }
    public ChangeType ChangeType { get; set; }
}

/// <summary>
/// Response with impact preview (dry-run analysis without persistence)
/// </summary>
public class ImpactPreviewResponse
{
    public Guid? DataEntityId { get; set; }
    public string? DataEntityName { get; set; }
    public Guid? DataAttributeId { get; set; }
    public string? DataAttributeName { get; set; }
    public ChangeType ChangeType { get; set; }
    public List<AffectedApiInfo> AffectedApis { get; set; } = new();
    public RiskLevel CalculatedRiskLevel { get; set; }
    public int TotalAffectedApis { get; set; }
    public int PublishedApisAffected { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Paginated list response
/// </summary>
public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Statistics for impact analyses
/// </summary>
public class ImpactAnalysisStatsResponse
{
    public int TotalAnalyses { get; set; }
    public int PendingAnalyses { get; set; }
    public int InProgressAnalyses { get; set; }
    public int ResolvedAnalyses { get; set; }
    public Dictionary<RiskLevel, int> AnalysesByRiskLevel { get; set; } = new();
    public Dictionary<ChangeType, int> AnalysesByChangeType { get; set; } = new();
}
