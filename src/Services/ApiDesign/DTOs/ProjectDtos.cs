using Apivia.Shared.Data.Enums;

namespace Apivia.Services.ApiDesign.DTOs;

/// <summary>
/// Request to create a new project
/// </summary>
public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid? TeamId { get; set; }
}

/// <summary>
/// Request to update an existing project
/// </summary>
public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? TeamId { get; set; }
}

/// <summary>
/// Response containing project details
/// </summary>
public class ProjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public Guid? TeamId { get; set; }
    public string? TeamName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int ApiSpecCount { get; set; }
}

/// <summary>
/// Simplified project response for list views
/// </summary>
public class ProjectListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public int ApiSpecCount { get; set; }
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Query parameters for filtering projects
/// </summary>
public class ProjectQueryParams
{
    public Guid? WorkspaceId { get; set; }
    public Guid? TeamId { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "ModifiedAt";
    public bool SortDescending { get; set; } = true;
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
