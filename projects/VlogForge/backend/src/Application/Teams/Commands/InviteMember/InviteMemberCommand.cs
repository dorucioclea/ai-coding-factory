using MediatR;
using VlogForge.Application.Teams.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Teams.Commands.InviteMember;

/// <summary>
/// Command to invite a member to a team.
/// Story: ACF-007
/// </summary>
public sealed record InviteMemberCommand(
    Guid TeamId,
    Guid InvitedByUserId,
    string Email,
    TeamRole Role
) : IRequest<TeamInvitationResponse>;
