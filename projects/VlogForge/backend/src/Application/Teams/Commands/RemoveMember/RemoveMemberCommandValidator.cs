using FluentValidation;

namespace VlogForge.Application.Teams.Commands.RemoveMember;

/// <summary>
/// Validator for RemoveMemberCommand.
/// Story: ACF-007
/// </summary>
public sealed class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.MemberUserId)
            .NotEmpty()
            .WithMessage("Member user ID is required.");

        RuleFor(x => x.RemovedByUserId)
            .NotEmpty()
            .WithMessage("Removed by user ID is required.");
    }
}
