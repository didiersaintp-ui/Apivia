using Apivia.Services.DataDictionary.DTOs;
using FluentValidation;

namespace Apivia.Services.DataDictionary.Validators;

/// <summary>
/// Validator for CreateDataEntityRequest
/// </summary>
public class CreateDataEntityRequestValidator : AbstractValidator<CreateDataEntityRequest>
{
    public CreateDataEntityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Entity name is required")
            .MaximumLength(200).WithMessage("Entity name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Entity name can only contain letters, numbers, and underscores (database naming convention)");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DictionaryId)
            .NotEmpty().WithMessage("Dictionary ID is required");

        RuleFor(x => x.BusinessOwner)
            .MaximumLength(200).WithMessage("Business owner must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.BusinessOwner));

        RuleFor(x => x.FunctionalDomain)
            .MaximumLength(200).WithMessage("Functional domain must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.FunctionalDomain));

        RuleFor(x => x.Sensitivity)
            .IsInEnum().WithMessage("Invalid data sensitivity level")
            .When(x => x.Sensitivity.HasValue);

        RuleFor(x => x.QualityLevel)
            .IsInEnum().WithMessage("Invalid data quality level")
            .When(x => x.QualityLevel.HasValue);

        RuleFor(x => x.Metadata)
            .Must(BeValidJson).WithMessage("Metadata must be valid JSON")
            .When(x => !string.IsNullOrEmpty(x.Metadata));
    }

    private bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Validator for UpdateDataEntityRequest
/// </summary>
public class UpdateDataEntityRequestValidator : AbstractValidator<UpdateDataEntityRequest>
{
    public UpdateDataEntityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Entity name is required")
            .MaximumLength(200).WithMessage("Entity name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Entity name can only contain letters, numbers, and underscores (database naming convention)");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.BusinessOwner)
            .MaximumLength(200).WithMessage("Business owner must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.BusinessOwner));

        RuleFor(x => x.FunctionalDomain)
            .MaximumLength(200).WithMessage("Functional domain must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.FunctionalDomain));

        RuleFor(x => x.Sensitivity)
            .IsInEnum().WithMessage("Invalid data sensitivity level")
            .When(x => x.Sensitivity.HasValue);

        RuleFor(x => x.QualityLevel)
            .IsInEnum().WithMessage("Invalid data quality level")
            .When(x => x.QualityLevel.HasValue);

        RuleFor(x => x.Metadata)
            .Must(BeValidJson).WithMessage("Metadata must be valid JSON")
            .When(x => !string.IsNullOrEmpty(x.Metadata));
    }

    private bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Validator for DataEntityQueryParams
/// </summary>
public class DataEntityQueryParamsValidator : AbstractValidator<DataEntityQueryParams>
{
    public DataEntityQueryParamsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}
