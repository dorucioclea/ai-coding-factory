using MediatR;
using VlogForge.Application.Messaging.DTOs;

namespace VlogForge.Application.Messaging.Commands.SendMessage;

/// <summary>
/// Command to send a message in a conversation.
/// Story: ACF-012
/// </summary>
public sealed record SendMessageCommand(
    Guid UserId,
    Guid ConversationId,
    string Content
) : IRequest<MessageResponse>;
