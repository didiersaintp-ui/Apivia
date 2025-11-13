using Apivia.Services.DataDictionary.DTOs;
using FluentValidation;

namespace Apivia.Services.DataDictionary.Validators;

/// <summary>
/// Validator for CreateDataDictionaryRequest
/// </summary>
public class CreateDataDictionaryRequestValidator : AbstractValidator<CreateDataDictionaryRequest>
{
    public CreateDataDictionaryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Dictionary name is required")
            .MaximumLength(200).WithMessage("Dictionary name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$")
                .WithMessage("Dictionary name can only contain letters, numbers, spaces, hyphens, underscores, and periods");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.WorkspaceId)
            .NotEmpty().WithMessage("Workspace ID is required");
    }
}

/// <summary>
/// Validator for UpdateDataDictionaryRequest
/// </summary>
public class UpdateDataDictionaryRequestValidator : AbstractValidator<UpdateDataDictionaryRequest>
{
    public UpdateDataDictionaryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Dictionary name is required")
            .MaximumLength(200).WithMessage("Dictionary name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$")
                .WithMessage("Dictionary name can only contain letters, numbers, spaces, hyphens, underscores, and periods");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
