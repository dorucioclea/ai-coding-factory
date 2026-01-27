using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Approvals.DTOs;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Approvals.Queries.GetApprovalHistory;

/// <summary>
/// Handler for GetApprovalHistoryQuery.
/// Story: ACF-009
/// </summary>
public sealed partial class GetApprovalHistoryQueryHandler : IRequestHandler<GetApprovalHistoryQuery, ApprovalHistoryResponse>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly IApprovalRecordRepository _approvalRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<GetApprovalHistoryQueryHandler> _logger;

    public GetApprovalHistoryQueryHandler(
        IContentItemRepository contentRepository,
        IApprovalRecordRepository approvalRepository,
        ITeamRepository teamRepository,
        ILogger<GetApprovalHistoryQueryHandler> logger)
    {
        _contentRepository = contentRepository;
        _approvalRepository = approvalRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<ApprovalHistoryResponse> Handle(GetApprovalHistoryQuery request, CancellationToken cancellationToken)
    {
        // Verify content item exists
        var contentItem = await _contentRepository.GetByIdAsync(request.ContentItemId, cancellationToken: cancellationToken);
        if (contentItem is null)
        {
            throw new ContentItemNotFoundException(request.ContentItemId);
        }

        // Get all approval records for this content
        var records = await _approvalRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);

        // Authorization: Allow content owner OR team approvers to view history
        var isOwner = contentItem.UserId == request.UserId;
        var isTeamApprover = false;

        if (!isOwner && records.Count > 0)
        {
            // Check if user is an approver in any team this content was submitted to
            var teamIds = records.Select(r => r.TeamId).Distinct();
            foreach (var teamId in teamIds)
            {
                var team = await _teamRepository.GetByIdWithMembersAsync(teamId, cancellationToken);
                if (team is not null && team.CanApproveContent(request.UserId))
                {
                    isTeamApprover = true;
                    break;
                }
            }
        }

        if (!isOwner && !isTeamApprover)
        {
            throw new ForbiddenAccessException("You must be the content owner or a team approver to view approval history.");
        }

        LogApprovalHistoryRetrieved(_logger, request.ContentItemId, records.Count, request.UserId);

        return new ApprovalHistoryResponse
        {
            ContentItemId = request.ContentItemId,
            Records = records.Select(ApprovalRecordResponse.FromEntity).ToList()
        };
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieved {Count} approval history records for content {ContentItemId} by user {UserId}")]
    private static partial void LogApprovalHistoryRetrieved(ILogger logger, Guid contentItemId, int count, Guid userId);
}
