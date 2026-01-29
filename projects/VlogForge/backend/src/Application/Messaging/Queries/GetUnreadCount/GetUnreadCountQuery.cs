using MediatR;

namespace VlogForge.Application.Messaging.Queries.GetUnreadCount;

/// <summary>
/// Query to get total unread message count for a user.
/// Story: ACF-012
/// </summary>
public sealed record GetUnreadCountQuery(
    Guid UserId
) : IRequest<int>;
