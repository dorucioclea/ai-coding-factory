using FluentValidation;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Profiles.Commands.UpdateProfile;

/// <summary>
/// Validator for UpdateProfileCommand.
/// Story: ACF-002
/// </summary>
public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(Bio.MaxLength)
                .WithMessage($"Bio cannot exceed {Bio.MaxLength} characters.")
            .When(x => x.Bio is not null);

        RuleFor(x => x.NicheTags)
            .Must(tags => tags == null || tags.Count <= CreatorProfile.MaxNicheTags)
                .WithMessage($"Cannot have more than {CreatorProfile.MaxNicheTags} niche tags.");

        RuleForEach(x => x.NicheTags)
            .MinimumLength(NicheTag.MinLength)
                .WithMessage($"Each niche tag must be at least {NicheTag.MinLength} characters.")
            .MaximumLength(NicheTag.MaxLength)
                .WithMessage($"Each niche tag cannot exceed {NicheTag.MaxLength} characters.")
            .Matches(@"^[a-zA-Z0-9]+(-[a-zA-Z0-9]+)*$")
                .WithMessage("Niche tags can only contain letters, numbers, and hyphens (no leading/trailing/consecutive hyphens).")
            .When(x => x.NicheTags is not null);

        RuleFor(x => x.CollaborationPreferences)
            .MaximumLength(500)
                .WithMessage("Collaboration preferences cannot exceed 500 characters.")
            .When(x => x.CollaborationPreferences is not null);
    }
}
