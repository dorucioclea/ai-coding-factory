using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Commands.CreateContentIdea;

/// <summary>
/// Validator for CreateContentIdeaCommand.
/// Story: ACF-005
/// </summary>
public sealed class CreateContentIdeaCommandValidator : AbstractValidator<CreateContentIdeaCommand>
{
    public CreateContentIdeaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(ContentItem.MaxTitleLength)
            .WithMessage($"Title cannot exceed {ContentItem.MaxTitleLength} characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(ContentItem.MaxNotesLength)
            .WithMessage($"Notes cannot exceed {ContentItem.MaxNotesLength} characters.")
            .When(x => x.Notes != null);

        RuleFor(x => x.PlatformTags)
            .Must(tags => tags == null || tags.Count <= ContentItem.MaxPlatformTags)
            .WithMessage($"Cannot have more than {ContentItem.MaxPlatformTags} platform tags.");
    }
}
