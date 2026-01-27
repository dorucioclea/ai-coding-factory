using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using VlogForge.Domain.Exceptions;
using VlogForge.Domain.Interfaces;

namespace VlogForge.Application.Approvals.Commands.RequestChanges;

/// <summary>
/// Handler for RequestChangesCommand.
/// Story: ACF-009
/// </summary>
public sealed partial class RequestChangesCommandHandler : IRequestHandler<RequestChangesCommand, ContentIdeaResponse>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IApprovalRecordRepository _approvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestChangesCommandHandler> _logger;

    public RequestChangesCommandHandler(
        IContentItemRepository contentRepository,
        ITeamRepository teamRepository,
        IApprovalRecordRepository approvalRepository,
        IUnitOfWork unitOfWork,
        ILogger<RequestChangesCommandHandler> logger)
    {
        _contentRepository = contentRepository;
        _teamRepository = teamRepository;
        _approvalRepository = approvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ContentIdeaResponse> Handle(RequestChangesCommand request, CancellationToken cancellationToken)
    {
        // Get the content item
        var contentItem = await _contentRepository.GetByIdAsync(request.ContentItemId, cancellationToken: cancellationToken);
        if (contentItem is null)
        {
            throw new ContentItemNotFoundException(request.ContentItemId);
        }

        // Verify team exists
        var team = await _teamRepository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            throw new TeamNotFoundException(request.TeamId);
        }

        // Verify content was submitted to this team (SECURITY: prevent cross-team attacks)
        var latestApprovalRecord = await _approvalRepository.GetLatestByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (latestApprovalRecord is null || latestApprovalRecord.TeamId != request.TeamId)
        {
            throw new BusinessRuleException("InvalidTeam", "This content was not submitted for approval to this team.");
        }

        // Verify user can approve content (same permission required for requesting changes)
        if (!team.CanApproveContent(request.ReviewerId))
        {
            throw new TeamAccessDeniedException("You do not have permission to request changes on content in this team.");
        }

        // Verify content is in InReview status
        if (contentItem.Status != IdeaStatus.InReview)
        {
            throw new BusinessRuleException("InvalidStatus", "Only content in InReview status can have changes requested.");
        }

        var previousStatus = contentItem.Status;

        // Update status to ChangesRequested
        contentItem.UpdateStatus(IdeaStatus.ChangesRequested);

        // Create approval record
        var approvalRecord = ApprovalRecord.Create(
            request.ContentItemId,
            request.TeamId,
            request.ReviewerId,
            ApprovalAction.ChangesRequested,
            previousStatus,
            IdeaStatus.ChangesRequested,
            request.Feedback);

        // Raise domain event BEFORE persistence (will be dispatched after SaveChanges)
        contentItem.RaiseDomainEvent(new ChangesRequestedEvent(
            contentItem.Id,
            request.TeamId,
            request.ReviewerId,
            contentItem.UserId,
            request.Feedback));

        // Use transaction for atomicity
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _approvalRepository.AddAsync(approvalRecord, cancellationToken);
            await _contentRepository.UpdateAsync(contentItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        LogChangesRequested(_logger, request.ContentItemId, request.TeamId, request.ReviewerId);

        return ContentIdeaResponse.FromEntity(contentItem);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Changes requested on content {ContentItemId} in team {TeamId} by user {ReviewerId}")]
    private static partial void LogChangesRequested(ILogger logger, Guid contentItemId, Guid teamId, Guid reviewerId);
}
