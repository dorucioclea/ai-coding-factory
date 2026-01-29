using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Messaging.Commands.SendMessage;

/// <summary>
/// Handler for sending a message.
/// Story: ACF-012
/// </summary>
public sealed partial class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommand, MessageResponse>
{
    private readonly IConversationRepository _conversationRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly ICreatorProfileRepository _profileRepo;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepo,
        IMessageRepository messageRepo,
        ICreatorProfileRepository profileRepo,
        ILogger<SendMessageCommandHandler> logger)
    {
        _conversationRepo = conversationRepo;
        _messageRepo = messageRepo;
        _profileRepo = profileRepo;
        _logger = logger;
    }

    public async Task<MessageResponse> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        // Verify conversation exists
        var conversation = await _conversationRepo.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
            throw new ConversationNotFoundException(request.ConversationId);

        // Verify user is a participant
        if (!conversation.IsParticipant(request.UserId))
            throw new NotConversationParticipantException(request.ConversationId, request.UserId);

        // Check rate limit (60 messages per minute)
        var sentCount = await _messageRepo.CountSentInLastMinuteAsync(request.UserId, cancellationToken);
        if (sentCount >= Message.MaxMessagesPerMinute)
            throw new MessagingRateLimitExceededException(request.UserId);

        // Create message
        var message = Message.Create(request.ConversationId, request.UserId, request.Content);

        await _messageRepo.AddAsync(message, cancellationToken);

        // Update conversation last message
        conversation.UpdateLastMessage(message.Content);
        await _conversationRepo.UpdateAsync(conversation, cancellationToken);

        await _messageRepo.SaveChangesAsync(cancellationToken);

        // Get sender profile for response
        var senderProfile = await _profileRepo.GetByUserIdAsync(request.UserId, cancellationToken);

        LogMessageSent(_logger, message.Id, request.ConversationId, request.UserId);

        return MessageResponse.FromEntity(message, senderProfile);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Message {MessageId} sent in conversation {ConversationId} by {SenderId}")]
    private static partial void LogMessageSent(
        ILogger logger, Guid messageId, Guid conversationId, Guid senderId);
}
