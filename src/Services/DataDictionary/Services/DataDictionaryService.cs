using Apivia.Services.DataDictionary.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apivia.Services.DataDictionary.Services;

/// <summary>
/// Service interface for data dictionary operations
/// </summary>
public interface IDataDictionaryService
{
    Task<DataDictionaryResponse> CreateAsync(CreateDataDictionaryRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<DataDictionaryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<DataDictionaryListItemResponse>> GetPagedAsync(DataDictionaryQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<DataDictionaryResponse> UpdateAsync(Guid id, UpdateDataDictionaryRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of data dictionary service
/// </summary>
public class DataDictionaryService : IDataDictionaryService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<DataDictionaryService> _logger;

    public DataDictionaryService(ApiviaDbContext context, ILogger<DataDictionaryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new data dictionary
    /// </summary>
    public async Task<DataDictionaryResponse> CreateAsync(CreateDataDictionaryRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify workspace exists
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId && w.IsActive, cancellationToken);

        if (workspace == null)
        {
            throw new InvalidOperationException("Workspace not found");
        }

        // Check for duplicate dictionary name in workspace
        var existingDictionary = await _context.DataDictionaries
            .AnyAsync(d => d.WorkspaceId == request.WorkspaceId &&
                          d.Name == request.Name &&
                          d.IsActive, cancellationToken);

        if (existingDictionary)
        {
            throw new InvalidOperationException("A data dictionary with this name already exists in the workspace");
        }

        // Create data dictionary
        var dictionary = new Apivia.Shared.Data.Entities.DataDictionary
        {
            Name = request.Name,
            Description = request.Description,
            WorkspaceId = request.WorkspaceId,
            CreatedBy = userId,
            ModifiedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.DataDictionaries.Add(dictionary);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Dictionary {DictionaryId} created by user {UserId}", dictionary.Id, userId);

        return await GetByIdAsync(dictionary.Id, cancellationToken);
    }

    /// <summary>
    /// Get data dictionary by ID with related data
    /// </summary>
    public async Task<DataDictionaryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dictionary = await _context.DataDictionaries
            .Include(d => d.Workspace)
            .Include(d => d.Creator)
            .Where(d => d.Id == id && d.IsActive)
            .Select(d => new DataDictionaryResponse
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                WorkspaceId = d.WorkspaceId,
                WorkspaceName = d.Workspace.Name,
                CreatedAt = d.CreatedAt,
                ModifiedAt = d.ModifiedAt,
                CreatedBy = d.CreatedBy,
                CreatedByName = d.Creator.FullName,
                EntityCount = d.Entities.Count(e => e.IsActive)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dictionary == null)
        {
            throw new KeyNotFoundException($"Data Dictionary with ID {id} not found");
        }

        return dictionary;
    }

    /// <summary>
    /// Get paginated list of data dictionaries with filtering
    /// </summary>
    public async Task<PagedResponse<DataDictionaryListItemResponse>> GetPagedAsync(DataDictionaryQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.DataDictionaries.Where(d => d.IsActive).AsQueryable();

        // Apply filters
        if (queryParams.WorkspaceId.HasValue)
        {
            query = query.Where(d => d.WorkspaceId == queryParams.WorkspaceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var searchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(d =>
                d.Name.ToLower().Contains(searchTerm) ||
                (d.Description != null && d.Description.ToLower().Contains(searchTerm)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = queryParams.SortBy?.ToLower() switch
        {
            "name" => queryParams.SortDescending
                ? query.OrderByDescending(d => d.Name)
                : query.OrderBy(d => d.Name),
            "createdat" => queryParams.SortDescending
                ? query.OrderByDescending(d => d.CreatedAt)
                : query.OrderBy(d => d.CreatedAt),
            _ => queryParams.SortDescending
                ? query.OrderByDescending(d => d.ModifiedAt)
                : query.OrderBy(d => d.ModifiedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Include(d => d.Workspace)
            .Select(d => new DataDictionaryListItemResponse
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                WorkspaceId = d.WorkspaceId,
                WorkspaceName = d.Workspace.Name,
                EntityCount = d.Entities.Count(e => e.IsActive),
                ModifiedAt = d.ModifiedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<DataDictionaryListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// Update an existing data dictionary
    /// </summary>
    public async Task<DataDictionaryResponse> UpdateAsync(Guid id, UpdateDataDictionaryRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var dictionary = await _context.DataDictionaries
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive, cancellationToken);

        if (dictionary == null)
        {
            throw new KeyNotFoundException($"Data Dictionary with ID {id} not found");
        }

        // Check for duplicate name if name is changing
        if (dictionary.Name != request.Name)
        {
            var existingDictionary = await _context.DataDictionaries
                .AnyAsync(d => d.WorkspaceId == dictionary.WorkspaceId &&
                              d.Name == request.Name &&
                              d.Id != id &&
                              d.IsActive, cancellationToken);

            if (existingDictionary)
            {
                throw new InvalidOperationException("A data dictionary with this name already exists in the workspace");
            }
        }

        // Update fields
        dictionary.Name = request.Name;
        dictionary.Description = request.Description;
        dictionary.ModifiedBy = userId;
        dictionary.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Dictionary {DictionaryId} updated by user {UserId}", dictionary.Id, userId);

        return await GetByIdAsync(dictionary.Id, cancellationToken);
    }

    /// <summary>
    /// Soft delete a data dictionary
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dictionary = await _context.DataDictionaries
            .Include(d => d.Entities)
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive, cancellationToken);

        if (dictionary == null)
        {
            throw new KeyNotFoundException($"Data Dictionary with ID {id} not found");
        }

        // Soft delete
        dictionary.IsActive = false;
        dictionary.ModifiedAt = DateTime.UtcNow;

        // Also soft delete all associated entities
        foreach (var entity in dictionary.Entities.Where(e => e.IsActive))
        {
            entity.IsActive = false;
            entity.ModifiedAt = DateTime.UtcNow;

            // Soft delete all attributes of the entity
            var attributes = await _context.DataAttributes
                .Where(a => a.EntityId == entity.Id && a.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var attribute in attributes)
            {
                attribute.IsActive = false;
                attribute.ModifiedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Dictionary {DictionaryId} soft deleted with {EntityCount} entities",
            dictionary.Id, dictionary.Entities.Count);

        return true;
    }
}
