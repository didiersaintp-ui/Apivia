using Apivia.Services.ApiDesign.DTOs;
using FluentValidation;

namespace Apivia.Services.ApiDesign.Validators;

/// <summary>
/// Validator for CreateApiSpecRequest
/// </summary>
public class CreateApiSpecRequestValidator : AbstractValidator<CreateApiSpecRequest>
{
    public CreateApiSpecRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("API specification name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$")
                .WithMessage("Name can only contain letters, numbers, spaces, hyphens, underscores, and periods");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Format)
            .IsInEnum().WithMessage("Invalid API specification format");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("API specification content is required")
            .Must(BeValidYamlOrJson).WithMessage("Content must be valid YAML or JSON");
    }

    private bool BeValidYamlOrJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        // Basic validation - actual validation will be done by the validation service
        var trimmed = content.Trim();
        return (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
               (trimmed.StartsWith("openapi:") || trimmed.StartsWith("swagger:"));
    }
}

/// <summary>
/// Validator for UpdateApiSpecRequest
/// </summary>
public class UpdateApiSpecRequestValidator : AbstractValidator<UpdateApiSpecRequest>
{
    public UpdateApiSpecRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("API specification name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$")
                .WithMessage("Name can only contain letters, numbers, spaces, hyphens, underscores, and periods");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("API specification content is required")
            .Must(BeValidYamlOrJson).WithMessage("Content must be valid YAML or JSON");
    }

    private bool BeValidYamlOrJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var trimmed = content.Trim();
        return (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
               (trimmed.StartsWith("openapi:") || trimmed.StartsWith("swagger:"));
    }
}

/// <summary>
/// Validator for ValidateApiSpecRequest
/// </summary>
public class ValidateApiSpecRequestValidator : AbstractValidator<ValidateApiSpecRequest>
{
    public ValidateApiSpecRequestValidator()
    {
        RuleFor(x => x.Format)
            .IsInEnum().WithMessage("Invalid API specification format");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required for validation");
    }
}

/// <summary>
/// Validator for ImportApiSpecRequest
/// </summary>
public class ImportApiSpecRequestValidator : AbstractValidator<ImportApiSpecRequest>
{
    public ImportApiSpecRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.SourceFormat)
            .IsInEnum().WithMessage("Invalid source format");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required for import");
    }
}

/// <summary>
/// Validator for ExportApiSpecRequest
/// </summary>
public class ExportApiSpecRequestValidator : AbstractValidator<ExportApiSpecRequest>
{
    public ExportApiSpecRequestValidator()
    {
        RuleFor(x => x.ApiSpecId)
            .NotEmpty().WithMessage("API Spec ID is required");

        RuleFor(x => x.TargetFormat)
            .IsInEnum().WithMessage("Invalid target format");
    }
}

/// <summary>
/// Validator for CreateVersionRequest
/// </summary>
public class CreateVersionRequestValidator : AbstractValidator<CreateVersionRequest>
{
    public CreateVersionRequestValidator()
    {
        RuleFor(x => x.NewVersion)
            .NotEmpty().WithMessage("New version is required")
            .Matches(@"^\d+\.\d+\.\d+$")
                .WithMessage("Version must follow semantic versioning format (e.g., 1.0.0)");

        RuleFor(x => x.VersionNotes)
            .MaximumLength(2000).WithMessage("Version notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.VersionNotes));
    }
}

/// <summary>
/// Validator for ApiSpecQueryParams
/// </summary>
public class ApiSpecQueryParamsValidator : AbstractValidator<ApiSpecQueryParams>
{
    public ApiSpecQueryParamsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.Format)
            .IsInEnum().WithMessage("Invalid format filter")
            .When(x => x.Format.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status filter")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}
