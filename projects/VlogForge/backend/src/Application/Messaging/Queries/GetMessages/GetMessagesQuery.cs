using MediatR;
using VlogForge.Application.Messaging.DTOs;

namespace VlogForge.Application.Messaging.Queries.GetMessages;

/// <summary>
/// Query to get messages in a conversation.
/// Story: ACF-012
/// </summary>
public sealed record GetMessagesQuery(
    Guid UserId,
    Guid ConversationId,
    int Page = 1,
    int PageSize = 50
) : IRequest<MessageListResponse>;
