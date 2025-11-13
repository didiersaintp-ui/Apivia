using Apivia.Services.ApiDesign.DTOs;
using FluentValidation;

namespace Apivia.Services.ApiDesign.Validators;

/// <summary>
/// Validator for CreateProjectRequest
/// </summary>
public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$")
                .WithMessage("Project name can only contain letters, numbers, spaces, hyphens, underscores, and periods");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.WorkspaceId)
            .NotEmpty().WithMessage("Workspace ID is required");
    }
}

/// <summary>
/// Validator for UpdateProjectRequest
/// </summary>
public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$")
                .WithMessage("Project name can only contain letters, numbers, spaces, hyphens, underscores, and periods");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

/// <summary>
/// Validator for ProjectQueryParams
/// </summary>
public class ProjectQueryParamsValidator : AbstractValidator<ProjectQueryParams>
{
    public ProjectQueryParamsValidator()
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
