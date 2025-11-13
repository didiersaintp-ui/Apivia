using Apivia.Shared.Data.Enums;

namespace Apivia.Services.ApiDesign.DTOs;

/// <summary>
/// Request to create a new API specification
/// </summary>
public class CreateApiSpecRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public ApiSpecFormat Format { get; set; } = ApiSpecFormat.OpenApiV3;
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Request to update an existing API specification
/// </summary>
public class UpdateApiSpecRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Response containing API specification details
/// </summary>
public class ApiSpecResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public ApiSpecFormat Format { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ApiSpecStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public Guid? ModifiedBy { get; set; }
    public string? ModifiedByName { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// Simplified API spec response for list views
/// </summary>
public class ApiSpecListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public ApiSpecFormat Format { get; set; }
    public string Version { get; set; } = string.Empty;
    public ApiSpecStatus Status { get; set; }
    public bool IsPublished { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByName { get; set; } = string.Empty;
}

/// <summary>
/// Query parameters for filtering API specifications
/// </summary>
public class ApiSpecQueryParams
{
    public Guid? ProjectId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public ApiSpecFormat? Format { get; set; }
    public ApiSpecStatus? Status { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "ModifiedAt";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Request to validate an API specification
/// </summary>
public class ValidateApiSpecRequest
{
    public ApiSpecFormat Format { get; set; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Response containing validation results
/// </summary>
public class ValidationResponse
{
    public bool IsValid { get; set; }
    public IEnumerable<ValidationError> Errors { get; set; } = new List<ValidationError>();
    public IEnumerable<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();
    public ValidationMetadata? Metadata { get; set; }
}

/// <summary>
/// Validation error details
/// </summary>
public class ValidationError
{
    public string Path { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public string Severity { get; set; } = "Error";
}

/// <summary>
/// Validation warning details
/// </summary>
public class ValidationWarning
{
    public string Path { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public string Severity { get; set; } = "Warning";
}

/// <summary>
/// Metadata extracted from validated specification
/// </summary>
public class ValidationMetadata
{
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public int EndpointCount { get; set; }
    public int SchemaCount { get; set; }
    public IEnumerable<string> Tags { get; set; } = new List<string>();
}

/// <summary>
/// Request to import API specification from various formats
/// </summary>
public class ImportApiSpecRequest
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ImportFormat SourceFormat { get; set; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Supported import formats
/// </summary>
public enum ImportFormat
{
    OpenApiV2,
    OpenApiV3,
    PostmanCollection
}

/// <summary>
/// Response after successful import
/// </summary>
public class ImportResponse
{
    public Guid ApiSpecId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool ConversionRequired { get; set; }
    public string? ConversionNotes { get; set; }
}

/// <summary>
/// Request to export API specification
/// </summary>
public class ExportApiSpecRequest
{
    public Guid ApiSpecId { get; set; }
    public ExportFormat TargetFormat { get; set; }
}

/// <summary>
/// Supported export formats
/// </summary>
public enum ExportFormat
{
    Yaml,
    Json,
    PostmanCollection
}

/// <summary>
/// Response containing exported specification
/// </summary>
public class ExportResponse
{
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// Request to publish an API specification
/// </summary>
public class PublishApiSpecRequest
{
    public string? ReleaseNotes { get; set; }
}

/// <summary>
/// Request to create a new version of an API specification
/// </summary>
public class CreateVersionRequest
{
    public string NewVersion { get; set; } = string.Empty;
    public string? VersionNotes { get; set; }
}
