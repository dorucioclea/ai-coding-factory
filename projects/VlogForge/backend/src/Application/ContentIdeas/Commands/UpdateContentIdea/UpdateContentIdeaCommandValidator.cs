using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Commands.UpdateContentIdea;

/// <summary>
/// Validator for UpdateContentIdeaCommand.
/// Story: ACF-005
/// </summary>
public sealed class UpdateContentIdeaCommandValidator : AbstractValidator<UpdateContentIdeaCommand>
{
    private const int MaxPlatformTagLength = 50;

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
            .WithMessage($"Cannot have more than {ContentItem.MaxPlatformTags} platform tags.")
            .Must(tags => tags == null || tags.All(t => string.IsNullOrEmpty(t) || t.Length <= MaxPlatformTagLength))
            .WithMessage($"Each platform tag must be {MaxPlatformTagLength} characters or less.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid status value.");
    }
}
