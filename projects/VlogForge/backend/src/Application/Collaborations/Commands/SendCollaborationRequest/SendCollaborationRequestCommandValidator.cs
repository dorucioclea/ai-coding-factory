using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Collaborations.Commands.SendCollaborationRequest;

/// <summary>
/// Validator for SendCollaborationRequestCommand.
/// Story: ACF-011
/// </summary>
public sealed class SendCollaborationRequestCommandValidator : AbstractValidator<SendCollaborationRequestCommand>
{
    public SendCollaborationRequestCommandValidator()
    {
        RuleFor(x => x.SenderId)
            .NotEmpty()
            .WithMessage("Sender ID is required.");

        RuleFor(x => x.RecipientId)
            .NotEmpty()
            .WithMessage("Recipient ID is required.")
            .NotEqual(x => x.SenderId)
            .WithMessage("Cannot send a collaboration request to yourself.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(CollaborationRequest.MaxMessageLength)
            .WithMessage($"Message cannot exceed {CollaborationRequest.MaxMessageLength} characters.");
    }
}
