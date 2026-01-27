using MediatR;
using VlogForge.Application.Approvals.DTOs;

namespace VlogForge.Application.Approvals.Queries.GetPendingApprovals;

/// <summary>
/// Query to get content items pending approval for a team.
/// Story: ACF-009
/// </summary>
public sealed record GetPendingApprovalsQuery(
    Guid TeamId,
    Guid UserId
) : IRequest<PendingApprovalsResponse>;
