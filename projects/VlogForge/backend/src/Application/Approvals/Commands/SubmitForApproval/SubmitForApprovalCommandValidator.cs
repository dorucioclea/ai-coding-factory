using FluentValidation;

namespace VlogForge.Application.Approvals.Commands.SubmitForApproval;

/// <summary>
/// Validator for SubmitForApprovalCommand.
/// Story: ACF-009
/// </summary>
public sealed class SubmitForApprovalCommandValidator : AbstractValidator<SubmitForApprovalCommand>
{
    public SubmitForApprovalCommandValidator()
    {
        RuleFor(x => x.ContentItemId)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
