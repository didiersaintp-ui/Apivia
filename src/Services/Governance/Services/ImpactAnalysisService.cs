using Apivia.Services.Governance.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Apivia.Shared.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Apivia.Services.Governance.Services;

/// <summary>
/// Service interface for impact analysis operations
/// </summary>
public interface IImpactAnalysisService
{
    Task<ImpactAnalysisResponse> CreateAsync(CreateImpactAnalysisRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ImpactAnalysisResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ImpactAnalysisListItemResponse>> GetPagedAsync(ImpactAnalysisQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<ImpactAnalysisResponse> ResolveAsync(Guid id, ResolveImpactAnalysisRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ImpactPreviewResponse> PreviewImpactAsync(PreviewImpactRequest request, CancellationToken cancellationToken = default);
    Task<ImpactAnalysisStatsResponse> GetStatsAsync(Guid? dictionaryId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of impact analysis service
/// </summary>
public class ImpactAnalysisService : IImpactAnalysisService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<ImpactAnalysisService> _logger;

    public ImpactAnalysisService(ApiviaDbContext context, ILogger<ImpactAnalysisService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new impact analysis
    /// </summary>
    public async Task<ImpactAnalysisResponse> CreateAsync(CreateImpactAnalysisRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify entity or attribute exists
        string targetName = string.Empty;

        if (request.DataEntityId.HasValue)
        {
            var entity = await _context.DataEntities
                .FirstOrDefaultAsync(e => e.Id == request.DataEntityId.Value && e.IsActive, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException("Data Entity not found");
            }
            targetName = entity.Name;
        }

        if (request.DataAttributeId.HasValue)
        {
            var attribute = await _context.DataAttributes
                .FirstOrDefaultAsync(a => a.Id == request.DataAttributeId.Value && a.IsActive, cancellationToken);

            if (attribute == null)
            {
                throw new InvalidOperationException("Data Attribute not found");
            }
            targetName = attribute.Name;
        }

        // Analyze affected APIs
        var affectedApis = await AnalyzeAffectedApisAsync(
            request.DataEntityId,
            request.DataAttributeId,
            cancellationToken);

        // Calculate overall risk level
        var overallRiskLevel = CalculateRiskLevel(affectedApis, request.ChangeType);

        // Create impact analysis
        var analysis = new ImpactAnalysis
        {
            DataEntityId = request.DataEntityId,
            DataAttributeId = request.DataAttributeId,
            ChangeType = request.ChangeType,
            ProposedChanges = request.ProposedChanges,
            AffectedApis = JsonSerializer.Serialize(affectedApis),
            OverallRiskLevel = overallRiskLevel,
            Status = ImpactAnalysisStatus.Pending,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ImpactAnalyses.Add(analysis);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Impact Analysis {AnalysisId} created for {Target} with {RiskLevel} risk by user {UserId}",
            analysis.Id, targetName, overallRiskLevel, userId);

        return await GetByIdAsync(analysis.Id, cancellationToken);
    }

    /// <summary>
    /// Get impact analysis by ID
    /// </summary>
    public async Task<ImpactAnalysisResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var analysis = await _context.ImpactAnalyses
            .Include(i => i.DataEntity)
            .Include(i => i.DataAttribute)
            .Include(i => i.Creator)
            .Include(i => i.Resolver)
            .Where(i => i.Id == id && i.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (analysis == null)
        {
            throw new KeyNotFoundException($"Impact Analysis with ID {id} not found");
        }

        var affectedApis = string.IsNullOrEmpty(analysis.AffectedApis)
            ? new List<AffectedApiInfo>()
            : JsonSerializer.Deserialize<List<AffectedApiInfo>>(analysis.AffectedApis) ?? new List<AffectedApiInfo>();

        return new ImpactAnalysisResponse
        {
            Id = analysis.Id,
            DataEntityId = analysis.DataEntityId,
            DataEntityName = analysis.DataEntity?.Name,
            DataAttributeId = analysis.DataAttributeId,
            DataAttributeName = analysis.DataAttribute?.Name,
            ChangeType = analysis.ChangeType,
            ProposedChanges = analysis.ProposedChanges,
            AffectedApis = affectedApis,
            OverallRiskLevel = analysis.OverallRiskLevel,
            Status = analysis.Status,
            CreatedAt = analysis.CreatedAt,
            CreatedBy = analysis.CreatedBy,
            CreatedByName = analysis.Creator.FullName,
            ResolvedAt = analysis.ResolvedAt,
            ResolvedBy = analysis.ResolvedBy,
            ResolvedByName = analysis.Resolver?.FullName
        };
    }

    /// <summary>
    /// Get paginated list of impact analyses
    /// </summary>
    public async Task<PagedResponse<ImpactAnalysisListItemResponse>> GetPagedAsync(ImpactAnalysisQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.ImpactAnalyses.Where(i => i.IsActive).AsQueryable();

        // Apply filters
        if (queryParams.DataEntityId.HasValue)
        {
            query = query.Where(i => i.DataEntityId == queryParams.DataEntityId.Value);
        }

        if (queryParams.DataAttributeId.HasValue)
        {
            query = query.Where(i => i.DataAttributeId == queryParams.DataAttributeId.Value);
        }

        if (queryParams.DictionaryId.HasValue)
        {
            query = query.Where(i =>
                (i.DataEntity != null && i.DataEntity.DictionaryId == queryParams.DictionaryId.Value) ||
                (i.DataAttribute != null && i.DataAttribute.Entity.DictionaryId == queryParams.DictionaryId.Value));
        }

        if (queryParams.ChangeType.HasValue)
        {
            query = query.Where(i => i.ChangeType == queryParams.ChangeType.Value);
        }

        if (queryParams.RiskLevel.HasValue)
        {
            query = query.Where(i => i.OverallRiskLevel == queryParams.RiskLevel.Value);
        }

        if (queryParams.Status.HasValue)
        {
            query = query.Where(i => i.Status == queryParams.Status.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = queryParams.SortBy?.ToLower() switch
        {
            "risklevel" => queryParams.SortDescending
                ? query.OrderByDescending(i => i.OverallRiskLevel)
                : query.OrderBy(i => i.OverallRiskLevel),
            "status" => queryParams.SortDescending
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            _ => queryParams.SortDescending
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Include(i => i.DataEntity)
            .Include(i => i.DataAttribute)
            .Select(i => new ImpactAnalysisListItemResponse
            {
                Id = i.Id,
                DataEntityId = i.DataEntityId,
                DataEntityName = i.DataEntity != null ? i.DataEntity.Name : null,
                DataAttributeId = i.DataAttributeId,
                DataAttributeName = i.DataAttribute != null ? i.DataAttribute.Name : null,
                ChangeType = i.ChangeType,
                OverallRiskLevel = i.OverallRiskLevel,
                Status = i.Status,
                AffectedApiCount = !string.IsNullOrEmpty(i.AffectedApis)
                    ? JsonSerializer.Deserialize<List<AffectedApiInfo>>(i.AffectedApis)!.Count
                    : 0,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<ImpactAnalysisListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// Resolve an impact analysis
    /// </summary>
    public async Task<ImpactAnalysisResponse> ResolveAsync(Guid id, ResolveImpactAnalysisRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var analysis = await _context.ImpactAnalyses
            .FirstOrDefaultAsync(i => i.Id == id && i.IsActive, cancellationToken);

        if (analysis == null)
        {
            throw new KeyNotFoundException($"Impact Analysis with ID {id} not found");
        }

        if (analysis.Status == ImpactAnalysisStatus.Resolved)
        {
            throw new InvalidOperationException("Impact Analysis is already resolved");
        }

        analysis.Status = ImpactAnalysisStatus.Resolved;
        analysis.ResolvedAt = DateTime.UtcNow;
        analysis.ResolvedBy = userId;
        analysis.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Impact Analysis {AnalysisId} resolved by user {UserId}", analysis.Id, userId);

        return await GetByIdAsync(analysis.Id, cancellationToken);
    }

    /// <summary>
    /// Preview impact without creating analysis (dry-run)
    /// Performs real analysis but does not persist results to database
    /// </summary>
    public async Task<ImpactPreviewResponse> PreviewImpactAsync(PreviewImpactRequest request, CancellationToken cancellationToken = default)
    {
        string targetName = string.Empty;

        if (request.DataEntityId.HasValue)
        {
            var entity = await _context.DataEntities
                .FirstOrDefaultAsync(e => e.Id == request.DataEntityId.Value && e.IsActive, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException("Data Entity not found");
            }
            targetName = entity.Name;
        }

        if (request.DataAttributeId.HasValue)
        {
            var attribute = await _context.DataAttributes
                .FirstOrDefaultAsync(a => a.Id == request.DataAttributeId.Value && a.IsActive, cancellationToken);

            if (attribute == null)
            {
                throw new InvalidOperationException("Data Attribute not found");
            }
            targetName = attribute.Name;
        }

        // Analyze affected APIs
        var affectedApis = await AnalyzeAffectedApisAsync(
            request.DataEntityId,
            request.DataAttributeId,
            cancellationToken);

        // Calculate risk level
        var riskLevel = CalculateRiskLevel(affectedApis, request.ChangeType);

        // Generate recommendations
        var recommendations = GenerateRecommendations(affectedApis, request.ChangeType, riskLevel);

        return new ImpactPreviewResponse
        {
            DataEntityId = request.DataEntityId,
            DataEntityName = request.DataEntityId.HasValue ? targetName : null,
            DataAttributeId = request.DataAttributeId,
            DataAttributeName = request.DataAttributeId.HasValue ? targetName : null,
            ChangeType = request.ChangeType,
            AffectedApis = affectedApis,
            CalculatedRiskLevel = riskLevel,
            TotalAffectedApis = affectedApis.Count,
            PublishedApisAffected = affectedApis.Count(a => a.IsPublished),
            Recommendations = recommendations
        };
    }

    /// <summary>
    /// Get statistics for impact analyses
    /// </summary>
    public async Task<ImpactAnalysisStatsResponse> GetStatsAsync(Guid? dictionaryId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ImpactAnalyses.Where(i => i.IsActive).AsQueryable();

        if (dictionaryId.HasValue)
        {
            query = query.Where(i =>
                (i.DataEntity != null && i.DataEntity.DictionaryId == dictionaryId.Value) ||
                (i.DataAttribute != null && i.DataAttribute.Entity.DictionaryId == dictionaryId.Value));
        }

        var totalAnalyses = await query.CountAsync(cancellationToken);
        var pendingAnalyses = await query.CountAsync(i => i.Status == ImpactAnalysisStatus.Pending, cancellationToken);
        var inProgressAnalyses = await query.CountAsync(i => i.Status == ImpactAnalysisStatus.InProgress, cancellationToken);
        var resolvedAnalyses = await query.CountAsync(i => i.Status == ImpactAnalysisStatus.Resolved, cancellationToken);

        var byRiskLevel = await query
            .GroupBy(i => i.OverallRiskLevel)
            .Select(g => new { RiskLevel = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var byChangeType = await query
            .GroupBy(i => i.ChangeType)
            .Select(g => new { ChangeType = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new ImpactAnalysisStatsResponse
        {
            TotalAnalyses = totalAnalyses,
            PendingAnalyses = pendingAnalyses,
            InProgressAnalyses = inProgressAnalyses,
            ResolvedAnalyses = resolvedAnalyses,
            AnalysesByRiskLevel = byRiskLevel.ToDictionary(x => x.RiskLevel, x => x.Count),
            AnalysesByChangeType = byChangeType.ToDictionary(x => x.ChangeType, x => x.Count)
        };
    }

    /// <summary>
    /// Analyze which APIs are affected by changes to entity or attribute
    /// </summary>
    private async Task<List<AffectedApiInfo>> AnalyzeAffectedApisAsync(
        Guid? dataEntityId,
        Guid? dataAttributeId,
        CancellationToken cancellationToken)
    {
        var affectedApis = new List<AffectedApiInfo>();

        // Get all links related to the entity or attribute
        var linksQuery = _context.ApiSchemaElements.Where(s => s.IsActive);

        if (dataEntityId.HasValue)
        {
            linksQuery = linksQuery.Where(s => s.DataEntityId == dataEntityId.Value);
        }

        if (dataAttributeId.HasValue)
        {
            linksQuery = linksQuery.Where(s => s.DataAttributeId == dataAttributeId.Value);
        }

        var links = await linksQuery
            .Include(s => s.ApiSpec)
                .ThenInclude(a => a.Project)
            .ToListAsync(cancellationToken);

        // Group by API Spec
        var apiSpecGroups = links.GroupBy(s => s.ApiSpecId);

        foreach (var group in apiSpecGroups)
        {
            var apiSpec = group.First().ApiSpec;
            var affectedPaths = group.Select(s => s.SchemaPath).Distinct().ToList();

            // Calculate risk level for this API
            var riskLevel = CalculateApiRiskLevel(apiSpec, affectedPaths.Count);

            affectedApis.Add(new AffectedApiInfo
            {
                ApiSpecId = apiSpec.Id,
                ApiSpecName = apiSpec.Name,
                ApiSpecVersion = apiSpec.Version,
                ProjectId = apiSpec.ProjectId,
                ProjectName = apiSpec.Project.Name,
                AffectedPaths = affectedPaths,
                RiskLevel = riskLevel,
                IsPublished = apiSpec.IsPublished
            });
        }

        return affectedApis;
    }

    /// <summary>
    /// Calculate overall risk level based on affected APIs and change type
    /// </summary>
    private RiskLevel CalculateRiskLevel(List<AffectedApiInfo> affectedApis, ChangeType changeType)
    {
        if (!affectedApis.Any())
        {
            return RiskLevel.Low;
        }

        // Factors that increase risk:
        // 1. Number of affected APIs
        // 2. Published APIs are higher risk
        // 3. Change type (Delete > TypeChange > Rename > AddField/Deprecate)
        // 4. Individual API risk levels

        var publishedApisCount = affectedApis.Count(a => a.IsPublished);
        var criticalApisCount = affectedApis.Count(a => a.RiskLevel == RiskLevel.Critical);
        var highRiskApisCount = affectedApis.Count(a => a.RiskLevel == RiskLevel.High);

        // Change type multiplier
        var changeTypeMultiplier = changeType switch
        {
            ChangeType.Delete => 3.0,
            ChangeType.TypeChange => 2.5,
            ChangeType.Rename => 2.0,
            ChangeType.AddConstraint => 1.5,
            ChangeType.Deprecate => 1.2,
            ChangeType.AddField => 1.0,
            _ => 1.0
        };

        // Calculate risk score
        var riskScore = 0.0;
        riskScore += affectedApis.Count * 1.0;
        riskScore += publishedApisCount * 2.0;
        riskScore += criticalApisCount * 3.0;
        riskScore += highRiskApisCount * 2.0;
        riskScore *= changeTypeMultiplier;

        // Determine overall risk level
        if (riskScore >= 20 || criticalApisCount > 0)
            return RiskLevel.Critical;
        if (riskScore >= 10 || highRiskApisCount > 0 || publishedApisCount > 2)
            return RiskLevel.High;
        if (riskScore >= 5 || publishedApisCount > 0)
            return RiskLevel.Medium;

        return RiskLevel.Low;
    }

    /// <summary>
    /// Calculate risk level for individual API
    /// </summary>
    private RiskLevel CalculateApiRiskLevel(ApiSpec apiSpec, int affectedPathsCount)
    {
        // Published APIs have higher risk
        if (apiSpec.IsPublished && affectedPathsCount > 5)
            return RiskLevel.Critical;
        if (apiSpec.IsPublished && affectedPathsCount > 2)
            return RiskLevel.High;
        if (apiSpec.IsPublished)
            return RiskLevel.Medium;

        // Draft APIs have lower risk
        if (affectedPathsCount > 10)
            return RiskLevel.High;
        if (affectedPathsCount > 5)
            return RiskLevel.Medium;

        return RiskLevel.Low;
    }

    /// <summary>
    /// Generate recommendations based on impact analysis
    /// </summary>
    private List<string> GenerateRecommendations(
        List<AffectedApiInfo> affectedApis,
        ChangeType changeType,
        RiskLevel riskLevel)
    {
        var recommendations = new List<string>();

        if (!affectedApis.Any())
        {
            recommendations.Add("No APIs are affected by this change. Proceed with caution.");
            return recommendations;
        }

        // General recommendations based on risk level
        if (riskLevel == RiskLevel.Critical)
        {
            recommendations.Add("⚠️ CRITICAL IMPACT: This change affects published APIs. Consider creating a new API version instead.");
            recommendations.Add("Schedule a maintenance window and notify all API consumers before implementing.");
        }
        else if (riskLevel == RiskLevel.High)
        {
            recommendations.Add("⚠️ HIGH IMPACT: Review all affected APIs carefully before proceeding.");
            recommendations.Add("Consider implementing this change in a phased approach.");
        }

        // Change type specific recommendations
        switch (changeType)
        {
            case ChangeType.Delete:
                recommendations.Add("For deletion: First deprecate the field/entity, then remove in a future version.");
                recommendations.Add("Provide migration guide for API consumers.");
                break;

            case ChangeType.TypeChange:
                recommendations.Add("Type changes are breaking changes. Update API version number.");
                recommendations.Add("Ensure all API consumers can handle the new type.");
                break;

            case ChangeType.Rename:
                recommendations.Add("Consider keeping the old name as an alias for backward compatibility.");
                recommendations.Add("Update all API documentation to reflect the new name.");
                break;

            case ChangeType.AddConstraint:
                recommendations.Add("Adding constraints may break existing integrations. Test thoroughly.");
                recommendations.Add("Consider making constraints optional initially.");
                break;
        }

        // API-specific recommendations
        var publishedApis = affectedApis.Where(a => a.IsPublished).ToList();
        if (publishedApis.Any())
        {
            recommendations.Add($"Affected published APIs ({publishedApis.Count}): {string.Join(", ", publishedApis.Select(a => a.ApiSpecName))}");
            recommendations.Add("Create change notifications for all affected API consumers.");
        }

        return recommendations;
    }
}
