using Apivia.Services.Governance.DTOs;
using FluentValidation;

namespace Apivia.Services.Governance.Validators;

/// <summary>
/// Validator for CreateImpactAnalysisRequest
/// </summary>
public class CreateImpactAnalysisRequestValidator : AbstractValidator<CreateImpactAnalysisRequest>
{
    public CreateImpactAnalysisRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.DataEntityId.HasValue || x.DataAttributeId.HasValue)
            .WithMessage("Either Data Entity ID or Data Attribute ID must be provided");

        RuleFor(x => x)
            .Must(x => !(x.DataEntityId.HasValue && x.DataAttributeId.HasValue))
            .WithMessage("Cannot analyze both Data Entity and Data Attribute simultaneously");

        RuleFor(x => x.ChangeType)
            .IsInEnum().WithMessage("Invalid change type");

        RuleFor(x => x.ProposedChanges)
            .NotEmpty().WithMessage("Proposed changes are required")
            .MaximumLength(5000).WithMessage("Proposed changes must not exceed 5000 characters")
            .Must(BeValidJson).WithMessage("Proposed changes must be valid JSON");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

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
/// Validator for ImpactAnalysisQueryParams
/// </summary>
public class ImpactAnalysisQueryParamsValidator : AbstractValidator<ImpactAnalysisQueryParams>
{
    public ImpactAnalysisQueryParamsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.ChangeType)
            .IsInEnum().WithMessage("Invalid change type")
            .When(x => x.ChangeType.HasValue);

        RuleFor(x => x.RiskLevel)
            .IsInEnum().WithMessage("Invalid risk level")
            .When(x => x.RiskLevel.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status")
            .When(x => x.Status.HasValue);
    }
}

/// <summary>
/// Validator for ResolveImpactAnalysisRequest
/// </summary>
public class ResolveImpactAnalysisRequestValidator : AbstractValidator<ResolveImpactAnalysisRequest>
{
    public ResolveImpactAnalysisRequestValidator()
    {
        RuleFor(x => x.Resolution)
            .MaximumLength(5000).WithMessage("Resolution must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Resolution));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for PreviewImpactRequest
/// </summary>
public class PreviewImpactRequestValidator : AbstractValidator<PreviewImpactRequest>
{
    public PreviewImpactRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.DataEntityId.HasValue || x.DataAttributeId.HasValue)
            .WithMessage("Either Data Entity ID or Data Attribute ID must be provided");

        RuleFor(x => x)
            .Must(x => !(x.DataEntityId.HasValue && x.DataAttributeId.HasValue))
            .WithMessage("Cannot preview impact for both Data Entity and Data Attribute simultaneously");

        RuleFor(x => x.ChangeType)
            .IsInEnum().WithMessage("Invalid change type");
    }
}
