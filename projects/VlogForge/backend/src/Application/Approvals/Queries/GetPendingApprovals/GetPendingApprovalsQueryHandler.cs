using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Approvals.DTOs;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Approvals.Queries.GetPendingApprovals;

/// <summary>
/// Handler for GetPendingApprovalsQuery.
/// Story: ACF-009
/// </summary>
public sealed partial class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, PendingApprovalsResponse>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IApprovalRecordRepository _approvalRepository;
    private readonly ILogger<GetPendingApprovalsQueryHandler> _logger;

    public GetPendingApprovalsQueryHandler(
        IContentItemRepository contentRepository,
        ITeamRepository teamRepository,
        IApprovalRecordRepository approvalRepository,
        ILogger<GetPendingApprovalsQueryHandler> logger)
    {
        _contentRepository = contentRepository;
        _teamRepository = teamRepository;
        _approvalRepository = approvalRepository;
        _logger = logger;
    }

    public async Task<PendingApprovalsResponse> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        // Verify team exists
        var team = await _teamRepository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            throw new TeamNotFoundException(request.TeamId);
        }

        // Verify user can approve content (only approvers see pending items)
        if (!team.CanApproveContent(request.UserId))
        {
            throw new TeamAccessDeniedException("You do not have permission to view pending approvals.");
        }

        // Get content items in InReview status for this team
        var pendingItems = await _contentRepository.GetPendingApprovalForTeamAsync(request.TeamId, cancellationToken);

        if (pendingItems.Count == 0)
        {
            LogNoPendingApprovals(_logger, request.TeamId);
            return new PendingApprovalsResponse
            {
                Items = [],
                TotalCount = 0
            };
        }

        // Batch fetch all approval records in a single query (avoids N+1 problem)
        var contentIds = pendingItems.Select(p => p.Id).ToList();
        var latestRecords = await _approvalRepository.GetLatestByContentItemIdsAsync(contentIds, cancellationToken);

        var items = pendingItems.Select(item =>
        {
            latestRecords.TryGetValue(item.Id, out var latestRecord);
            return new PendingApprovalItem
            {
                ContentItemId = item.Id,
                Title = item.Title,
                Notes = item.Notes,
                SubmittedByUserId = item.UserId,
                SubmittedAt = latestRecord?.CreatedAt ?? item.CreatedAt,
                PlatformTags = item.PlatformTags.ToList()
            };
        }).ToList();

        LogPendingApprovalsRetrieved(_logger, request.TeamId, items.Count, request.UserId);

        return new PendingApprovalsResponse
        {
            Items = items,
            TotalCount = items.Count
        };
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "No pending approvals found for team {TeamId}")]
    private static partial void LogNoPendingApprovals(ILogger logger, Guid teamId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieved {Count} pending approvals for team {TeamId} by user {UserId}")]
    private static partial void LogPendingApprovalsRetrieved(ILogger logger, Guid teamId, int count, Guid userId);
}
