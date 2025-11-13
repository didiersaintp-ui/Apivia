namespace Apivia.Services.DataDictionary.DTOs;

/// <summary>
/// Request to create a new data dictionary
/// </summary>
public class CreateDataDictionaryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkspaceId { get; set; }
}

/// <summary>
/// Request to update an existing data dictionary
/// </summary>
public class UpdateDataDictionaryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Response containing data dictionary details
/// </summary>
public class DataDictionaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int EntityCount { get; set; }
}

/// <summary>
/// Simplified data dictionary response for list views
/// </summary>
public class DataDictionaryListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public int EntityCount { get; set; }
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Query parameters for filtering data dictionaries
/// </summary>
public class DataDictionaryQueryParams
{
    public Guid? WorkspaceId { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "ModifiedAt";
    public bool SortDescending { get; set; } = true;
}
