using FluentValidation;

namespace VlogForge.Application.Integrations.Commands.DisconnectPlatform;

/// <summary>
/// Validator for DisconnectPlatformCommand.
/// Story: ACF-003
/// </summary>
public sealed class DisconnectPlatformCommandValidator : AbstractValidator<DisconnectPlatformCommand>
{
    public DisconnectPlatformCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.PlatformType)
            .Must(p => IntegrationConstants.SupportedPlatforms.Contains(p))
            .WithMessage("Platform must be YouTube, Instagram, or TikTok.");
    }
}
