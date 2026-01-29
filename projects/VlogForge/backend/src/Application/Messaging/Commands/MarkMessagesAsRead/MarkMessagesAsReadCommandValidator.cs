using FluentValidation;

namespace VlogForge.Application.Messaging.Commands.MarkMessagesAsRead;

/// <summary>
/// Validator for MarkMessagesAsReadCommand.
/// Story: ACF-012
/// </summary>
public sealed class MarkMessagesAsReadCommandValidator : AbstractValidator<MarkMessagesAsReadCommand>
{
    public MarkMessagesAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("Conversation ID is required.");
    }
}
