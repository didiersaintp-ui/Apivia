using Apivia.Services.Governance.DTOs;
using FluentValidation;

namespace Apivia.Services.Governance.Validators;

/// <summary>
/// Validator for CreateLinkRequest
/// </summary>
public class CreateLinkRequestValidator : AbstractValidator<CreateLinkRequest>
{
    public CreateLinkRequestValidator()
    {
        RuleFor(x => x.ApiSpecId)
            .NotEmpty().WithMessage("API Spec ID is required");

        RuleFor(x => x.SchemaPath)
            .NotEmpty().WithMessage("Schema path is required")
            .MaximumLength(500).WithMessage("Schema path must not exceed 500 characters")
            .Must(BeValidJsonPath).WithMessage("Schema path must be a valid JSONPath expression");

        RuleFor(x => x)
            .Must(x => x.DataEntityId.HasValue || x.DataAttributeId.HasValue)
            .WithMessage("Either Data Entity ID or Data Attribute ID must be provided");

        RuleFor(x => x)
            .Must(x => !(x.DataEntityId.HasValue && x.DataAttributeId.HasValue))
            .WithMessage("Cannot link to both Data Entity and Data Attribute simultaneously");
    }

    private bool BeValidJsonPath(string path)
    {
        // Basic JSONPath validation
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // Should start with $ (root) or # (definitions/components)
        return path.StartsWith("$") || path.StartsWith("#");
    }
}

/// <summary>
/// Validator for BulkCreateLinksRequest
/// </summary>
public class BulkCreateLinksRequestValidator : AbstractValidator<BulkCreateLinksRequest>
{
    public BulkCreateLinksRequestValidator()
    {
        RuleFor(x => x.ApiSpecId)
            .NotEmpty().WithMessage("API Spec ID is required");

        RuleFor(x => x.Links)
            .NotEmpty().WithMessage("At least one link is required")
            .Must(x => x.Count <= 100).WithMessage("Cannot bulk create more than 100 links at once");

        RuleForEach(x => x.Links)
            .SetValidator(new CreateLinkRequestValidator());
    }
}

/// <summary>
/// Validator for LinkQueryParams
/// </summary>
public class LinkQueryParamsValidator : AbstractValidator<LinkQueryParams>
{
    public LinkQueryParamsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(200).WithMessage("Page size must not exceed 200");
    }
}
