namespace Apivia.Services.DataDictionary.DTOs;

/// <summary>
/// Request to create a new data attribute
/// </summary>
public class CreateDataAttributeRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid EntityId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationRules { get; set; } // JSON validation rules
    public string? Metadata { get; set; } // JSON metadata
}

/// <summary>
/// Request to update an existing data attribute
/// </summary>
public class UpdateDataAttributeRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationRules { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Response containing data attribute details
/// </summary>
public class DataAttributeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationRules { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int LinkedApiCount { get; set; }
}

/// <summary>
/// Simplified data attribute response for list views
/// </summary>
public class DataAttributeListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid EntityId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public int LinkedApiCount { get; set; }
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Query parameters for filtering data attributes
/// </summary>
public class DataAttributeQueryParams
{
    public Guid? EntityId { get; set; }
    public Guid? DictionaryId { get; set; }
    public string? SearchTerm { get; set; }
    public string? DataType { get; set; }
    public bool? IsRequired { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Request to bulk create attributes
/// </summary>
public class BulkCreateAttributesRequest
{
    public Guid EntityId { get; set; }
    public List<CreateDataAttributeRequest> Attributes { get; set; } = new();
}

/// <summary>
/// Response for bulk operations
/// </summary>
public class BulkOperationResponse
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Guid> CreatedIds { get; set; } = new();
}
