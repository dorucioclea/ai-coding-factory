using MediatR;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Queries.GetUserTeams;

/// <summary>
/// Query to get teams a user belongs to with pagination support.
/// Story: ACF-007
/// </summary>
public sealed record GetUserTeamsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<TeamListResponse>;
