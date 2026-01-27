using FluentValidation;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Integrations.Commands.CompleteOAuth;

/// <summary>
/// Validator for CompleteOAuthCommand.
/// Story: ACF-003
/// </summary>
public sealed class CompleteOAuthCommandValidator : AbstractValidator<CompleteOAuthCommand>
{
    public CompleteOAuthCommandValidator(IOAuthRedirectValidator redirectValidator)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.PlatformType)
            .Must(p => IntegrationConstants.SupportedPlatforms.Contains(p))
            .WithMessage("Platform must be YouTube, Instagram, or TikTok.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Authorization code is required.")
            .MaximumLength(IntegrationConstants.MaxAuthorizationCodeLength)
                .WithMessage($"Authorization code cannot exceed {IntegrationConstants.MaxAuthorizationCodeLength} characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State parameter is required.")
            .MaximumLength(IntegrationConstants.MaxStateLength)
                .WithMessage($"State parameter cannot exceed {IntegrationConstants.MaxStateLength} characters.");

        RuleFor(x => x.RedirectUri)
            .NotEmpty().WithMessage("Redirect URI is required.")
            .MaximumLength(IntegrationConstants.MaxRedirectUriLength)
                .WithMessage($"Redirect URI cannot exceed {IntegrationConstants.MaxRedirectUriLength} characters.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Redirect URI must be a valid absolute URL.")
            .Must(redirectValidator.IsAllowed)
                .WithMessage("Redirect URI is not in the allowed list.");
    }
}
