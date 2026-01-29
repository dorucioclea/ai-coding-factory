using MediatR;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Messaging.Queries.GetUnreadCount;

/// <summary>
/// Handler for getting total unread message count.
/// Story: ACF-012
/// </summary>
public sealed class GetUnreadCountQueryHandler
    : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly IMessageRepository _messageRepo;

    public GetUnreadCountQueryHandler(IMessageRepository messageRepo)
    {
        _messageRepo = messageRepo;
    }

    public async Task<int> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        return await _messageRepo.GetUnreadCountAsync(request.UserId, cancellationToken);
    }
}
