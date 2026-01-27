using MediatR;
using VlogForge.Application.Teams.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Teams.Commands.ChangeMemberRole;

/// <summary>
/// Command to change a team member's role.
/// Story: ACF-007
/// </summary>
public sealed record ChangeMemberRoleCommand(
    Guid TeamId,
    Guid MemberUserId,
    TeamRole NewRole,
    Guid ChangedByUserId
) : IRequest<TeamMemberResponse>;
