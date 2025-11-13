using Apivia.Shared.Data.Enums;

namespace Apivia.Services.DataDictionary.DTOs;

/// <summary>
/// Request to create a new data entity
/// </summary>
public class CreateDataEntityRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DictionaryId { get; set; }
    public string? BusinessOwner { get; set; }
    public string? FunctionalDomain { get; set; }
    public DataSensitivityLevel? Sensitivity { get; set; }
    public DataQualityLevel? QualityLevel { get; set; }
    public string? Metadata { get; set; } // JSON metadata
}

/// <summary>
/// Request to update an existing data entity
/// </summary>
public class UpdateDataEntityRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BusinessOwner { get; set; }
    public string? FunctionalDomain { get; set; }
    public DataSensitivityLevel? Sensitivity { get; set; }
    public DataQualityLevel? QualityLevel { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Response containing data entity details
/// </summary>
public class DataEntityResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DictionaryId { get; set; }
    public string DictionaryName { get; set; } = string.Empty;
    public string? BusinessOwner { get; set; }
    public string? FunctionalDomain { get; set; }
    public DataSensitivityLevel? Sensitivity { get; set; }
    public DataQualityLevel? QualityLevel { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int AttributeCount { get; set; }
    public int LinkedApiCount { get; set; }
}

/// <summary>
/// Simplified data entity response for list views
/// </summary>
public class DataEntityListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DictionaryId { get; set; }
    public string? BusinessOwner { get; set; }
    public string? FunctionalDomain { get; set; }
    public DataSensitivityLevel? Sensitivity { get; set; }
    public DataQualityLevel? QualityLevel { get; set; }
    public int AttributeCount { get; set; }
    public int LinkedApiCount { get; set; }
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Query parameters for filtering data entities
/// </summary>
public class DataEntityQueryParams
{
    public Guid? DictionaryId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public string? SearchTerm { get; set; }
    public string? FunctionalDomain { get; set; }
    public DataSensitivityLevel? Sensitivity { get; set; }
    public DataQualityLevel? QualityLevel { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "ModifiedAt";
    public bool SortDescending { get; set; } = true;
}
