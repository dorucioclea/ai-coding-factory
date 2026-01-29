using FluentValidation;

namespace VlogForge.Application.Messaging.Commands.StartConversation;

/// <summary>
/// Validator for StartConversationCommand.
/// Story: ACF-012
/// </summary>
public sealed class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.ParticipantId)
            .NotEmpty()
            .WithMessage("Participant ID is required.")
            .NotEqual(x => x.UserId)
            .WithMessage("Cannot start a conversation with yourself.");
    }
}
