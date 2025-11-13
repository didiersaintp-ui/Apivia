using Apivia.Services.DataDictionary.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apivia.Services.DataDictionary.Services;

/// <summary>
/// Service interface for data entity operations
/// </summary>
public interface IDataEntityService
{
    Task<DataEntityResponse> CreateAsync(CreateDataEntityRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<DataEntityResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<DataEntityListItemResponse>> GetPagedAsync(DataEntityQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<DataEntityResponse> UpdateAsync(Guid id, UpdateDataEntityRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of data entity service
/// </summary>
public class DataEntityService : IDataEntityService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<DataEntityService> _logger;

    public DataEntityService(ApiviaDbContext context, ILogger<DataEntityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new data entity
    /// </summary>
    public async Task<DataEntityResponse> CreateAsync(CreateDataEntityRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify dictionary exists
        var dictionary = await _context.DataDictionaries
            .FirstOrDefaultAsync(d => d.Id == request.DictionaryId && d.IsActive, cancellationToken);

        if (dictionary == null)
        {
            throw new InvalidOperationException("Data dictionary not found");
        }

        // Check for duplicate entity name in dictionary
        var existingEntity = await _context.DataEntities
            .AnyAsync(e => e.DictionaryId == request.DictionaryId &&
                          e.Name == request.Name &&
                          e.IsActive, cancellationToken);

        if (existingEntity)
        {
            throw new InvalidOperationException("A data entity with this name already exists in the dictionary");
        }

        // Create data entity
        var entity = new DataEntity
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            DictionaryId = request.DictionaryId,
            BusinessOwner = request.BusinessOwner,
            FunctionalDomain = request.FunctionalDomain,
            Sensitivity = request.Sensitivity,
            QualityLevel = request.QualityLevel,
            Metadata = request.Metadata,
            CreatedBy = userId,
            ModifiedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.DataEntities.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Entity {EntityId} created by user {UserId} in dictionary {DictionaryId}",
            entity.Id, userId, request.DictionaryId);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    /// <summary>
    /// Get data entity by ID with related data
    /// </summary>
    public async Task<DataEntityResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DataEntities
            .Include(e => e.Dictionary)
            .Include(e => e.Creator)
            .Where(e => e.Id == id && e.IsActive)
            .Select(e => new DataEntityResponse
            {
                Id = e.Id,
                Name = e.Name,
                DisplayName = e.DisplayName,
                Description = e.Description,
                DictionaryId = e.DictionaryId,
                DictionaryName = e.Dictionary.Name,
                BusinessOwner = e.BusinessOwner,
                FunctionalDomain = e.FunctionalDomain,
                Sensitivity = e.Sensitivity,
                QualityLevel = e.QualityLevel,
                Metadata = e.Metadata,
                CreatedAt = e.CreatedAt,
                ModifiedAt = e.ModifiedAt,
                CreatedBy = e.CreatedBy,
                CreatedByName = e.Creator.FullName,
                AttributeCount = e.Attributes.Count(a => a.IsActive),
                LinkedApiCount = e.LinkedSchemas.Count(s => s.IsActive)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Data Entity with ID {id} not found");
        }

        return entity;
    }

    /// <summary>
    /// Get paginated list of data entities with filtering
    /// </summary>
    public async Task<PagedResponse<DataEntityListItemResponse>> GetPagedAsync(DataEntityQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.DataEntities.Where(e => e.IsActive).AsQueryable();

        // Apply filters
        if (queryParams.DictionaryId.HasValue)
        {
            query = query.Where(e => e.DictionaryId == queryParams.DictionaryId.Value);
        }

        if (queryParams.WorkspaceId.HasValue)
        {
            query = query.Where(e => e.Dictionary.WorkspaceId == queryParams.WorkspaceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.FunctionalDomain))
        {
            query = query.Where(e => e.FunctionalDomain == queryParams.FunctionalDomain);
        }

        if (queryParams.Sensitivity.HasValue)
        {
            query = query.Where(e => e.Sensitivity == queryParams.Sensitivity.Value);
        }

        if (queryParams.QualityLevel.HasValue)
        {
            query = query.Where(e => e.QualityLevel == queryParams.QualityLevel.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var searchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(searchTerm) ||
                e.DisplayName.ToLower().Contains(searchTerm) ||
                (e.Description != null && e.Description.ToLower().Contains(searchTerm)) ||
                (e.BusinessOwner != null && e.BusinessOwner.ToLower().Contains(searchTerm)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = queryParams.SortBy?.ToLower() switch
        {
            "name" => queryParams.SortDescending
                ? query.OrderByDescending(e => e.Name)
                : query.OrderBy(e => e.Name),
            "displayname" => queryParams.SortDescending
                ? query.OrderByDescending(e => e.DisplayName)
                : query.OrderBy(e => e.DisplayName),
            "createdat" => queryParams.SortDescending
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt),
            _ => queryParams.SortDescending
                ? query.OrderByDescending(e => e.ModifiedAt)
                : query.OrderBy(e => e.ModifiedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(e => new DataEntityListItemResponse
            {
                Id = e.Id,
                Name = e.Name,
                DisplayName = e.DisplayName,
                Description = e.Description,
                DictionaryId = e.DictionaryId,
                BusinessOwner = e.BusinessOwner,
                FunctionalDomain = e.FunctionalDomain,
                Sensitivity = e.Sensitivity,
                QualityLevel = e.QualityLevel,
                AttributeCount = e.Attributes.Count(a => a.IsActive),
                LinkedApiCount = e.LinkedSchemas.Count(s => s.IsActive),
                ModifiedAt = e.ModifiedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<DataEntityListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// Update an existing data entity
    /// </summary>
    public async Task<DataEntityResponse> UpdateAsync(Guid id, UpdateDataEntityRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DataEntities
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Data Entity with ID {id} not found");
        }

        // Check for duplicate name if name is changing
        if (entity.Name != request.Name)
        {
            var existingEntity = await _context.DataEntities
                .AnyAsync(e => e.DictionaryId == entity.DictionaryId &&
                              e.Name == request.Name &&
                              e.Id != id &&
                              e.IsActive, cancellationToken);

            if (existingEntity)
            {
                throw new InvalidOperationException("A data entity with this name already exists in the dictionary");
            }
        }

        // Update fields
        entity.Name = request.Name;
        entity.DisplayName = request.DisplayName;
        entity.Description = request.Description;
        entity.BusinessOwner = request.BusinessOwner;
        entity.FunctionalDomain = request.FunctionalDomain;
        entity.Sensitivity = request.Sensitivity;
        entity.QualityLevel = request.QualityLevel;
        entity.Metadata = request.Metadata;
        entity.ModifiedBy = userId;
        entity.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Entity {EntityId} updated by user {UserId}", entity.Id, userId);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    /// <summary>
    /// Soft delete a data entity
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DataEntities
            .Include(e => e.Attributes)
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Data Entity with ID {id} not found");
        }

        // Check if entity has linked API schemas
        var hasLinkedApis = await _context.ApiSchemaElements
            .AnyAsync(s => s.DataEntityId == id && s.IsActive, cancellationToken);

        if (hasLinkedApis)
        {
            throw new InvalidOperationException("Cannot delete data entity that is linked to API specifications. Please remove the links first.");
        }

        // Soft delete
        entity.IsActive = false;
        entity.ModifiedAt = DateTime.UtcNow;

        // Also soft delete all associated attributes
        foreach (var attribute in entity.Attributes.Where(a => a.IsActive))
        {
            attribute.IsActive = false;
            attribute.ModifiedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Entity {EntityId} soft deleted with {AttributeCount} attributes",
            entity.Id, entity.Attributes.Count);

        return true;
    }
}
