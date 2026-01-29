using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Messaging.Commands.StartConversation;

/// <summary>
/// Handler for starting a conversation.
/// Story: ACF-012
/// </summary>
public sealed partial class StartConversationCommandHandler
    : IRequestHandler<StartConversationCommand, ConversationResponse>
{
    private readonly IConversationRepository _conversationRepo;
    private readonly ICollaborationRequestRepository _collaborationRepo;
    private readonly ITeamRepository _teamRepo;
    private readonly ICreatorProfileRepository _profileRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly ILogger<StartConversationCommandHandler> _logger;

    public StartConversationCommandHandler(
        IConversationRepository conversationRepo,
        ICollaborationRequestRepository collaborationRepo,
        ITeamRepository teamRepo,
        ICreatorProfileRepository profileRepo,
        IMessageRepository messageRepo,
        ILogger<StartConversationCommandHandler> logger)
    {
        _conversationRepo = conversationRepo;
        _collaborationRepo = collaborationRepo;
        _teamRepo = teamRepo;
        _profileRepo = profileRepo;
        _messageRepo = messageRepo;
        _logger = logger;
    }

    public async Task<ConversationResponse> Handle(
        StartConversationCommand request,
        CancellationToken cancellationToken)
    {
        // Verify both users have profiles
        var userProfile = await _profileRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (userProfile is null)
            throw new EntityNotFoundException("CreatorProfile", request.UserId);

        var participantProfile = await _profileRepo.GetByUserIdAsync(request.ParticipantId, cancellationToken);
        if (participantProfile is null)
            throw new EntityNotFoundException("CreatorProfile", request.ParticipantId);

        // Check for existing conversation
        var existingConversation = await _conversationRepo.GetByParticipantsAsync(
            request.UserId, request.ParticipantId, cancellationToken);

        if (existingConversation is not null)
        {
            var unreadCount = await _messageRepo.GetUnreadCountForConversationAsync(
                existingConversation.Id, request.UserId, cancellationToken);

            return ConversationResponse.FromEntity(
                existingConversation, request.UserId, participantProfile, unreadCount);
        }

        // Verify mutual collaboration: accepted request in either direction OR shared team membership
        var hasCollaboration = await HasMutualCollaborationAsync(
            request.UserId, request.ParticipantId, cancellationToken);

        if (!hasCollaboration)
            throw new NoMutualCollaborationException(request.UserId, request.ParticipantId);

        // Create new conversation
        var conversation = Conversation.Create(request.UserId, request.ParticipantId);

        await _conversationRepo.AddAsync(conversation, cancellationToken);
        await _conversationRepo.SaveChangesAsync(cancellationToken);

        LogConversationCreated(_logger, conversation.Id, request.UserId, request.ParticipantId);

        return ConversationResponse.FromEntity(conversation, request.UserId, participantProfile, 0);
    }

    private async Task<bool> HasMutualCollaborationAsync(
        Guid userId, Guid participantId, CancellationToken cancellationToken)
    {
        // Check for accepted collaboration request in either direction
        var (sentRequests, _) = await _collaborationRepo.GetSentRequestsAsync(
            userId, CollaborationRequestStatus.Accepted, 1, 100, cancellationToken);

        if (sentRequests.Any(r => r.RecipientId == participantId))
            return true;

        var (receivedRequests, _) = await _collaborationRepo.GetReceivedRequestsAsync(
            userId, CollaborationRequestStatus.Accepted, 1, 100, cancellationToken);

        if (receivedRequests.Any(r => r.SenderId == participantId))
            return true;

        // Check shared team membership
        var hasSharedTeam = await _teamRepo.AreInSameTeamAsync(userId, participantId, cancellationToken);
        return hasSharedTeam;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Conversation {ConversationId} created between {UserId} and {ParticipantId}")]
    private static partial void LogConversationCreated(
        ILogger logger, Guid conversationId, Guid userId, Guid participantId);
}
