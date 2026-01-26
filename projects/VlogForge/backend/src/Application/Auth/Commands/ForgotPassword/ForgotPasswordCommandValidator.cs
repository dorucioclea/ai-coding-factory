using FluentValidation;

namespace VlogForge.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Validator for ForgotPasswordCommand.
/// Story: ACF-001
/// </summary>
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
