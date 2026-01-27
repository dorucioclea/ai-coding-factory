using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Commands.CreateContentIdea;

/// <summary>
/// Validator for CreateContentIdeaCommand.
/// Story: ACF-005
/// </summary>
public sealed class CreateContentIdeaCommandValidator : AbstractValidator<CreateContentIdeaCommand>
{
    private const int MaxPlatformTagLength = 50;

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
            .WithMessage($"Cannot have more than {ContentItem.MaxPlatformTags} platform tags.")
            .Must(tags => tags == null || tags.All(t => string.IsNullOrEmpty(t) || t.Length <= MaxPlatformTagLength))
            .WithMessage($"Each platform tag must be {MaxPlatformTagLength} characters or less.");
    }
}
