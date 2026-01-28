using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Collaborations.DTOs;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Collaborations.Commands.SendCollaborationRequest;

/// <summary>
/// Handler for sending a collaboration request.
/// Story: ACF-011
/// </summary>
public sealed partial class SendCollaborationRequestCommandHandler
    : IRequestHandler<SendCollaborationRequestCommand, CollaborationRequestResponse>
{
    private readonly ICollaborationRequestRepository _collaborationRepo;
    private readonly ICreatorProfileRepository _profileRepo;
    private readonly ILogger<SendCollaborationRequestCommandHandler> _logger;

    public SendCollaborationRequestCommandHandler(
        ICollaborationRequestRepository collaborationRepo,
        ICreatorProfileRepository profileRepo,
        ILogger<SendCollaborationRequestCommandHandler> logger)
    {
        _collaborationRepo = collaborationRepo;
        _profileRepo = profileRepo;
        _logger = logger;
    }

    public async Task<CollaborationRequestResponse> Handle(
        SendCollaborationRequestCommand request,
        CancellationToken cancellationToken)
    {
        // Verify sender has a profile
        var senderProfile = await _profileRepo.GetByUserIdAsync(request.SenderId, cancellationToken);
        if (senderProfile is null)
            throw new EntityNotFoundException("CreatorProfile", request.SenderId);

        // Verify recipient has a profile
        var recipientProfile = await _profileRepo.GetByUserIdAsync(request.RecipientId, cancellationToken);
        if (recipientProfile is null)
            throw new EntityNotFoundException("CreatorProfile", request.RecipientId);

        // Verify recipient is open to collaborations
        if (!recipientProfile.OpenToCollaborations)
            throw new RecipientNotOpenToCollaborationsException(request.RecipientId);

        // Check rate limit (max 5 per day)
        var sentToday = await _collaborationRepo.CountSentTodayAsync(request.SenderId, cancellationToken);
        if (sentToday >= CollaborationRequest.MaxRequestsPerDayPerUser)
            throw new CollaborationRateLimitExceededException(request.SenderId);

        // Check for duplicate pending request
        var duplicateExists = await _collaborationRepo.ExistsPendingBetweenAsync(
            request.SenderId, request.RecipientId, cancellationToken);
        if (duplicateExists)
            throw new DuplicateCollaborationRequestException();

        // Create the request
        var collaborationRequest = CollaborationRequest.Create(
            request.SenderId,
            request.RecipientId,
            request.Message);

        await _collaborationRepo.AddAsync(collaborationRequest, cancellationToken);
        await _collaborationRepo.SaveChangesAsync(cancellationToken);

        LogRequestSent(_logger, collaborationRequest.Id, request.SenderId, request.RecipientId);

        return CollaborationRequestResponse.FromEntity(
            collaborationRequest, senderProfile, recipientProfile);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Collaboration request {RequestId} sent from {SenderId} to {RecipientId}")]
    private static partial void LogRequestSent(
        ILogger logger, Guid requestId, Guid senderId, Guid recipientId);
}
