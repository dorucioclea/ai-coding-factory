using MediatR;

namespace VlogForge.Application.Messaging.Commands.MarkMessagesAsRead;

/// <summary>
/// Command to mark all unread messages in a conversation as read.
/// Story: ACF-012
/// </summary>
public sealed record MarkMessagesAsReadCommand(
    Guid UserId,
    Guid ConversationId
) : IRequest<int>;
