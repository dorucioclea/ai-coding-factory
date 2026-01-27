using MediatR;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Queries.GetTeamMembers;

/// <summary>
/// Query to get team members.
/// Story: ACF-007
/// </summary>
public sealed record GetTeamMembersQuery(
    Guid TeamId,
    Guid RequestingUserId
) : IRequest<IReadOnlyList<TeamMemberResponse>>;
