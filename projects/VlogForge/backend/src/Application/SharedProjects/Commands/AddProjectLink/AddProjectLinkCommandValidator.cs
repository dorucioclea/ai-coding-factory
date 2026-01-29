using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.SharedProjects.Commands.AddProjectLink;

/// <summary>
/// Validator for AddProjectLinkCommand.
/// Story: ACF-013
/// </summary>
public sealed class AddProjectLinkCommandValidator : AbstractValidator<AddProjectLinkCommand>
{
    public AddProjectLinkCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Link title is required.")
            .MaximumLength(SharedProjectLink.MaxTitleLength);
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .MaximumLength(SharedProjectLink.MaxUrlLength);
        RuleFor(x => x.Description)
            .MaximumLength(SharedProjectLink.MaxDescriptionLength)
            .When(x => x.Description is not null);
    }
}
