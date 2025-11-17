using Apivia.Services.ApiDesign.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Apivia.Shared.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Apivia.Services.ApiDesign.Services;

/// <summary>
/// Service interface for API specification operations
/// </summary>
public interface IApiSpecService
{
    Task<ApiSpecResponse> CreateAsync(CreateApiSpecRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiSpecResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ApiSpecListItemResponse>> GetPagedAsync(ApiSpecQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<ApiSpecResponse> UpdateAsync(Guid id, UpdateApiSpecRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiSpecResponse> PublishAsync(Guid id, PublishApiSpecRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiSpecResponse> CreateVersionAsync(Guid id, CreateVersionRequest request, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of API specification service
/// </summary>
public class ApiSpecService : IApiSpecService
{
    private readonly ApiviaDbContext _context;
    private readonly IValidationService _validationService;
    private readonly ILogger<ApiSpecService> _logger;
    private readonly IDistributedCache _cache;

    public ApiSpecService(
        ApiviaDbContext context,
        IValidationService validationService,
        ILogger<ApiSpecService> logger,
        IDistributedCache cache)
    {
        _context = context;
        _validationService = validationService;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Create a new API specification
    /// </summary>
    public async Task<ApiSpecResponse> CreateAsync(CreateApiSpecRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify project exists
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.IsActive, cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        // Validate the API specification content
        var validationResult = await _validationService.ValidateAsync(new ValidateApiSpecRequest
        {
            Format = request.Format,
            Content = request.Content
        }, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.Message));
            throw new InvalidOperationException($"API specification validation failed: {errors}");
        }

        // Extract version from metadata
        var version = validationResult.Metadata?.Version ?? "1.0.0";

        // Create API spec
        var apiSpec = new ApiSpec
        {
            Name = request.Name,
            Description = request.Description,
            ProjectId = request.ProjectId,
            Format = request.Format,
            Version = version,
            Content = request.Content,
            Status = ApiSpecStatus.Draft,
            CreatedBy = userId,
            ModifiedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true,
            IsPublished = false
        };

        _context.ApiSpecs.Add(apiSpec);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("API Spec {ApiSpecId} created by user {UserId} in project {ProjectId}",
            apiSpec.Id, userId, request.ProjectId);

        return await GetByIdAsync(apiSpec.Id, cancellationToken);
    }

    /// <summary>
    /// Get API specification by ID with related data
    /// </summary>
    public async Task<ApiSpecResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Try to get from cache first
        var cacheKey = $"apispec:{id}";
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedData))
        {
            try
            {
                var cachedSpec = JsonSerializer.Deserialize<ApiSpecResponse>(cachedData);
                if (cachedSpec != null)
                {
                    _logger.LogDebug("API Spec {ApiSpecId} retrieved from cache", id);
                    return cachedSpec;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached API Spec {ApiSpecId}, fetching from database", id);
                // Continue to fetch from database
            }
        }

        // Fetch from database
        var apiSpec = await _context.ApiSpecs
            .Include(a => a.Project)
            .Include(a => a.Creator)
            .Include(a => a.Modifier)
            .Where(a => a.Id == id && a.IsActive)
            .Select(a => new ApiSpecResponse
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                ProjectId = a.ProjectId,
                ProjectName = a.Project.Name,
                Format = a.Format,
                Version = a.Version,
                Content = a.Content,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                ModifiedAt = a.ModifiedAt,
                CreatedBy = a.CreatedBy,
                CreatedByName = a.Creator.FullName,
                ModifiedBy = a.ModifiedBy,
                ModifiedByName = a.Modifier != null ? a.Modifier.FullName : null,
                IsPublished = a.IsPublished,
                PublishedAt = a.PublishedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (apiSpec == null)
        {
            throw new KeyNotFoundException($"API Spec with ID {id} not found");
        }

        // Cache the result for 15 minutes
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };

        try
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(apiSpec),
                cacheOptions,
                cancellationToken);
            _logger.LogDebug("API Spec {ApiSpecId} cached for 15 minutes", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache API Spec {ApiSpecId}", id);
            // Don't throw - caching is not critical
        }

        return apiSpec;
    }

    /// <summary>
    /// Get paginated list of API specifications with filtering
    /// </summary>
    public async Task<PagedResponse<ApiSpecListItemResponse>> GetPagedAsync(ApiSpecQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.ApiSpecs.Where(a => a.IsActive).AsQueryable();

        // Apply filters
        if (queryParams.ProjectId.HasValue)
        {
            query = query.Where(a => a.ProjectId == queryParams.ProjectId.Value);
        }

        if (queryParams.WorkspaceId.HasValue)
        {
            query = query.Where(a => a.Project.WorkspaceId == queryParams.WorkspaceId.Value);
        }

        if (queryParams.Format.HasValue)
        {
            query = query.Where(a => a.Format == queryParams.Format.Value);
        }

        if (queryParams.Status.HasValue)
        {
            query = query.Where(a => a.Status == queryParams.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var searchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(searchTerm) ||
                (a.Description != null && a.Description.ToLower().Contains(searchTerm)) ||
                a.Version.ToLower().Contains(searchTerm));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = queryParams.SortBy?.ToLower() switch
        {
            "name" => queryParams.SortDescending
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name),
            "version" => queryParams.SortDescending
                ? query.OrderByDescending(a => a.Version)
                : query.OrderBy(a => a.Version),
            "createdat" => queryParams.SortDescending
                ? query.OrderByDescending(a => a.CreatedAt)
                : query.OrderBy(a => a.CreatedAt),
            _ => queryParams.SortDescending
                ? query.OrderByDescending(a => a.ModifiedAt)
                : query.OrderBy(a => a.ModifiedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Include(a => a.Modifier)
            .Select(a => new ApiSpecListItemResponse
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                ProjectId = a.ProjectId,
                Format = a.Format,
                Version = a.Version,
                Status = a.Status,
                IsPublished = a.IsPublished,
                ModifiedAt = a.ModifiedAt,
                ModifiedByName = a.Modifier != null ? a.Modifier.FullName : string.Empty
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<ApiSpecListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// Update an existing API specification
    /// </summary>
    public async Task<ApiSpecResponse> UpdateAsync(Guid id, UpdateApiSpecRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var apiSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (apiSpec == null)
        {
            throw new KeyNotFoundException($"API Spec with ID {id} not found");
        }

        // Don't allow updates to published specs
        if (apiSpec.IsPublished)
        {
            throw new InvalidOperationException("Cannot update a published API specification. Create a new version instead.");
        }

        // Validate the API specification content
        var validationResult = await _validationService.ValidateAsync(new ValidateApiSpecRequest
        {
            Format = apiSpec.Format,
            Content = request.Content
        }, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.Message));
            throw new InvalidOperationException($"API specification validation failed: {errors}");
        }

        // Extract version from metadata if changed
        var newVersion = validationResult.Metadata?.Version ?? apiSpec.Version;

        // Update fields
        apiSpec.Name = request.Name;
        apiSpec.Description = request.Description;
        apiSpec.Content = request.Content;
        apiSpec.Version = newVersion;
        apiSpec.ModifiedBy = userId;
        apiSpec.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await InvalidateCacheAsync(id, cancellationToken);

        _logger.LogInformation("API Spec {ApiSpecId} updated by user {UserId}", apiSpec.Id, userId);

        return await GetByIdAsync(apiSpec.Id, cancellationToken);
    }

    /// <summary>
    /// Soft delete an API specification
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var apiSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (apiSpec == null)
        {
            throw new KeyNotFoundException($"API Spec with ID {id} not found");
        }

        // Don't allow deletion of published specs
        if (apiSpec.IsPublished)
        {
            throw new InvalidOperationException("Cannot delete a published API specification");
        }

        // Soft delete
        apiSpec.IsActive = false;
        apiSpec.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await InvalidateCacheAsync(id, cancellationToken);

        _logger.LogInformation("API Spec {ApiSpecId} soft deleted", apiSpec.Id);

        return true;
    }

    /// <summary>
    /// Publish an API specification
    /// </summary>
    public async Task<ApiSpecResponse> PublishAsync(Guid id, PublishApiSpecRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var apiSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (apiSpec == null)
        {
            throw new KeyNotFoundException($"API Spec with ID {id} not found");
        }

        if (apiSpec.IsPublished)
        {
            throw new InvalidOperationException("API specification is already published");
        }

        // Final validation before publishing
        var validationResult = await _validationService.ValidateAsync(new ValidateApiSpecRequest
        {
            Format = apiSpec.Format,
            Content = apiSpec.Content
        }, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.Message));
            throw new InvalidOperationException($"Cannot publish invalid API specification: {errors}");
        }

        // Update status
        apiSpec.Status = ApiSpecStatus.Published;
        apiSpec.IsPublished = true;
        apiSpec.PublishedAt = DateTime.UtcNow;
        apiSpec.ModifiedBy = userId;
        apiSpec.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await InvalidateCacheAsync(id, cancellationToken);

        _logger.LogInformation("API Spec {ApiSpecId} published by user {UserId}", apiSpec.Id, userId);

        return await GetByIdAsync(apiSpec.Id, cancellationToken);
    }

    /// <summary>
    /// Create a new version of an API specification
    /// </summary>
    public async Task<ApiSpecResponse> CreateVersionAsync(Guid id, CreateVersionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var originalSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (originalSpec == null)
        {
            throw new KeyNotFoundException($"API Spec with ID {id} not found");
        }

        // Create new version based on original
        var newSpec = new ApiSpec
        {
            Name = originalSpec.Name,
            Description = request.VersionNotes ?? originalSpec.Description,
            ProjectId = originalSpec.ProjectId,
            Format = originalSpec.Format,
            Version = request.NewVersion,
            Content = originalSpec.Content,
            Status = ApiSpecStatus.Draft,
            CreatedBy = userId,
            ModifiedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true,
            IsPublished = false,
            ParentVersionId = id
        };

        _context.ApiSpecs.Add(newSpec);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New version {Version} of API Spec {ApiSpecId} created by user {UserId}",
            request.NewVersion, id, userId);

        return await GetByIdAsync(newSpec.Id, cancellationToken);
    }

    /// <summary>
    /// Invalidate cache for an API specification
    /// </summary>
    private async Task InvalidateCacheAsync(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = $"apispec:{id}";
        try
        {
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Cache invalidated for API Spec {ApiSpecId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for API Spec {ApiSpecId}", id);
            // Don't throw - cache invalidation failure is not critical
        }
    }
}
