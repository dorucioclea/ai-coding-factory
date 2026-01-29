using MediatR;
using VlogForge.Application.Messaging.DTOs;

namespace VlogForge.Application.Messaging.Queries.GetConversations;

/// <summary>
/// Query to get a user's conversations.
/// Story: ACF-012
/// </summary>
public sealed record GetConversationsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<ConversationListResponse>;
