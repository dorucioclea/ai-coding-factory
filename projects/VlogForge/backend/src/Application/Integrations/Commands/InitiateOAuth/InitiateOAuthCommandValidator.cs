using FluentValidation;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Integrations.Commands.InitiateOAuth;

/// <summary>
/// Validator for InitiateOAuthCommand.
/// Story: ACF-003
/// </summary>
public sealed class InitiateOAuthCommandValidator : AbstractValidator<InitiateOAuthCommand>
{
    public InitiateOAuthCommandValidator(IOAuthRedirectValidator redirectValidator)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.PlatformType)
            .Must(p => IntegrationConstants.SupportedPlatforms.Contains(p))
            .WithMessage("Platform must be YouTube, Instagram, or TikTok.");

        RuleFor(x => x.RedirectUri)
            .NotEmpty().WithMessage("Redirect URI is required.")
            .MaximumLength(IntegrationConstants.MaxRedirectUriLength)
                .WithMessage($"Redirect URI cannot exceed {IntegrationConstants.MaxRedirectUriLength} characters.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Redirect URI must be a valid absolute URL.")
            .Must(redirectValidator.IsAllowed)
                .WithMessage(uri => $"Redirect URI is not in the allowed list. Allowed URIs: {string.Join(", ", redirectValidator.AllowedUris)}");
    }
}
