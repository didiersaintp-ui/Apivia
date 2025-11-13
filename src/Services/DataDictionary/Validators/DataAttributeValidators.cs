using Apivia.Services.DataDictionary.DTOs;
using FluentValidation;

namespace Apivia.Services.DataDictionary.Validators;

/// <summary>
/// Validator for CreateDataAttributeRequest
/// </summary>
public class CreateDataAttributeRequestValidator : AbstractValidator<CreateDataAttributeRequest>
{
    private static readonly string[] ValidDataTypes = new[]
    {
        "string", "integer", "long", "decimal", "boolean", "date", "datetime",
        "time", "uuid", "json", "binary", "text", "varchar", "char"
    };

    public CreateDataAttributeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required")
            .MaximumLength(200).WithMessage("Attribute name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Attribute name can only contain letters, numbers, and underscores (database naming convention)");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required");

        RuleFor(x => x.DataType)
            .NotEmpty().WithMessage("Data type is required")
            .Must(BeValidDataType).WithMessage($"Data type must be one of: {string.Join(", ", ValidDataTypes)}");

        RuleFor(x => x.MaxLength)
            .GreaterThan(0).WithMessage("Max length must be greater than 0")
            .When(x => x.MaxLength.HasValue);

        RuleFor(x => x.ValidationRules)
            .Must(BeValidJson).WithMessage("Validation rules must be valid JSON")
            .When(x => !string.IsNullOrEmpty(x.ValidationRules));

        RuleFor(x => x.Metadata)
            .Must(BeValidJson).WithMessage("Metadata must be valid JSON")
            .When(x => !string.IsNullOrEmpty(x.Metadata));
    }

    private bool BeValidDataType(string dataType)
    {
        return ValidDataTypes.Contains(dataType.ToLower());
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
/// Validator for UpdateDataAttributeRequest
/// </summary>
public class UpdateDataAttributeRequestValidator : AbstractValidator<UpdateDataAttributeRequest>
{
    private static readonly string[] ValidDataTypes = new[]
    {
        "string", "integer", "long", "decimal", "boolean", "date", "datetime",
        "time", "uuid", "json", "binary", "text", "varchar", "char"
    };

    public UpdateDataAttributeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required")
            .MaximumLength(200).WithMessage("Attribute name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Attribute name can only contain letters, numbers, and underscores (database naming convention)");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DataType)
            .NotEmpty().WithMessage("Data type is required")
            .Must(BeValidDataType).WithMessage($"Data type must be one of: {string.Join(", ", ValidDataTypes)}");

        RuleFor(x => x.MaxLength)
            .GreaterThan(0).WithMessage("Max length must be greater than 0")
            .When(x => x.MaxLength.HasValue);

        RuleFor(x => x.ValidationRules)
            .Must(BeValidJson).WithMessage("Validation rules must be valid JSON")
            .When(x => !string.IsNullOrEmpty(x.ValidationRules));

        RuleFor(x => x.Metadata)
            .Must(BeValidJson).WithMessage("Metadata must be valid JSON")
            .When(x => !string.IsNullOrEmpty(x.Metadata));
    }

    private bool BeValidDataType(string dataType)
    {
        return ValidDataTypes.Contains(dataType.ToLower());
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
/// Validator for BulkCreateAttributesRequest
/// </summary>
public class BulkCreateAttributesRequestValidator : AbstractValidator<BulkCreateAttributesRequest>
{
    public BulkCreateAttributesRequestValidator()
    {
        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required");

        RuleFor(x => x.Attributes)
            .NotEmpty().WithMessage("At least one attribute is required")
            .Must(x => x.Count <= 100).WithMessage("Cannot bulk create more than 100 attributes at once");

        RuleForEach(x => x.Attributes)
            .SetValidator(new CreateDataAttributeRequestValidator());
    }
}
