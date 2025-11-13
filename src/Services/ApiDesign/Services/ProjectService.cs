using Apivia.Services.ApiDesign.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apivia.Services.ApiDesign.Services;

/// <summary>
/// Service interface for project operations
/// </summary>
public interface IProjectService
{
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ProjectResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProjectListItemResponse>> GetPagedAsync(ProjectQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of project service
/// </summary>
public class ProjectService : IProjectService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ApiviaDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify workspace exists and user has access
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId, cancellationToken);

        if (workspace == null)
        {
            throw new InvalidOperationException("Workspace not found");
        }

        // Verify team exists if provided
        if (request.TeamId.HasValue)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == request.TeamId.Value && t.WorkspaceId == request.WorkspaceId, cancellationToken);

            if (team == null)
            {
                throw new InvalidOperationException("Team not found or does not belong to the workspace");
            }
        }

        // Check for duplicate project name in workspace
        var existingProject = await _context.Projects
            .AnyAsync(p => p.WorkspaceId == request.WorkspaceId &&
                          p.Name == request.Name &&
                          p.IsActive, cancellationToken);

        if (existingProject)
        {
            throw new InvalidOperationException("A project with this name already exists in the workspace");
        }

        // Create project
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            WorkspaceId = request.WorkspaceId,
            TeamId = request.TeamId,
            CreatedBy = userId,
            ModifiedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} created by user {UserId}", project.Id, userId);

        return await GetByIdAsync(project.Id, cancellationToken);
    }

    /// <summary>
    /// Get project by ID with related data
    /// </summary>
    public async Task<ProjectResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Team)
            .Include(p => p.Creator)
            .Where(p => p.Id == id)
            .Select(p => new ProjectResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                WorkspaceId = p.WorkspaceId,
                WorkspaceName = p.Workspace.Name,
                TeamId = p.TeamId,
                TeamName = p.Team != null ? p.Team.Name : null,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt,
                CreatedBy = p.CreatedBy,
                CreatedByName = p.Creator.FullName,
                ApiSpecCount = p.ApiSpecs.Count(a => a.IsActive)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {id} not found");
        }

        return project;
    }

    /// <summary>
    /// Get paginated list of projects with filtering
    /// </summary>
    public async Task<PagedResponse<ProjectListItemResponse>> GetPagedAsync(ProjectQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Projects.AsQueryable();

        // Apply filters
        if (queryParams.WorkspaceId.HasValue)
        {
            query = query.Where(p => p.WorkspaceId == queryParams.WorkspaceId.Value);
        }

        if (queryParams.TeamId.HasValue)
        {
            query = query.Where(p => p.TeamId == queryParams.TeamId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var searchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = queryParams.SortBy?.ToLower() switch
        {
            "name" => queryParams.SortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "createdat" => queryParams.SortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => queryParams.SortDescending
                ? query.OrderByDescending(p => p.ModifiedAt)
                : query.OrderBy(p => p.ModifiedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Include(p => p.Workspace)
            .Select(p => new ProjectListItemResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                WorkspaceId = p.WorkspaceId,
                WorkspaceName = p.Workspace.Name,
                ApiSpecCount = p.ApiSpecs.Count(a => a.IsActive),
                ModifiedAt = p.ModifiedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<ProjectListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    public async Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {id} not found");
        }

        // Check for duplicate name if name is changing
        if (project.Name != request.Name)
        {
            var existingProject = await _context.Projects
                .AnyAsync(p => p.WorkspaceId == project.WorkspaceId &&
                              p.Name == request.Name &&
                              p.Id != id &&
                              p.IsActive, cancellationToken);

            if (existingProject)
            {
                throw new InvalidOperationException("A project with this name already exists in the workspace");
            }
        }

        // Verify team if provided
        if (request.TeamId.HasValue)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == request.TeamId.Value && t.WorkspaceId == project.WorkspaceId, cancellationToken);

            if (team == null)
            {
                throw new InvalidOperationException("Team not found or does not belong to the workspace");
            }
        }

        // Update fields
        project.Name = request.Name;
        project.Description = request.Description;
        project.TeamId = request.TeamId;
        project.ModifiedBy = userId;
        project.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} updated by user {UserId}", project.Id, userId);

        return await GetByIdAsync(project.Id, cancellationToken);
    }

    /// <summary>
    /// Soft delete a project
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {id} not found");
        }

        // Soft delete
        project.IsActive = false;
        project.ModifiedAt = DateTime.UtcNow;

        // Also soft delete all associated API specs
        var apiSpecs = await _context.ApiSpecs
            .Where(a => a.ProjectId == id && a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var spec in apiSpecs)
        {
            spec.IsActive = false;
            spec.ModifiedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} soft deleted with {SpecCount} API specs", project.Id, apiSpecs.Count);

        return true;
    }
}
