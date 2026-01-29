using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Messaging.Commands.MarkMessagesAsRead;

/// <summary>
/// Handler for marking messages as read.
/// Story: ACF-012
/// </summary>
public sealed partial class MarkMessagesAsReadCommandHandler
    : IRequestHandler<MarkMessagesAsReadCommand, int>
{
    private readonly IConversationRepository _conversationRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly ILogger<MarkMessagesAsReadCommandHandler> _logger;

    public MarkMessagesAsReadCommandHandler(
        IConversationRepository conversationRepo,
        IMessageRepository messageRepo,
        ILogger<MarkMessagesAsReadCommandHandler> logger)
    {
        _conversationRepo = conversationRepo;
        _messageRepo = messageRepo;
        _logger = logger;
    }

    public async Task<int> Handle(
        MarkMessagesAsReadCommand request,
        CancellationToken cancellationToken)
    {
        // Verify conversation exists
        var conversation = await _conversationRepo.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
            throw new ConversationNotFoundException(request.ConversationId);

        // Verify user is a participant
        if (!conversation.IsParticipant(request.UserId))
            throw new NotConversationParticipantException(request.ConversationId, request.UserId);

        // Mark messages as read using bulk ExecuteUpdateAsync for performance.
        // This bypasses EF change tracker, so per-message MessageReadEvent is not raised.
        // If read-receipt notifications are needed, publish a single aggregate event here.
        var count = await _messageRepo.MarkConversationMessagesAsReadAsync(
            request.ConversationId, request.UserId, cancellationToken);

        if (count > 0)
            LogMessagesMarkedAsRead(_logger, count, request.ConversationId, request.UserId);

        return count;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "{Count} messages marked as read in conversation {ConversationId} by {UserId}")]
    private static partial void LogMessagesMarkedAsRead(
        ILogger logger, int count, Guid conversationId, Guid userId);
}
