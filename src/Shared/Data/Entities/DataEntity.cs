namespace Apivia.Shared.Data.Entities;

public enum DataSensitivityLevel
{
    Public,
    Internal,
    Confidential,
    Restricted
}

public enum DataQualityLevel
{
    Bronze,
    Silver,
    Gold,
    Platinum
}

/// <summary>
/// Data Entity - business entity definition
/// </summary>
public class DataEntity : BaseEntity
{
    public Guid DictionaryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BusinessOwner { get; set; }
    public string? BusinessOwnerEmail { get; set; }
    public string? FunctionalDomain { get; set; }
    public DataSensitivityLevel? Sensitivity { get; set; }
    public DataQualityLevel? QualityLevel { get; set; }
    public string? Metadata { get; set; } // JSON metadata
    public int Version { get; set; } = 1;
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }

    // Navigation properties
    public DataDictionary Dictionary { get; set; } = null!;
    public ICollection<DataAttribute> Attributes { get; set; } = new List<DataAttribute>();
    public ICollection<ApiSchemaElement> LinkedSchemas { get; set; } = new List<ApiSchemaElement>();
    public ICollection<ImpactAnalysis> ImpactAnalyses { get; set; } = new List<ImpactAnalysis>();
}
