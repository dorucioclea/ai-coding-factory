using FluentValidation;

namespace VlogForge.Application.Auth.Commands.VerifyEmail;

/// <summary>
/// Validator for VerifyEmailCommand.
/// Story: ACF-001
/// </summary>
public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required.");
    }
}
