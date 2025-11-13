using Apivia.Services.ApiDesign.DTOs;
using Apivia.Shared.Data.Enums;
using NJsonSchema;
using NJsonSchema.Validation;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Apivia.Services.ApiDesign.Services;

/// <summary>
/// Service interface for API specification validation
/// </summary>
public interface IValidationService
{
    Task<ValidationResponse> ValidateAsync(ValidateApiSpecRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of validation service for OpenAPI specifications
/// </summary>
public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;
    private readonly IDeserializer _yamlDeserializer;
    private readonly ISerializer _yamlSerializer;

    public ValidationService(ILogger<ValidationService> logger)
    {
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
    /// Validate an API specification and extract metadata
    /// </summary>
    public async Task<ValidationResponse> ValidateAsync(ValidateApiSpecRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();
        ValidationMetadata? metadata = null;

        try
        {
            // Parse the content based on format
            var contentAsJson = await ConvertToJsonAsync(request.Content, request.Format);

            // Validate the structure
            var (structureErrors, structureWarnings) = ValidateStructure(contentAsJson, request.Format);
            errors.AddRange(structureErrors);
            warnings.AddRange(structureWarnings);

            // Extract metadata if validation passes basic checks
            if (errors.Count == 0)
            {
                metadata = ExtractMetadata(contentAsJson);
            }

            return new ValidationResponse
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings,
                Metadata = metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation error for API specification");

            errors.Add(new ValidationError
            {
                Path = "/",
                Message = $"Failed to parse specification: {ex.Message}",
                Severity = "Error"
            });

            return new ValidationResponse
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings,
                Metadata = null
            };
        }
    }

    /// <summary>
    /// Convert YAML or JSON content to JSON string
    /// </summary>
    private async Task<string> ConvertToJsonAsync(string content, ApiSpecFormat format)
    {
        await Task.CompletedTask; // Make async for potential future enhancements

        var trimmedContent = content.Trim();

        // If it's already JSON, return as-is
        if (trimmedContent.StartsWith("{"))
        {
            // Validate it's valid JSON
            JsonDocument.Parse(trimmedContent);
            return trimmedContent;
        }

        // If it's YAML, convert to JSON
        try
        {
            var yamlObject = _yamlDeserializer.Deserialize<object>(content);
            var jsonString = JsonSerializer.Serialize(yamlObject);
            return jsonString;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse YAML content: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validate the structure of the OpenAPI specification
    /// </summary>
    private (List<ValidationError> errors, List<ValidationWarning> warnings) ValidateStructure(string jsonContent, ApiSpecFormat format)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        try
        {
            var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            // Determine OpenAPI version
            string? openapiVersion = null;
            string? swaggerVersion = null;

            if (root.TryGetProperty("openapi", out var openapiProp))
            {
                openapiVersion = openapiProp.GetString();
            }
            else if (root.TryGetProperty("swagger", out var swaggerProp))
            {
                swaggerVersion = swaggerProp.GetString();
            }

            // Check version matches format
            if (format == ApiSpecFormat.OpenApiV3 && openapiVersion == null)
            {
                errors.Add(new ValidationError
                {
                    Path = "/openapi",
                    Message = "OpenAPI 3.x specification must have 'openapi' field",
                    Severity = "Error"
                });
                return (errors, warnings);
            }

            if (format == ApiSpecFormat.OpenApiV2 && swaggerVersion == null)
            {
                errors.Add(new ValidationError
                {
                    Path = "/swagger",
                    Message = "OpenAPI 2.0 (Swagger) specification must have 'swagger' field",
                    Severity = "Error"
                });
                return (errors, warnings);
            }

            // Validate required fields for OpenAPI 3.x
            if (openapiVersion != null)
            {
                ValidateOpenApi3Required(root, errors);
                ValidateOpenApi3Structure(root, errors, warnings);
            }
            // Validate required fields for OpenAPI 2.0 (Swagger)
            else if (swaggerVersion != null)
            {
                ValidateOpenApi2Required(root, errors);
                ValidateOpenApi2Structure(root, errors, warnings);
            }
        }
        catch (JsonException ex)
        {
            errors.Add(new ValidationError
            {
                Path = "/",
                Message = $"Invalid JSON structure: {ex.Message}",
                Severity = "Error"
            });
        }

        return (errors, warnings);
    }

    /// <summary>
    /// Validate required fields for OpenAPI 3.x
    /// </summary>
    private void ValidateOpenApi3Required(JsonElement root, List<ValidationError> errors)
    {
        // Required: openapi, info, paths
        if (!root.TryGetProperty("info", out var info))
        {
            errors.Add(new ValidationError { Path = "/info", Message = "Missing required 'info' object" });
        }
        else
        {
            if (!info.TryGetProperty("title", out _))
            {
                errors.Add(new ValidationError { Path = "/info/title", Message = "Missing required 'title' in info" });
            }
            if (!info.TryGetProperty("version", out _))
            {
                errors.Add(new ValidationError { Path = "/info/version", Message = "Missing required 'version' in info" });
            }
        }

        if (!root.TryGetProperty("paths", out _))
        {
            errors.Add(new ValidationError { Path = "/paths", Message = "Missing required 'paths' object" });
        }
    }

    /// <summary>
    /// Validate structure for OpenAPI 3.x
    /// </summary>
    private void ValidateOpenApi3Structure(JsonElement root, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        // Check for servers
        if (!root.TryGetProperty("servers", out _))
        {
            warnings.Add(new ValidationWarning
            {
                Path = "/servers",
                Message = "No 'servers' defined. Consider adding server information.",
                Severity = "Warning"
            });
        }

        // Check paths structure
        if (root.TryGetProperty("paths", out var paths))
        {
            if (paths.ValueKind == JsonValueKind.Object)
            {
                var pathCount = 0;
                foreach (var path in paths.EnumerateObject())
                {
                    pathCount++;
                    ValidatePathItem(path, errors, warnings, isOpenApi3: true);
                }

                if (pathCount == 0)
                {
                    warnings.Add(new ValidationWarning
                    {
                        Path = "/paths",
                        Message = "No paths defined in the specification",
                        Severity = "Warning"
                    });
                }
            }
        }

        // Check for components/schemas
        if (root.TryGetProperty("components", out var components))
        {
            if (components.TryGetProperty("schemas", out var schemas) && schemas.ValueKind == JsonValueKind.Object)
            {
                foreach (var schema in schemas.EnumerateObject())
                {
                    ValidateSchema(schema.Value, $"/components/schemas/{schema.Name}", errors, warnings);
                }
            }
        }
    }

    /// <summary>
    /// Validate required fields for OpenAPI 2.0 (Swagger)
    /// </summary>
    private void ValidateOpenApi2Required(JsonElement root, List<ValidationError> errors)
    {
        // Required: swagger, info, paths
        if (!root.TryGetProperty("info", out var info))
        {
            errors.Add(new ValidationError { Path = "/info", Message = "Missing required 'info' object" });
        }
        else
        {
            if (!info.TryGetProperty("title", out _))
            {
                errors.Add(new ValidationError { Path = "/info/title", Message = "Missing required 'title' in info" });
            }
            if (!info.TryGetProperty("version", out _))
            {
                errors.Add(new ValidationError { Path = "/info/version", Message = "Missing required 'version' in info" });
            }
        }

        if (!root.TryGetProperty("paths", out _))
        {
            errors.Add(new ValidationError { Path = "/paths", Message = "Missing required 'paths' object" });
        }
    }

    /// <summary>
    /// Validate structure for OpenAPI 2.0 (Swagger)
    /// </summary>
    private void ValidateOpenApi2Structure(JsonElement root, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        // Check for host/basePath
        if (!root.TryGetProperty("host", out _))
        {
            warnings.Add(new ValidationWarning
            {
                Path = "/host",
                Message = "No 'host' defined. Consider adding host information.",
                Severity = "Warning"
            });
        }

        // Check paths structure
        if (root.TryGetProperty("paths", out var paths))
        {
            if (paths.ValueKind == JsonValueKind.Object)
            {
                var pathCount = 0;
                foreach (var path in paths.EnumerateObject())
                {
                    pathCount++;
                    ValidatePathItem(path, errors, warnings, isOpenApi3: false);
                }

                if (pathCount == 0)
                {
                    warnings.Add(new ValidationWarning
                    {
                        Path = "/paths",
                        Message = "No paths defined in the specification",
                        Severity = "Warning"
                    });
                }
            }
        }
    }

    /// <summary>
    /// Validate a path item
    /// </summary>
    private void ValidatePathItem(JsonProperty pathItem, List<ValidationError> errors, List<ValidationWarning> warnings, bool isOpenApi3)
    {
        var pathName = pathItem.Name;
        var pathValue = pathItem.Value;

        if (!pathName.StartsWith("/"))
        {
            errors.Add(new ValidationError
            {
                Path = $"/paths/{pathName}",
                Message = "Path must start with '/'",
                Severity = "Error"
            });
        }

        if (pathValue.ValueKind == JsonValueKind.Object)
        {
            var hasOperations = false;
            var validMethods = new[] { "get", "post", "put", "patch", "delete", "options", "head", "trace" };

            foreach (var operation in pathValue.EnumerateObject())
            {
                if (validMethods.Contains(operation.Name.ToLower()))
                {
                    hasOperations = true;
                }
            }

            if (!hasOperations)
            {
                warnings.Add(new ValidationWarning
                {
                    Path = $"/paths/{pathName}",
                    Message = "Path has no HTTP method operations defined",
                    Severity = "Warning"
                });
            }
        }
    }

    /// <summary>
    /// Validate a schema definition
    /// </summary>
    private void ValidateSchema(JsonElement schema, string path, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            errors.Add(new ValidationError
            {
                Path = path,
                Message = "Schema must be an object",
                Severity = "Error"
            });
            return;
        }

        // Check for type or $ref
        if (!schema.TryGetProperty("type", out _) &&
            !schema.TryGetProperty("$ref", out _) &&
            !schema.TryGetProperty("allOf", out _) &&
            !schema.TryGetProperty("oneOf", out _) &&
            !schema.TryGetProperty("anyOf", out _))
        {
            warnings.Add(new ValidationWarning
            {
                Path = path,
                Message = "Schema should have 'type' or '$ref' or composition keywords",
                Severity = "Warning"
            });
        }
    }

    /// <summary>
    /// Extract metadata from the specification
    /// </summary>
    private ValidationMetadata ExtractMetadata(string jsonContent)
    {
        var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var metadata = new ValidationMetadata();

        // Extract info
        if (root.TryGetProperty("info", out var info))
        {
            if (info.TryGetProperty("title", out var title))
                metadata.Title = title.GetString();
            if (info.TryGetProperty("version", out var version))
                metadata.Version = version.GetString();
            if (info.TryGetProperty("description", out var description))
                metadata.Description = description.GetString();
        }

        // Count endpoints
        if (root.TryGetProperty("paths", out var paths) && paths.ValueKind == JsonValueKind.Object)
        {
            var endpointCount = 0;
            foreach (var path in paths.EnumerateObject())
            {
                if (path.Value.ValueKind == JsonValueKind.Object)
                {
                    var validMethods = new[] { "get", "post", "put", "patch", "delete", "options", "head", "trace" };
                    endpointCount += path.Value.EnumerateObject()
                        .Count(op => validMethods.Contains(op.Name.ToLower()));
                }
            }
            metadata.EndpointCount = endpointCount;
        }

        // Count schemas
        var schemaCount = 0;
        if (root.TryGetProperty("components", out var components) &&
            components.TryGetProperty("schemas", out var schemas) &&
            schemas.ValueKind == JsonValueKind.Object)
        {
            schemaCount = schemas.EnumerateObject().Count();
        }
        else if (root.TryGetProperty("definitions", out var definitions) &&
                 definitions.ValueKind == JsonValueKind.Object)
        {
            schemaCount = definitions.EnumerateObject().Count();
        }
        metadata.SchemaCount = schemaCount;

        // Extract tags
        var tags = new List<string>();
        if (root.TryGetProperty("tags", out var tagsArray) && tagsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var tag in tagsArray.EnumerateArray())
            {
                if (tag.TryGetProperty("name", out var tagName))
                {
                    var name = tagName.GetString();
                    if (!string.IsNullOrEmpty(name))
                        tags.Add(name);
                }
            }
        }
        metadata.Tags = tags;

        return metadata;
    }
}
