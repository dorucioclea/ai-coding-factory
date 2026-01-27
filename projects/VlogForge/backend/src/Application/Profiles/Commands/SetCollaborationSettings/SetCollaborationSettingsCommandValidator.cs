using FluentValidation;

namespace VlogForge.Application.Profiles.Commands.SetCollaborationSettings;

/// <summary>
/// Validator for SetCollaborationSettingsCommand.
/// Story: ACF-002
/// </summary>
public sealed class SetCollaborationSettingsCommandValidator : AbstractValidator<SetCollaborationSettingsCommand>
{
    public SetCollaborationSettingsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CollaborationPreferences)
            .MaximumLength(500)
                .WithMessage("Collaboration preferences cannot exceed 500 characters.")
            .When(x => x.CollaborationPreferences is not null);
    }
}
