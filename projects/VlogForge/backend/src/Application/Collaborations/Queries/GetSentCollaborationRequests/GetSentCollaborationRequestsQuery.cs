using MediatR;
using VlogForge.Application.Collaborations.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Collaborations.Queries.GetSentCollaborationRequests;

/// <summary>
/// Query to get a user's sent collaboration requests.
/// Story: ACF-011
/// </summary>
public sealed record GetSentCollaborationRequestsQuery(
    Guid UserId,
    CollaborationRequestStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<CollaborationRequestListResponse>;
