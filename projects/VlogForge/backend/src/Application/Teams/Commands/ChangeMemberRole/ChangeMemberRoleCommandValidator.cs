using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Teams.Commands.ChangeMemberRole;

/// <summary>
/// Validator for ChangeMemberRoleCommand.
/// Story: ACF-007
/// </summary>
public sealed class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.MemberUserId)
            .NotEmpty()
            .WithMessage("Member user ID is required.");

        RuleFor(x => x.ChangedByUserId)
            .NotEmpty()
            .WithMessage("Changed by user ID is required.");

        RuleFor(x => x.NewRole)
            .IsInEnum()
            .WithMessage("Invalid role specified.")
            .Must(r => r != TeamRole.Owner)
            .WithMessage("Cannot assign Owner role. Use ownership transfer instead.");
    }
}
