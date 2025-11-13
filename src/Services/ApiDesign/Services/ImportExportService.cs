using Apivia.Services.ApiDesign.DTOs;
using Apivia.Shared.Data.Enums;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Apivia.Services.ApiDesign.Services;

/// <summary>
/// Service interface for importing and exporting API specifications
/// </summary>
public interface IImportExportService
{
    Task<ImportResponse> ImportAsync(ImportApiSpecRequest request, CancellationToken cancellationToken = default);
    Task<ExportResponse> ExportAsync(ExportApiSpecRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of import/export service
/// </summary>
public class ImportExportService : IImportExportService
{
    private readonly IApiSpecService _apiSpecService;
    private readonly ILogger<ImportExportService> _logger;
    private readonly IDeserializer _yamlDeserializer;
    private readonly ISerializer _yamlSerializer;

    public ImportExportService(
        IApiSpecService apiSpecService,
        ILogger<ImportExportService> logger)
    {
        _apiSpecService = apiSpecService;
        _logger = logger;

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Import an API specification from various formats
    /// </summary>
    public async Task<ImportResponse> ImportAsync(ImportApiSpecRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            string processedContent;
            var conversionRequired = false;
            string? conversionNotes = null;
            var targetFormat = ApiSpecFormat.OpenApiV3;

            switch (request.SourceFormat)
            {
                case ImportFormat.OpenApiV3:
                    processedContent = request.Content;
                    targetFormat = ApiSpecFormat.OpenApiV3;
                    break;

                case ImportFormat.OpenApiV2:
                    // For now, store as-is, but mark for conversion
                    processedContent = request.Content;
                    targetFormat = ApiSpecFormat.OpenApiV2;
                    conversionRequired = true;
                    conversionNotes = "OpenAPI 2.0 imported successfully. Consider converting to OpenAPI 3.0 for better features.";
                    break;

                case ImportFormat.PostmanCollection:
                    // Convert Postman Collection to OpenAPI 3.0
                    processedContent = ConvertPostmanToOpenApi(request.Content);
                    targetFormat = ApiSpecFormat.OpenApiV3;
                    conversionRequired = true;
                    conversionNotes = "Postman Collection converted to OpenAPI 3.0. Please review and adjust as needed.";
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported import format: {request.SourceFormat}");
            }

            // Create the API spec using the service
            var createRequest = new CreateApiSpecRequest
            {
                Name = request.Name,
                ProjectId = request.ProjectId,
                Format = targetFormat,
                Content = processedContent
            };

            // Note: We need userId here - this will be passed from the controller
            // For now, using a placeholder that will be replaced in the controller
            var apiSpecResponse = await _apiSpecService.CreateAsync(createRequest, Guid.Empty, cancellationToken);

            _logger.LogInformation("API Spec imported from {SourceFormat}: {ApiSpecId}",
                request.SourceFormat, apiSpecResponse.Id);

            return new ImportResponse
            {
                ApiSpecId = apiSpecResponse.Id,
                Name = apiSpecResponse.Name,
                ConversionRequired = conversionRequired,
                ConversionNotes = conversionNotes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import API specification from {SourceFormat}", request.SourceFormat);
            throw new InvalidOperationException($"Import failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Export an API specification to various formats
    /// </summary>
    public async Task<ExportResponse> ExportAsync(ExportApiSpecRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the API spec
            var apiSpec = await _apiSpecService.GetByIdAsync(request.ApiSpecId, cancellationToken);

            string content;
            string contentType;
            string fileName;

            switch (request.TargetFormat)
            {
                case ExportFormat.Yaml:
                    content = ConvertToYaml(apiSpec.Content);
                    contentType = "application/x-yaml";
                    fileName = $"{SanitizeFileName(apiSpec.Name)}-v{apiSpec.Version}.yaml";
                    break;

                case ExportFormat.Json:
                    content = ConvertToJson(apiSpec.Content);
                    contentType = "application/json";
                    fileName = $"{SanitizeFileName(apiSpec.Name)}-v{apiSpec.Version}.json";
                    break;

                case ExportFormat.PostmanCollection:
                    content = ConvertToPostmanCollection(apiSpec.Content, apiSpec.Name, apiSpec.Version);
                    contentType = "application/json";
                    fileName = $"{SanitizeFileName(apiSpec.Name)}-postman-collection.json";
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported export format: {request.TargetFormat}");
            }

            _logger.LogInformation("API Spec {ApiSpecId} exported to {TargetFormat}",
                request.ApiSpecId, request.TargetFormat);

            return new ExportResponse
            {
                FileName = fileName,
                Content = content,
                ContentType = contentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export API specification {ApiSpecId} to {TargetFormat}",
                request.ApiSpecId, request.TargetFormat);
            throw new InvalidOperationException($"Export failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Convert content to YAML format
    /// </summary>
    private string ConvertToYaml(string content)
    {
        var trimmedContent = content.Trim();

        // If already YAML, return as-is
        if (!trimmedContent.StartsWith("{"))
        {
            return content;
        }

        // Convert from JSON to YAML
        var jsonObj = JsonSerializer.Deserialize<object>(content);
        return _yamlSerializer.Serialize(jsonObj);
    }

    /// <summary>
    /// Convert content to JSON format
    /// </summary>
    private string ConvertToJson(string content)
    {
        var trimmedContent = content.Trim();

        // If already JSON, format it nicely
        if (trimmedContent.StartsWith("{"))
        {
            var jsonObj = JsonSerializer.Deserialize<object>(content);
            return JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        // Convert from YAML to JSON
        var yamlObj = _yamlDeserializer.Deserialize<object>(content);
        return JsonSerializer.Serialize(yamlObj, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Convert Postman Collection to OpenAPI 3.0
    /// </summary>
    private string ConvertPostmanToOpenApi(string postmanContent)
    {
        // Parse Postman Collection
        var postmanCollection = JsonSerializer.Deserialize<JsonElement>(postmanContent);

        // Extract info
        var collectionName = postmanCollection.TryGetProperty("info", out var info) &&
                            info.TryGetProperty("name", out var name)
            ? name.GetString() ?? "API"
            : "API";

        var collectionDescription = info.TryGetProperty("description", out var desc)
            ? desc.GetString()
            : null;

        // Build OpenAPI structure
        var openApiSpec = new
        {
            openapi = "3.0.0",
            info = new
            {
                title = collectionName,
                version = "1.0.0",
                description = collectionDescription
            },
            servers = new[]
            {
                new { url = "https://api.example.com", description = "Production server" }
            },
            paths = BuildPathsFromPostman(postmanCollection)
        };

        return JsonSerializer.Serialize(openApiSpec, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Build OpenAPI paths from Postman collection items
    /// </summary>
    private Dictionary<string, object> BuildPathsFromPostman(JsonElement postmanCollection)
    {
        var paths = new Dictionary<string, object>();

        if (!postmanCollection.TryGetProperty("item", out var items))
        {
            return paths;
        }

        foreach (var item in items.EnumerateArray())
        {
            ProcessPostmanItem(item, paths);
        }

        return paths;
    }

    /// <summary>
    /// Process a Postman item (request or folder)
    /// </summary>
    private void ProcessPostmanItem(JsonElement item, Dictionary<string, object> paths)
    {
        // Check if it's a request
        if (item.TryGetProperty("request", out var request))
        {
            var method = request.TryGetProperty("method", out var methodProp)
                ? methodProp.GetString()?.ToLower() ?? "get"
                : "get";

            var url = request.TryGetProperty("url", out var urlProp)
                ? ExtractPathFromPostmanUrl(urlProp)
                : "/";

            var description = item.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()
                : null;

            // Add to paths
            if (!paths.ContainsKey(url))
            {
                paths[url] = new Dictionary<string, object>();
            }

            var pathItem = (Dictionary<string, object>)paths[url];
            pathItem[method] = new
            {
                summary = description,
                description = description,
                responses = new
                {
                    _200 = new
                    {
                        description = "Successful response"
                    }
                }
            };
        }
        // Check if it's a folder with nested items
        else if (item.TryGetProperty("item", out var nestedItems))
        {
            foreach (var nestedItem in nestedItems.EnumerateArray())
            {
                ProcessPostmanItem(nestedItem, paths);
            }
        }
    }

    /// <summary>
    /// Extract path from Postman URL object
    /// </summary>
    private string ExtractPathFromPostmanUrl(JsonElement urlElement)
    {
        // URL can be a string or object
        if (urlElement.ValueKind == JsonValueKind.String)
        {
            var urlString = urlElement.GetString() ?? "";
            var uri = new Uri(urlString, UriKind.RelativeOrAbsolute);
            return uri.IsAbsoluteUri ? uri.AbsolutePath : urlString;
        }

        // URL object with path array
        if (urlElement.TryGetProperty("path", out var pathArray) && pathArray.ValueKind == JsonValueKind.Array)
        {
            var pathSegments = pathArray.EnumerateArray()
                .Select(p => p.GetString() ?? "")
                .Where(p => !string.IsNullOrEmpty(p));
            return "/" + string.Join("/", pathSegments);
        }

        return "/";
    }

    /// <summary>
    /// Convert to Postman Collection format
    /// </summary>
    private string ConvertToPostmanCollection(string openapiContent, string name, string version)
    {
        // Parse OpenAPI spec
        var openapiDoc = JsonSerializer.Deserialize<JsonElement>(ConvertToJson(openapiContent));

        // Build Postman Collection structure
        var postmanCollection = new
        {
            info = new
            {
                name = $"{name} v{version}",
                schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
            },
            item = BuildPostmanItemsFromPaths(openapiDoc)
        };

        return JsonSerializer.Serialize(postmanCollection, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Build Postman collection items from OpenAPI paths
    /// </summary>
    private List<object> BuildPostmanItemsFromPaths(JsonElement openapiDoc)
    {
        var items = new List<object>();

        if (!openapiDoc.TryGetProperty("paths", out var paths))
        {
            return items;
        }

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject())
            {
                var method = operation.Name.ToUpper();
                var validMethods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS", "HEAD" };

                if (!validMethods.Contains(method))
                    continue;

                var operationObj = operation.Value;
                var summary = operationObj.TryGetProperty("summary", out var summaryProp)
                    ? summaryProp.GetString()
                    : $"{method} {path.Name}";

                items.Add(new
                {
                    name = summary,
                    request = new
                    {
                        method = method,
                        url = new
                        {
                            raw = "{{baseUrl}}" + path.Name,
                            host = new[] { "{{baseUrl}}" },
                            path = path.Name.TrimStart('/').Split('/')
                        }
                    }
                });
            }
        }

        return items;
    }

    /// <summary>
    /// Sanitize filename for safe file system usage
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
}
