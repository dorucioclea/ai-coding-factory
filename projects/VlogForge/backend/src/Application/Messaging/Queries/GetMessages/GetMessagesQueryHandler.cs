using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Messaging.Queries.GetMessages;

/// <summary>
/// Handler for getting messages in a conversation.
/// Story: ACF-012
/// </summary>
public sealed class GetMessagesQueryHandler
    : IRequestHandler<GetMessagesQuery, MessageListResponse>
{
    private readonly IConversationRepository _conversationRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly ICreatorProfileRepository _profileRepo;

    public GetMessagesQueryHandler(
        IConversationRepository conversationRepo,
        IMessageRepository messageRepo,
        ICreatorProfileRepository profileRepo)
    {
        _conversationRepo = conversationRepo;
        _messageRepo = messageRepo;
        _profileRepo = profileRepo;
    }

    public async Task<MessageListResponse> Handle(
        GetMessagesQuery request,
        CancellationToken cancellationToken)
    {
        // Verify conversation exists
        var conversation = await _conversationRepo.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
            throw new ConversationNotFoundException(request.ConversationId);

        // Verify user is a participant
        if (!conversation.IsParticipant(request.UserId))
            throw new NotConversationParticipantException(request.ConversationId, request.UserId);

        var (messages, totalCount) = await _messageRepo.GetConversationMessagesAsync(
            request.ConversationId, request.Page, request.PageSize, cancellationToken);

        // Batch-fetch sender profiles to avoid N+1 queries
        var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
        var profileMap = await _profileRepo.GetByUserIdsAsync(senderIds, cancellationToken);

        var items = messages.Select(message =>
        {
            profileMap.TryGetValue(message.SenderId, out var senderProfile);
            return MessageResponse.FromEntity(message, senderProfile);
        }).ToList();

        return new MessageListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
