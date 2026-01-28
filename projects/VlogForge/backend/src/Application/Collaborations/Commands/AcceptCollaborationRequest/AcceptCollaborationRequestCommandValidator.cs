using FluentValidation;

namespace VlogForge.Application.Collaborations.Commands.AcceptCollaborationRequest;

/// <summary>
/// Validator for AcceptCollaborationRequestCommand.
/// Story: ACF-011
/// </summary>
public sealed class AcceptCollaborationRequestCommandValidator
    : AbstractValidator<AcceptCollaborationRequestCommand>
{
    public AcceptCollaborationRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
