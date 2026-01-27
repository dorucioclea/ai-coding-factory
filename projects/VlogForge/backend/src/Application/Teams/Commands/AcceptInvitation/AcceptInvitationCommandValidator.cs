using FluentValidation;

namespace VlogForge.Application.Teams.Commands.AcceptInvitation;

/// <summary>
/// Validator for AcceptInvitationCommand.
/// Story: ACF-007
/// </summary>
public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Invitation token is required.")
            .MinimumLength(32)
            .MaximumLength(64)
            .WithMessage("Invalid token format.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.UserEmail)
            .NotEmpty()
            .WithMessage("User email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");
    }
}
