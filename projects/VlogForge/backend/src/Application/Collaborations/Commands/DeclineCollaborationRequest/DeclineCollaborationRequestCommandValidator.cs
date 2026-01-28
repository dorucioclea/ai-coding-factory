using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Collaborations.Commands.DeclineCollaborationRequest;

/// <summary>
/// Validator for DeclineCollaborationRequestCommand.
/// Story: ACF-011
/// </summary>
public sealed class DeclineCollaborationRequestCommandValidator
    : AbstractValidator<DeclineCollaborationRequestCommand>
{
    public DeclineCollaborationRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(CollaborationRequest.MaxDeclineReasonLength)
            .WithMessage($"Decline reason cannot exceed {CollaborationRequest.MaxDeclineReasonLength} characters.")
            .When(x => x.Reason is not null);
    }
}
