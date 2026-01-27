using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Teams.Commands.InviteMember;

/// <summary>
/// Validator for InviteMemberCommand.
/// Story: ACF-007
/// </summary>
public sealed class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.InvitedByUserId)
            .NotEmpty()
            .WithMessage("Inviting user ID is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(254)
            .WithMessage("Email cannot exceed 254 characters.");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role specified.")
            .Must(r => r != TeamRole.Owner)
            .WithMessage("Cannot invite a member as Owner.");
    }
}
