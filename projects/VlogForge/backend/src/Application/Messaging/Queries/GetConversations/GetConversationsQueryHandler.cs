using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.DTOs;

namespace VlogForge.Application.Messaging.Queries.GetConversations;

/// <summary>
/// Handler for getting a user's conversations.
/// Story: ACF-012
/// </summary>
public sealed class GetConversationsQueryHandler
    : IRequestHandler<GetConversationsQuery, ConversationListResponse>
{
    private readonly IConversationRepository _conversationRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly ICreatorProfileRepository _profileRepo;

    public GetConversationsQueryHandler(
        IConversationRepository conversationRepo,
        IMessageRepository messageRepo,
        ICreatorProfileRepository profileRepo)
    {
        _conversationRepo = conversationRepo;
        _messageRepo = messageRepo;
        _profileRepo = profileRepo;
    }

    public async Task<ConversationListResponse> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var (conversations, totalCount) = await _conversationRepo.GetUserConversationsAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        var items = new List<ConversationResponse>();

        foreach (var conversation in conversations)
        {
            var otherParticipantId = conversation.GetOtherParticipantId(request.UserId);
            var participantProfile = await _profileRepo.GetByUserIdAsync(otherParticipantId, cancellationToken);
            var unreadCount = await _messageRepo.GetUnreadCountForConversationAsync(
                conversation.Id, request.UserId, cancellationToken);

            items.Add(ConversationResponse.FromEntity(
                conversation, request.UserId, participantProfile, unreadCount));
        }

        return new ConversationListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
