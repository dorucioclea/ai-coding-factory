using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using VlogForge.Domain.Exceptions;
using VlogForge.Domain.Interfaces;

namespace VlogForge.Application.Approvals.Commands.ApproveContent;

/// <summary>
/// Handler for ApproveContentCommand.
/// Story: ACF-009
/// </summary>
public sealed partial class ApproveContentCommandHandler : IRequestHandler<ApproveContentCommand, ContentIdeaResponse>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IApprovalRecordRepository _approvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveContentCommandHandler> _logger;

    public ApproveContentCommandHandler(
        IContentItemRepository contentRepository,
        ITeamRepository teamRepository,
        IApprovalRecordRepository approvalRepository,
        IUnitOfWork unitOfWork,
        ILogger<ApproveContentCommandHandler> logger)
    {
        _contentRepository = contentRepository;
        _teamRepository = teamRepository;
        _approvalRepository = approvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ContentIdeaResponse> Handle(ApproveContentCommand request, CancellationToken cancellationToken)
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

        // Verify content was submitted to this team (SECURITY: prevent cross-team approval attacks)
        var latestApprovalRecord = await _approvalRepository.GetLatestByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (latestApprovalRecord is null || latestApprovalRecord.TeamId != request.TeamId)
        {
            throw new BusinessRuleException("InvalidTeam", "This content was not submitted for approval to this team.");
        }

        // Verify user can approve content
        if (!team.CanApproveContent(request.ApproverId))
        {
            throw new TeamAccessDeniedException("You do not have permission to approve content in this team.");
        }

        // Verify content is in InReview status
        if (contentItem.Status != IdeaStatus.InReview)
        {
            throw new BusinessRuleException("InvalidStatus", "Only content in InReview status can be approved.");
        }

        var previousStatus = contentItem.Status;

        // Update status to Approved
        contentItem.UpdateStatus(IdeaStatus.Approved);

        // Create approval record
        var approvalRecord = ApprovalRecord.Create(
            request.ContentItemId,
            request.TeamId,
            request.ApproverId,
            ApprovalAction.Approved,
            previousStatus,
            IdeaStatus.Approved,
            request.Feedback);

        // Raise domain event BEFORE persistence (will be dispatched after SaveChanges)
        contentItem.RaiseDomainEvent(new ContentApprovedEvent(
            contentItem.Id,
            request.TeamId,
            request.ApproverId,
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

        LogContentApproved(_logger, request.ContentItemId, request.TeamId, request.ApproverId);

        return ContentIdeaResponse.FromEntity(contentItem);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Content {ContentItemId} approved in team {TeamId} by user {ApproverId}")]
    private static partial void LogContentApproved(ILogger logger, Guid contentItemId, Guid teamId, Guid approverId);
}
