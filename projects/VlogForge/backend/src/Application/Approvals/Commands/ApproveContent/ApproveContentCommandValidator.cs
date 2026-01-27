using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Approvals.Commands.ApproveContent;

/// <summary>
/// Validator for ApproveContentCommand.
/// Story: ACF-009
/// </summary>
public sealed class ApproveContentCommandValidator : AbstractValidator<ApproveContentCommand>
{
    public ApproveContentCommandValidator()
    {
        RuleFor(x => x.ContentItemId)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.ApproverId)
            .NotEmpty()
            .WithMessage("Approver ID is required.");

        RuleFor(x => x.Feedback)
            .MaximumLength(ApprovalRecord.MaxFeedbackLength)
            .When(x => x.Feedback != null)
            .WithMessage($"Feedback cannot exceed {ApprovalRecord.MaxFeedbackLength} characters.");
    }
}
