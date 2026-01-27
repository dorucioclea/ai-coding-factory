using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Teams.Commands.CreateTeam;

/// <summary>
/// Validator for CreateTeamCommand.
/// Story: ACF-007
/// </summary>
public sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("Owner ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Team name is required.")
            .MaximumLength(Team.MaxNameLength)
            .WithMessage($"Team name cannot exceed {Team.MaxNameLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(Team.MaxDescriptionLength)
            .WithMessage($"Description cannot exceed {Team.MaxDescriptionLength} characters.")
            .When(x => x.Description is not null);
    }
}
