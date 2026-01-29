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

        // Batch-fetch profiles and unread counts to avoid N+1 queries
        var participantIds = conversations
            .Select(c => c.GetOtherParticipantId(request.UserId))
            .Distinct()
            .ToList();
        var conversationIds = conversations.Select(c => c.Id).ToList();

        var profileMap = await _profileRepo.GetByUserIdsAsync(participantIds, cancellationToken);
        var unreadMap = await _messageRepo.GetUnreadCountsForConversationsAsync(
            conversationIds, request.UserId, cancellationToken);

        var items = conversations.Select(conversation =>
        {
            var otherParticipantId = conversation.GetOtherParticipantId(request.UserId);
            profileMap.TryGetValue(otherParticipantId, out var participantProfile);
            unreadMap.TryGetValue(conversation.Id, out var unreadCount);

            return ConversationResponse.FromEntity(
                conversation, request.UserId, participantProfile, unreadCount);
        }).ToList();

        return new ConversationListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
