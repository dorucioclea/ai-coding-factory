using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Collaborations.DTOs;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Collaborations.Commands.DeclineCollaborationRequest;

/// <summary>
/// Handler for declining a collaboration request.
/// Story: ACF-011
/// </summary>
public sealed partial class DeclineCollaborationRequestCommandHandler
    : IRequestHandler<DeclineCollaborationRequestCommand, CollaborationRequestResponse>
{
    private readonly ICollaborationRequestRepository _collaborationRepo;
    private readonly ICreatorProfileRepository _profileRepo;
    private readonly ILogger<DeclineCollaborationRequestCommandHandler> _logger;

    public DeclineCollaborationRequestCommandHandler(
        ICollaborationRequestRepository collaborationRepo,
        ICreatorProfileRepository profileRepo,
        ILogger<DeclineCollaborationRequestCommandHandler> logger)
    {
        _collaborationRepo = collaborationRepo;
        _profileRepo = profileRepo;
        _logger = logger;
    }

    public async Task<CollaborationRequestResponse> Handle(
        DeclineCollaborationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var collaborationRequest = await _collaborationRepo.GetByIdAsync(
            request.RequestId, cancellationToken);

        if (collaborationRequest is null)
            throw new CollaborationRequestNotFoundException(request.RequestId);

        // Fetch profiles BEFORE state change to avoid inconsistency if profile fetch fails after save
        var senderProfile = await _profileRepo.GetByUserIdAsync(
            collaborationRequest.SenderId, cancellationToken);
        var recipientProfile = await _profileRepo.GetByUserIdAsync(
            collaborationRequest.RecipientId, cancellationToken);

        collaborationRequest.Decline(request.UserId, request.Reason);

        await _collaborationRepo.UpdateAsync(collaborationRequest, cancellationToken);
        await _collaborationRepo.SaveChangesAsync(cancellationToken);

        LogRequestDeclined(_logger, request.RequestId, request.UserId);

        return CollaborationRequestResponse.FromEntity(
            collaborationRequest, senderProfile, recipientProfile);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Collaboration request {RequestId} declined by {UserId}")]
    private static partial void LogRequestDeclined(ILogger logger, Guid requestId, Guid userId);
}
