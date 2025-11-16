using Apivia.Services.Governance.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Apivia.Services.Governance.Services;

/// <summary>
/// Service interface for linkage operations between API specs and data dictionary
/// </summary>
public interface ILinkageService
{
    Task<LinkResponse> CreateLinkAsync(CreateLinkRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<BulkOperationResponse> BulkCreateLinksAsync(BulkCreateLinksRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<LinkResponse> GetLinkByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<LinkListItemResponse>> GetLinksAsync(LinkQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<bool> DeleteLinkAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiSpecLinksResponse> GetApiSpecLinksAsync(Guid apiSpecId, CancellationToken cancellationToken = default);
    Task<DataEntityLinksResponse> GetDataEntityLinksAsync(Guid dataEntityId, CancellationToken cancellationToken = default);
    Task<LinkageAnalysisResponse> AnalyzeLinkageAsync(Guid apiSpecId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of linkage service
/// </summary>
public class LinkageService : ILinkageService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<LinkageService> _logger;

    public LinkageService(ApiviaDbContext context, ILogger<LinkageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a link between API schema element and data dictionary
    /// </summary>
    public async Task<LinkResponse> CreateLinkAsync(CreateLinkRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify API spec exists
        var apiSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == request.ApiSpecId && a.IsActive, cancellationToken);

        if (apiSpec == null)
        {
            throw new InvalidOperationException("API Spec not found");
        }

        // Verify data entity or attribute exists
        if (request.DataEntityId.HasValue)
        {
            var entity = await _context.DataEntities
                .FirstOrDefaultAsync(e => e.Id == request.DataEntityId.Value && e.IsActive, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException("Data Entity not found");
            }
        }

        if (request.DataAttributeId.HasValue)
        {
            var attribute = await _context.DataAttributes
                .FirstOrDefaultAsync(a => a.Id == request.DataAttributeId.Value && a.IsActive, cancellationToken);

            if (attribute == null)
            {
                throw new InvalidOperationException("Data Attribute not found");
            }
        }

        // Check for duplicate link
        var existingLink = await _context.ApiSchemaElements
            .AnyAsync(s => s.ApiSpecId == request.ApiSpecId &&
                          s.SchemaPath == request.SchemaPath &&
                          s.IsActive, cancellationToken);

        if (existingLink)
        {
            throw new InvalidOperationException("A link already exists for this schema path");
        }

        // Create link
        var link = new ApiSchemaElement
        {
            ApiSpecId = request.ApiSpecId,
            SchemaPath = request.SchemaPath,
            DataEntityId = request.DataEntityId,
            DataAttributeId = request.DataAttributeId,
            LinkedAt = DateTime.UtcNow,
            LinkedBy = userId,
            AutoSynced = request.AutoSync,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ApiSchemaElements.Add(link);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Link {LinkId} created between API Spec {ApiSpecId} and {Target} by user {UserId}",
            link.Id, request.ApiSpecId,
            request.DataEntityId.HasValue ? $"Entity {request.DataEntityId}" : $"Attribute {request.DataAttributeId}",
            userId);

        return await GetLinkByIdAsync(link.Id, cancellationToken);
    }

    /// <summary>
    /// Bulk create links
    /// </summary>
    public async Task<BulkOperationResponse> BulkCreateLinksAsync(BulkCreateLinksRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var response = new BulkOperationResponse();

        // Verify API spec exists
        var apiSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == request.ApiSpecId && a.IsActive, cancellationToken);

        if (apiSpec == null)
        {
            throw new InvalidOperationException("API Spec not found");
        }

        // Get existing schema paths for duplicate check
        var existingPaths = await _context.ApiSchemaElements
            .Where(s => s.ApiSpecId == request.ApiSpecId && s.IsActive)
            .Select(s => s.SchemaPath.ToLower())
            .ToListAsync(cancellationToken);

        var linksToCreate = new List<ApiSchemaElement>();

        foreach (var linkRequest in request.Links)
        {
            try
            {
                // Check for duplicates
                if (existingPaths.Contains(linkRequest.SchemaPath.ToLower()) ||
                    linksToCreate.Any(l => l.SchemaPath.Equals(linkRequest.SchemaPath, StringComparison.OrdinalIgnoreCase)))
                {
                    response.FailureCount++;
                    response.Errors.Add($"Duplicate schema path: {linkRequest.SchemaPath}");
                    continue;
                }

                // Verify entity or attribute exists
                if (linkRequest.DataEntityId.HasValue)
                {
                    var entityExists = await _context.DataEntities
                        .AnyAsync(e => e.Id == linkRequest.DataEntityId.Value && e.IsActive, cancellationToken);

                    if (!entityExists)
                    {
                        response.FailureCount++;
                        response.Errors.Add($"Data Entity {linkRequest.DataEntityId} not found for path {linkRequest.SchemaPath}");
                        continue;
                    }
                }

                if (linkRequest.DataAttributeId.HasValue)
                {
                    var attributeExists = await _context.DataAttributes
                        .AnyAsync(a => a.Id == linkRequest.DataAttributeId.Value && a.IsActive, cancellationToken);

                    if (!attributeExists)
                    {
                        response.FailureCount++;
                        response.Errors.Add($"Data Attribute {linkRequest.DataAttributeId} not found for path {linkRequest.SchemaPath}");
                        continue;
                    }
                }

                var link = new ApiSchemaElement
                {
                    ApiSpecId = request.ApiSpecId,
                    SchemaPath = linkRequest.SchemaPath,
                    DataEntityId = linkRequest.DataEntityId,
                    DataAttributeId = linkRequest.DataAttributeId,
                    LinkedAt = DateTime.UtcNow,
                    LinkedBy = userId,
                    AutoSynced = linkRequest.AutoSync,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    IsActive = true
                };

                linksToCreate.Add(link);
                response.CreatedIds.Add(link.Id);
                response.SuccessCount++;
            }
            catch (Exception ex)
            {
                response.FailureCount++;
                response.Errors.Add($"Failed to create link for path {linkRequest.SchemaPath}: {ex.Message}");
                _logger.LogError(ex, "Error creating link for schema path {SchemaPath}", linkRequest.SchemaPath);
            }
        }

        if (linksToCreate.Any())
        {
            _context.ApiSchemaElements.AddRange(linksToCreate);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bulk created {Count} links for API Spec {ApiSpecId} by user {UserId}",
                linksToCreate.Count, request.ApiSpecId, userId);
        }

        return response;
    }

    /// <summary>
    /// Get link by ID
    /// </summary>
    public async Task<LinkResponse> GetLinkByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var link = await _context.ApiSchemaElements
            .Include(s => s.ApiSpec)
            .Include(s => s.DataEntity)
            .Include(s => s.DataAttribute)
            .Include(s => s.Linker)
            .Where(s => s.Id == id && s.IsActive)
            .Select(s => new LinkResponse
            {
                Id = s.Id,
                ApiSpecId = s.ApiSpecId,
                ApiSpecName = s.ApiSpec.Name,
                SchemaPath = s.SchemaPath,
                DataEntityId = s.DataEntityId,
                DataEntityName = s.DataEntity != null ? s.DataEntity.Name : null,
                DataAttributeId = s.DataAttributeId,
                DataAttributeName = s.DataAttribute != null ? s.DataAttribute.Name : null,
                AutoSynced = s.AutoSynced,
                LinkedAt = s.LinkedAt,
                LinkedBy = s.LinkedBy,
                LinkedByName = s.Linker.FullName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (link == null)
        {
            throw new KeyNotFoundException($"Link with ID {id} not found");
        }

        return link;
    }

    /// <summary>
    /// Get paginated list of links with filtering
    /// </summary>
    public async Task<PagedResponse<LinkListItemResponse>> GetLinksAsync(LinkQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.ApiSchemaElements.Where(s => s.IsActive).AsQueryable();

        // Apply filters
        if (queryParams.ApiSpecId.HasValue)
        {
            query = query.Where(s => s.ApiSpecId == queryParams.ApiSpecId.Value);
        }

        if (queryParams.DataEntityId.HasValue)
        {
            query = query.Where(s => s.DataEntityId == queryParams.DataEntityId.Value);
        }

        if (queryParams.DataAttributeId.HasValue)
        {
            query = query.Where(s => s.DataAttributeId == queryParams.DataAttributeId.Value);
        }

        if (queryParams.ProjectId.HasValue)
        {
            query = query.Where(s => s.ApiSpec.ProjectId == queryParams.ProjectId.Value);
        }

        if (queryParams.DictionaryId.HasValue)
        {
            query = query.Where(s =>
                (s.DataEntity != null && s.DataEntity.DictionaryId == queryParams.DictionaryId.Value) ||
                (s.DataAttribute != null && s.DataAttribute.Entity.DictionaryId == queryParams.DictionaryId.Value));
        }

        if (queryParams.AutoSynced.HasValue)
        {
            query = query.Where(s => s.AutoSynced == queryParams.AutoSynced.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(s => s.LinkedAt)
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Include(s => s.ApiSpec)
            .Include(s => s.DataEntity)
            .Include(s => s.DataAttribute)
            .Select(s => new LinkListItemResponse
            {
                Id = s.Id,
                ApiSpecId = s.ApiSpecId,
                ApiSpecName = s.ApiSpec.Name,
                SchemaPath = s.SchemaPath,
                DataEntityId = s.DataEntityId,
                DataEntityName = s.DataEntity != null ? s.DataEntity.Name : null,
                DataAttributeId = s.DataAttributeId,
                DataAttributeName = s.DataAttribute != null ? s.DataAttribute.Name : null,
                LinkedAt = s.LinkedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<LinkListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// Delete a link
    /// </summary>
    public async Task<bool> DeleteLinkAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var link = await _context.ApiSchemaElements
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, cancellationToken);

        if (link == null)
        {
            throw new KeyNotFoundException($"Link with ID {id} not found");
        }

        // Soft delete
        link.IsActive = false;
        link.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Link {LinkId} soft deleted", link.Id);

        return true;
    }

    /// <summary>
    /// Get all links for an API spec
    /// </summary>
    public async Task<ApiSpecLinksResponse> GetApiSpecLinksAsync(Guid apiSpecId, CancellationToken cancellationToken = default)
    {
        var apiSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == apiSpecId && a.IsActive, cancellationToken);

        if (apiSpec == null)
        {
            throw new KeyNotFoundException($"API Spec with ID {apiSpecId} not found");
        }

        var links = await _context.ApiSchemaElements
            .Include(s => s.DataEntity)
            .Include(s => s.DataAttribute)
            .Include(s => s.Linker)
            .Where(s => s.ApiSpecId == apiSpecId && s.IsActive)
            .Select(s => new LinkResponse
            {
                Id = s.Id,
                ApiSpecId = s.ApiSpecId,
                ApiSpecName = apiSpec.Name,
                SchemaPath = s.SchemaPath,
                DataEntityId = s.DataEntityId,
                DataEntityName = s.DataEntity != null ? s.DataEntity.Name : null,
                DataAttributeId = s.DataAttributeId,
                DataAttributeName = s.DataAttribute != null ? s.DataAttribute.Name : null,
                AutoSynced = s.AutoSynced,
                LinkedAt = s.LinkedAt,
                LinkedBy = s.LinkedBy,
                LinkedByName = s.Linker.FullName
            })
            .ToListAsync(cancellationToken);

        return new ApiSpecLinksResponse
        {
            ApiSpecId = apiSpecId,
            ApiSpecName = apiSpec.Name,
            Links = links,
            TotalLinks = links.Count,
            EntityLinksCount = links.Count(l => l.DataEntityId.HasValue),
            AttributeLinksCount = links.Count(l => l.DataAttributeId.HasValue)
        };
    }

    /// <summary>
    /// Get all links for a data entity
    /// </summary>
    public async Task<DataEntityLinksResponse> GetDataEntityLinksAsync(Guid dataEntityId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DataEntities
            .FirstOrDefaultAsync(e => e.Id == dataEntityId && e.IsActive, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Data Entity with ID {dataEntityId} not found");
        }

        var links = await _context.ApiSchemaElements
            .Include(s => s.ApiSpec)
            .Include(s => s.Linker)
            .Where(s => s.DataEntityId == dataEntityId && s.IsActive)
            .Select(s => new LinkResponse
            {
                Id = s.Id,
                ApiSpecId = s.ApiSpecId,
                ApiSpecName = s.ApiSpec.Name,
                SchemaPath = s.SchemaPath,
                DataEntityId = s.DataEntityId,
                DataEntityName = entity.Name,
                DataAttributeId = s.DataAttributeId,
                DataAttributeName = null,
                AutoSynced = s.AutoSynced,
                LinkedAt = s.LinkedAt,
                LinkedBy = s.LinkedBy,
                LinkedByName = s.Linker.FullName
            })
            .ToListAsync(cancellationToken);

        var linkedApiSpecs = links.Select(l => l.ApiSpecName).Distinct().ToList();

        return new DataEntityLinksResponse
        {
            DataEntityId = dataEntityId,
            DataEntityName = entity.Name,
            Links = links,
            TotalLinks = links.Count,
            LinkedApiSpecs = linkedApiSpecs
        };
    }

    /// <summary>
    /// Analyze API spec and suggest potential links
    /// </summary>
    public async Task<LinkageAnalysisResponse> AnalyzeLinkageAsync(Guid apiSpecId, CancellationToken cancellationToken = default)
    {
        var apiSpec = await _context.ApiSpecs
            .Include(a => a.Project)
                .ThenInclude(p => p.Workspace)
            .FirstOrDefaultAsync(a => a.Id == apiSpecId && a.IsActive, cancellationToken);

        if (apiSpec == null)
        {
            throw new KeyNotFoundException($"API Spec with ID {apiSpecId} not found");
        }

        // Get all entities and attributes from the same workspace
        var workspaceId = apiSpec.Project.WorkspaceId;
        var entities = await _context.DataEntities
            .Include(e => e.Attributes)
            .Where(e => e.Dictionary.WorkspaceId == workspaceId && e.IsActive)
            .ToListAsync(cancellationToken);

        // Parse API spec content to extract schema elements
        var suggestions = new List<SuggestedLink>();

        try
        {
            var content = apiSpec.Content;
            var jsonContent = content.Trim().StartsWith("{")
                ? content
                : ConvertYamlToJson(content);

            var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            // Analyze schemas/components
            if (root.TryGetProperty("components", out var components) &&
                components.TryGetProperty("schemas", out var schemas))
            {
                foreach (var schema in schemas.EnumerateObject())
                {
                    var schemaName = schema.Name;
                    var schemaPath = $"#/components/schemas/{schemaName}";

                    // Find matching entity by name similarity
                    var matchingEntity = FindBestMatchingEntity(schemaName, entities);
                    if (matchingEntity != null)
                    {
                        suggestions.Add(new SuggestedLink
                        {
                            SchemaPath = schemaPath,
                            SchemaType = "object",
                            SuggestedEntityId = matchingEntity.Item1.Id,
                            SuggestedEntityName = matchingEntity.Item1.Name,
                            ConfidenceScore = matchingEntity.Item2,
                            Reason = $"Name similarity match: '{schemaName}' ≈ '{matchingEntity.Item1.Name}'"
                        });
                    }

                    // Analyze properties for attribute matching
                    if (schema.Value.TryGetProperty("properties", out var properties))
                    {
                        foreach (var property in properties.EnumerateObject())
                        {
                            var propertyName = property.Name;
                            var propertyPath = $"{schemaPath}/properties/{propertyName}";

                            // Find matching attribute
                            var matchingAttribute = FindBestMatchingAttribute(propertyName, entities);
                            if (matchingAttribute != null && matchingAttribute.Item2 > 0.7)
                            {
                                suggestions.Add(new SuggestedLink
                                {
                                    SchemaPath = propertyPath,
                                    SchemaType = GetPropertyType(property.Value),
                                    SuggestedAttributeId = matchingAttribute.Item1.Id,
                                    SuggestedAttributeName = matchingAttribute.Item1.Name,
                                    ConfidenceScore = matchingAttribute.Item2,
                                    Reason = $"Property name match: '{propertyName}' ≈ '{matchingAttribute.Item1.Name}'"
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to analyze API spec {ApiSpecId} for linkage suggestions", apiSpecId);
        }

        return new LinkageAnalysisResponse
        {
            ApiSpecId = apiSpecId,
            SuggestedLinks = suggestions.OrderByDescending(s => s.ConfidenceScore).ToList(),
            TotalSuggestions = suggestions.Count
        };
    }

    private string ConvertYamlToJson(string yaml)
    {
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<object>(yaml);
        return JsonSerializer.Serialize(yamlObject);
    }

    private (DataEntity, double)? FindBestMatchingEntity(string schemaName, List<DataEntity> entities)
    {
        var normalizedSchemaName = NormalizeName(schemaName);
        var bestMatch = entities
            .Select(e => (Entity: e, Score: CalculateSimilarity(normalizedSchemaName, NormalizeName(e.Name))))
            .Where(x => x.Score > 0.6)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return bestMatch.Score > 0 ? (bestMatch.Entity, bestMatch.Score) : null;
    }

    private (DataAttribute, double)? FindBestMatchingAttribute(string propertyName, List<DataEntity> entities)
    {
        var normalizedPropertyName = NormalizeName(propertyName);
        var allAttributes = entities.SelectMany(e => e.Attributes).Where(a => a.IsActive).ToList();
        var bestMatch = allAttributes
            .Select(a => (Attribute: a, Score: CalculateSimilarity(normalizedPropertyName, NormalizeName(a.Name))))
            .Where(x => x.Score > 0.6)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return bestMatch.Score > 0 ? (bestMatch.Attribute, bestMatch.Score) : null;
    }

    private string NormalizeName(string name)
    {
        // Remove underscores, convert to lowercase, remove common prefixes/suffixes
        return name.ToLower()
            .Replace("_", "")
            .Replace("-", "")
            .Replace("dto", "")
            .Replace("model", "")
            .Replace("request", "")
            .Replace("response", "")
            .Trim();
    }

    private double CalculateSimilarity(string s1, string s2)
    {
        if (s1 == s2) return 1.0;
        if (s1.Contains(s2) || s2.Contains(s1)) return 0.9;

        // Levenshtein distance-based similarity
        var maxLen = Math.Max(s1.Length, s2.Length);
        if (maxLen == 0) return 1.0;

        var distance = LevenshteinDistance(s1, s2);
        return 1.0 - (double)distance / maxLen;
    }

    private int LevenshteinDistance(string s1, string s2)
    {
        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }

    private string GetPropertyType(JsonElement property)
    {
        if (property.TryGetProperty("type", out var type))
        {
            return type.GetString() ?? "unknown";
        }
        return "unknown";
    }
}

/// <summary>
/// Bulk operation response
/// </summary>
public class BulkOperationResponse
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Guid> CreatedIds { get; set; } = new();
}

/// <summary>
/// Paginated response
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
