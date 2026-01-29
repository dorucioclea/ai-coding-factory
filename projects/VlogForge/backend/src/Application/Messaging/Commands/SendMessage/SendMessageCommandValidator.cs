using FluentValidation;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Messaging.Commands.SendMessage;

/// <summary>
/// Validator for SendMessageCommand.
/// Story: ACF-012
/// </summary>
public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("Conversation ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Message content is required.")
            .MaximumLength(Message.MaxContentLength)
            .WithMessage($"Message content cannot exceed {Message.MaxContentLength} characters.");
    }
}
