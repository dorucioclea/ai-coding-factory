using FluentValidation;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Profiles.Commands.CreateProfile;

/// <summary>
/// Validator for CreateProfileCommand.
/// Story: ACF-002
/// </summary>
public sealed class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(CreatorProfile.MinUsernameLength)
                .WithMessage($"Username must be at least {CreatorProfile.MinUsernameLength} characters.")
            .MaximumLength(CreatorProfile.MaxUsernameLength)
                .WithMessage($"Username cannot exceed {CreatorProfile.MaxUsernameLength} characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(Bio.MaxLength)
                .WithMessage($"Bio cannot exceed {Bio.MaxLength} characters.")
            .When(x => x.Bio is not null);
    }
}
