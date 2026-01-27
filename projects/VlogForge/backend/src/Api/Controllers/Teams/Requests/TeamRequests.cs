using VlogForge.Domain.Entities;

namespace VlogForge.Api.Controllers.Teams.Requests;

/// <summary>
/// Request to create a team.
/// Story: ACF-007
/// </summary>
public sealed record CreateTeamRequest(
    string Name,
    string? Description = null
);

/// <summary>
/// Request to update a team.
/// Story: ACF-007
/// </summary>
public sealed record UpdateTeamRequest(
    string Name,
    string? Description = null
);

/// <summary>
/// Request to invite a member to a team.
/// Story: ACF-007
/// </summary>
public sealed record InviteMemberRequest(
    string Email,
    TeamRole Role
);

/// <summary>
/// Request to accept a team invitation.
/// Story: ACF-007
/// </summary>
public sealed record AcceptInvitationRequest(
    string Token
);

/// <summary>
/// Request to change a member's role.
/// Story: ACF-007
/// </summary>
public sealed record ChangeMemberRoleRequest(
    TeamRole NewRole
);
