using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Approvals.Commands.RequestChanges;

/// <summary>
/// Validator for RequestChangesCommand.
/// Story: ACF-009
/// </summary>
public sealed class RequestChangesCommandValidator : AbstractValidator<RequestChangesCommand>
{
    public RequestChangesCommandValidator()
    {
        RuleFor(x => x.ContentItemId)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("Team ID is required.");

        RuleFor(x => x.ReviewerId)
            .NotEmpty()
            .WithMessage("Reviewer ID is required.");

        RuleFor(x => x.Feedback)
            .NotEmpty()
            .WithMessage("Feedback is required when requesting changes.")
            .MaximumLength(ApprovalRecord.MaxFeedbackLength)
            .WithMessage($"Feedback cannot exceed {ApprovalRecord.MaxFeedbackLength} characters.");
    }
}
