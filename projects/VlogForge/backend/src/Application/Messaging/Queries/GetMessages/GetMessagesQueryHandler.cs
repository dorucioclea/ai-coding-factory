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

        var items = new List<MessageResponse>();

        foreach (var message in messages)
        {
            var senderProfile = await _profileRepo.GetByUserIdAsync(message.SenderId, cancellationToken);
            items.Add(MessageResponse.FromEntity(message, senderProfile));
        }

        return new MessageListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
