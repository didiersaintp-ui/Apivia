namespace Apivia.Services.Governance.DTOs;

/// <summary>
/// Request to create a link between API schema element and data dictionary
/// </summary>
public class CreateLinkRequest
{
    public Guid ApiSpecId { get; set; }
    public string SchemaPath { get; set; } = string.Empty; // JSONPath to schema element
    public Guid? DataEntityId { get; set; }
    public Guid? DataAttributeId { get; set; }
    public bool AutoSync { get; set; } = false;
}

/// <summary>
/// Request to bulk create links
/// </summary>
public class BulkCreateLinksRequest
{
    public Guid ApiSpecId { get; set; }
    public List<CreateLinkRequest> Links { get; set; } = new();
}

/// <summary>
/// Response containing link details
/// </summary>
public class LinkResponse
{
    public Guid Id { get; set; }
    public Guid ApiSpecId { get; set; }
    public string ApiSpecName { get; set; } = string.Empty;
    public string SchemaPath { get; set; } = string.Empty;
    public Guid? DataEntityId { get; set; }
    public string? DataEntityName { get; set; }
    public Guid? DataAttributeId { get; set; }
    public string? DataAttributeName { get; set; }
    public bool AutoSynced { get; set; }
    public DateTime LinkedAt { get; set; }
    public Guid LinkedBy { get; set; }
    public string LinkedByName { get; set; } = string.Empty;
}

/// <summary>
/// Simplified link response for list views
/// </summary>
public class LinkListItemResponse
{
    public Guid Id { get; set; }
    public Guid ApiSpecId { get; set; }
    public string ApiSpecName { get; set; } = string.Empty;
    public string SchemaPath { get; set; } = string.Empty;
    public Guid? DataEntityId { get; set; }
    public string? DataEntityName { get; set; }
    public Guid? DataAttributeId { get; set; }
    public string? DataAttributeName { get; set; }
    public DateTime LinkedAt { get; set; }
}

/// <summary>
/// Query parameters for filtering links
/// </summary>
public class LinkQueryParams
{
    public Guid? ApiSpecId { get; set; }
    public Guid? DataEntityId { get; set; }
    public Guid? DataAttributeId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? DictionaryId { get; set; }
    public bool? AutoSynced { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Response for analyzing potential links
/// </summary>
public class LinkageAnalysisResponse
{
    public Guid ApiSpecId { get; set; }
    public List<SuggestedLink> SuggestedLinks { get; set; } = new();
    public int TotalSuggestions { get; set; }
}

/// <summary>
/// Suggested link based on naming/structure analysis
/// </summary>
public class SuggestedLink
{
    public string SchemaPath { get; set; } = string.Empty;
    public string SchemaType { get; set; } = string.Empty;
    public Guid? SuggestedEntityId { get; set; }
    public string? SuggestedEntityName { get; set; }
    public Guid? SuggestedAttributeId { get; set; }
    public string? SuggestedAttributeName { get; set; }
    public double ConfidenceScore { get; set; } // 0.0 to 1.0
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Response showing all links for an API spec
/// </summary>
public class ApiSpecLinksResponse
{
    public Guid ApiSpecId { get; set; }
    public string ApiSpecName { get; set; } = string.Empty;
    public List<LinkResponse> Links { get; set; } = new();
    public int TotalLinks { get; set; }
    public int EntityLinksCount { get; set; }
    public int AttributeLinksCount { get; set; }
}

/// <summary>
/// Response showing all links for a data entity
/// </summary>
public class DataEntityLinksResponse
{
    public Guid DataEntityId { get; set; }
    public string DataEntityName { get; set; } = string.Empty;
    public List<LinkResponse> Links { get; set; } = new();
    public int TotalLinks { get; set; }
    public List<string> LinkedApiSpecs { get; set; } = new();
}
