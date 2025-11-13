using Apivia.Services.DataDictionary.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apivia.Services.DataDictionary.Services;

/// <summary>
/// Service interface for data attribute operations
/// </summary>
public interface IDataAttributeService
{
    Task<DataAttributeResponse> CreateAsync(CreateDataAttributeRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<DataAttributeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<DataAttributeListItemResponse>> GetPagedAsync(DataAttributeQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<DataAttributeResponse> UpdateAsync(Guid id, UpdateDataAttributeRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BulkOperationResponse> BulkCreateAsync(BulkCreateAttributesRequest request, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of data attribute service
/// </summary>
public class DataAttributeService : IDataAttributeService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<DataAttributeService> _logger;

    public DataAttributeService(ApiviaDbContext context, ILogger<DataAttributeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new data attribute
    /// </summary>
    public async Task<DataAttributeResponse> CreateAsync(CreateDataAttributeRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify entity exists
        var entity = await _context.DataEntities
            .FirstOrDefaultAsync(e => e.Id == request.EntityId && e.IsActive, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException("Data entity not found");
        }

        // Check for duplicate attribute name in entity
        var existingAttribute = await _context.DataAttributes
            .AnyAsync(a => a.EntityId == request.EntityId &&
                          a.Name == request.Name &&
                          a.IsActive, cancellationToken);

        if (existingAttribute)
        {
            throw new InvalidOperationException("An attribute with this name already exists in the entity");
        }

        // Create data attribute
        var attribute = new DataAttribute
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            EntityId = request.EntityId,
            DataType = request.DataType.ToLower(),
            MaxLength = request.MaxLength,
            IsRequired = request.IsRequired,
            IsUnique = request.IsUnique,
            DefaultValue = request.DefaultValue,
            ValidationRules = request.ValidationRules,
            Metadata = request.Metadata,
            CreatedBy = userId,
            ModifiedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.DataAttributes.Add(attribute);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Attribute {AttributeId} created by user {UserId} in entity {EntityId}",
            attribute.Id, userId, request.EntityId);

        return await GetByIdAsync(attribute.Id, cancellationToken);
    }

    /// <summary>
    /// Get data attribute by ID with related data
    /// </summary>
    public async Task<DataAttributeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attribute = await _context.DataAttributes
            .Include(a => a.Entity)
            .Include(a => a.Creator)
            .Where(a => a.Id == id && a.IsActive)
            .Select(a => new DataAttributeResponse
            {
                Id = a.Id,
                Name = a.Name,
                DisplayName = a.DisplayName,
                Description = a.Description,
                EntityId = a.EntityId,
                EntityName = a.Entity.Name,
                DataType = a.DataType,
                MaxLength = a.MaxLength,
                IsRequired = a.IsRequired,
                IsUnique = a.IsUnique,
                DefaultValue = a.DefaultValue,
                ValidationRules = a.ValidationRules,
                Metadata = a.Metadata,
                CreatedAt = a.CreatedAt,
                ModifiedAt = a.ModifiedAt,
                CreatedBy = a.CreatedBy,
                CreatedByName = a.Creator.FullName,
                LinkedApiCount = a.LinkedSchemas.Count(s => s.IsActive)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (attribute == null)
        {
            throw new KeyNotFoundException($"Data Attribute with ID {id} not found");
        }

        return attribute;
    }

    /// <summary>
    /// Get paginated list of data attributes with filtering
    /// </summary>
    public async Task<PagedResponse<DataAttributeListItemResponse>> GetPagedAsync(DataAttributeQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.DataAttributes.Where(a => a.IsActive).AsQueryable();

        // Apply filters
        if (queryParams.EntityId.HasValue)
        {
            query = query.Where(a => a.EntityId == queryParams.EntityId.Value);
        }

        if (queryParams.DictionaryId.HasValue)
        {
            query = query.Where(a => a.Entity.DictionaryId == queryParams.DictionaryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.DataType))
        {
            var dataType = queryParams.DataType.ToLower();
            query = query.Where(a => a.DataType == dataType);
        }

        if (queryParams.IsRequired.HasValue)
        {
            query = query.Where(a => a.IsRequired == queryParams.IsRequired.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var searchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(searchTerm) ||
                a.DisplayName.ToLower().Contains(searchTerm) ||
                (a.Description != null && a.Description.ToLower().Contains(searchTerm)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = queryParams.SortBy?.ToLower() switch
        {
            "name" => queryParams.SortDescending
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name),
            "displayname" => queryParams.SortDescending
                ? query.OrderByDescending(a => a.DisplayName)
                : query.OrderBy(a => a.DisplayName),
            "datatype" => queryParams.SortDescending
                ? query.OrderByDescending(a => a.DataType)
                : query.OrderBy(a => a.DataType),
            "modifiedat" => queryParams.SortDescending
                ? query.OrderByDescending(a => a.ModifiedAt)
                : query.OrderBy(a => a.ModifiedAt),
            _ => queryParams.SortDescending
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name)
        };

        // Apply pagination
        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(a => new DataAttributeListItemResponse
            {
                Id = a.Id,
                Name = a.Name,
                DisplayName = a.DisplayName,
                Description = a.Description,
                EntityId = a.EntityId,
                DataType = a.DataType,
                IsRequired = a.IsRequired,
                IsUnique = a.IsUnique,
                LinkedApiCount = a.LinkedSchemas.Count(s => s.IsActive),
                ModifiedAt = a.ModifiedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<DataAttributeListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// Update an existing data attribute
    /// </summary>
    public async Task<DataAttributeResponse> UpdateAsync(Guid id, UpdateDataAttributeRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var attribute = await _context.DataAttributes
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (attribute == null)
        {
            throw new KeyNotFoundException($"Data Attribute with ID {id} not found");
        }

        // Check for duplicate name if name is changing
        if (attribute.Name != request.Name)
        {
            var existingAttribute = await _context.DataAttributes
                .AnyAsync(a => a.EntityId == attribute.EntityId &&
                              a.Name == request.Name &&
                              a.Id != id &&
                              a.IsActive, cancellationToken);

            if (existingAttribute)
            {
                throw new InvalidOperationException("An attribute with this name already exists in the entity");
            }
        }

        // Update fields
        attribute.Name = request.Name;
        attribute.DisplayName = request.DisplayName;
        attribute.Description = request.Description;
        attribute.DataType = request.DataType.ToLower();
        attribute.MaxLength = request.MaxLength;
        attribute.IsRequired = request.IsRequired;
        attribute.IsUnique = request.IsUnique;
        attribute.DefaultValue = request.DefaultValue;
        attribute.ValidationRules = request.ValidationRules;
        attribute.Metadata = request.Metadata;
        attribute.ModifiedBy = userId;
        attribute.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Attribute {AttributeId} updated by user {UserId}", attribute.Id, userId);

        return await GetByIdAsync(attribute.Id, cancellationToken);
    }

    /// <summary>
    /// Soft delete a data attribute
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attribute = await _context.DataAttributes
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (attribute == null)
        {
            throw new KeyNotFoundException($"Data Attribute with ID {id} not found");
        }

        // Check if attribute has linked API schemas
        var hasLinkedApis = await _context.ApiSchemaElements
            .AnyAsync(s => s.DataAttributeId == id && s.IsActive, cancellationToken);

        if (hasLinkedApis)
        {
            throw new InvalidOperationException("Cannot delete data attribute that is linked to API specifications. Please remove the links first.");
        }

        // Soft delete
        attribute.IsActive = false;
        attribute.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data Attribute {AttributeId} soft deleted", attribute.Id);

        return true;
    }

    /// <summary>
    /// Bulk create attributes
    /// </summary>
    public async Task<BulkOperationResponse> BulkCreateAsync(BulkCreateAttributesRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var response = new BulkOperationResponse();

        // Verify entity exists
        var entity = await _context.DataEntities
            .FirstOrDefaultAsync(e => e.Id == request.EntityId && e.IsActive, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException("Data entity not found");
        }

        // Get existing attribute names for duplicate check
        var existingNames = await _context.DataAttributes
            .Where(a => a.EntityId == request.EntityId && a.IsActive)
            .Select(a => a.Name.ToLower())
            .ToListAsync(cancellationToken);

        var attributesToCreate = new List<DataAttribute>();

        foreach (var attrRequest in request.Attributes)
        {
            try
            {
                // Check for duplicates
                if (existingNames.Contains(attrRequest.Name.ToLower()) ||
                    attributesToCreate.Any(a => a.Name.Equals(attrRequest.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    response.FailureCount++;
                    response.Errors.Add($"Duplicate attribute name: {attrRequest.Name}");
                    continue;
                }

                var attribute = new DataAttribute
                {
                    Name = attrRequest.Name,
                    DisplayName = attrRequest.DisplayName,
                    Description = attrRequest.Description,
                    EntityId = request.EntityId,
                    DataType = attrRequest.DataType.ToLower(),
                    MaxLength = attrRequest.MaxLength,
                    IsRequired = attrRequest.IsRequired,
                    IsUnique = attrRequest.IsUnique,
                    DefaultValue = attrRequest.DefaultValue,
                    ValidationRules = attrRequest.ValidationRules,
                    Metadata = attrRequest.Metadata,
                    CreatedBy = userId,
                    ModifiedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    IsActive = true
                };

                attributesToCreate.Add(attribute);
                response.CreatedIds.Add(attribute.Id);
                response.SuccessCount++;
            }
            catch (Exception ex)
            {
                response.FailureCount++;
                response.Errors.Add($"Failed to create attribute {attrRequest.Name}: {ex.Message}");
                _logger.LogError(ex, "Error creating attribute {AttributeName}", attrRequest.Name);
            }
        }

        if (attributesToCreate.Any())
        {
            _context.DataAttributes.AddRange(attributesToCreate);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bulk created {Count} attributes for entity {EntityId} by user {UserId}",
                attributesToCreate.Count, request.EntityId, userId);
        }

        return response;
    }
}
