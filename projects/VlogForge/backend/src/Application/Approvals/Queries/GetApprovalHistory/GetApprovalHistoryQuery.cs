using MediatR;
using VlogForge.Application.Approvals.DTOs;

namespace VlogForge.Application.Approvals.Queries.GetApprovalHistory;

/// <summary>
/// Query to get approval history for a content item.
/// Story: ACF-009
/// </summary>
public sealed record GetApprovalHistoryQuery(
    Guid ContentItemId,
    Guid UserId
) : IRequest<ApprovalHistoryResponse>;
