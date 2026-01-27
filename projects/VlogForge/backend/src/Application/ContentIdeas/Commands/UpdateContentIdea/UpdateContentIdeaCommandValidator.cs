using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Commands.UpdateContentIdea;

/// <summary>
/// Validator for UpdateContentIdeaCommand.
/// Story: ACF-005
/// </summary>
public sealed class UpdateContentIdeaCommandValidator : AbstractValidator<UpdateContentIdeaCommand>
{
    public UpdateContentIdeaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

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

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid status value.");
    }
}
