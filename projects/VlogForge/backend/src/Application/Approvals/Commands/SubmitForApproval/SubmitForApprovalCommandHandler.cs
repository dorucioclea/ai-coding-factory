using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using VlogForge.Domain.Exceptions;
using VlogForge.Domain.Interfaces;

namespace VlogForge.Application.Approvals.Commands.SubmitForApproval;

/// <summary>
/// Handler for SubmitForApprovalCommand.
/// Story: ACF-009
/// </summary>
public sealed partial class SubmitForApprovalCommandHandler : IRequestHandler<SubmitForApprovalCommand, ContentIdeaResponse>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IApprovalRecordRepository _approvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubmitForApprovalCommandHandler> _logger;

    public SubmitForApprovalCommandHandler(
        IContentItemRepository contentRepository,
        ITeamRepository teamRepository,
        IApprovalRecordRepository approvalRepository,
        IUnitOfWork unitOfWork,
        ILogger<SubmitForApprovalCommandHandler> logger)
    {
        _contentRepository = contentRepository;
        _teamRepository = teamRepository;
        _approvalRepository = approvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ContentIdeaResponse> Handle(SubmitForApprovalCommand request, CancellationToken cancellationToken)
    {
        // Get the content item
        var contentItem = await _contentRepository.GetByIdAsync(request.ContentItemId, cancellationToken: cancellationToken);
        if (contentItem is null)
        {
            throw new ContentItemNotFoundException(request.ContentItemId);
        }

        // Verify user is the content owner
        if (contentItem.UserId != request.UserId)
        {
            throw new ForbiddenAccessException("Only the content owner can submit for approval.");
        }

        // Verify team exists and user is a member
        var team = await _teamRepository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            throw new TeamNotFoundException(request.TeamId);
        }

        if (!team.IsMember(request.UserId))
        {
            throw new TeamAccessDeniedException("You must be a team member to submit content for approval.");
        }

        // Verify team requires approval
        if (!team.RequiresApproval)
        {
            throw new BusinessRuleException("WorkflowNotEnabled", "This team does not have approval workflow enabled.");
        }

        // Verify content is in Draft status
        if (contentItem.Status != IdeaStatus.Draft && contentItem.Status != IdeaStatus.ChangesRequested)
        {
            throw new BusinessRuleException("InvalidStatus", "Only content in Draft or ChangesRequested status can be submitted for approval.");
        }

        var previousStatus = contentItem.Status;

        // Update status to InReview
        contentItem.UpdateStatus(IdeaStatus.InReview);

        // Determine action type
        var action = previousStatus == IdeaStatus.ChangesRequested
            ? ApprovalAction.Resubmitted
            : ApprovalAction.Submitted;

        // Create approval record
        var approvalRecord = ApprovalRecord.Create(
            request.ContentItemId,
            request.TeamId,
            request.UserId,
            action,
            previousStatus,
            IdeaStatus.InReview);

        // Raise domain event BEFORE persistence (will be dispatched after SaveChanges)
        contentItem.RaiseDomainEvent(new ContentSubmittedForApprovalEvent(
            contentItem.Id,
            request.TeamId,
            request.UserId,
            previousStatus));

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

        LogContentSubmitted(_logger, request.ContentItemId, request.TeamId, request.UserId);

        return ContentIdeaResponse.FromEntity(contentItem);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Content {ContentItemId} submitted for approval in team {TeamId} by user {UserId}")]
    private static partial void LogContentSubmitted(ILogger logger, Guid contentItemId, Guid teamId, Guid userId);
}
