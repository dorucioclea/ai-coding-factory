using MediatR;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Queries.GetTeam;

/// <summary>
/// Query to get a team by ID.
/// Story: ACF-007
/// </summary>
public sealed record GetTeamQuery(
    Guid TeamId,
    Guid RequestingUserId
) : IRequest<TeamWithMembersResponse?>;
