using FluentValidation;

namespace VlogForge.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Validator for RefreshTokenCommand.
/// Story: ACF-001
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
