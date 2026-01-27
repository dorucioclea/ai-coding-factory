using MediatR;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Commands.AcceptInvitation;

/// <summary>
/// Command to accept a team invitation.
/// Story: ACF-007
/// </summary>
public sealed record AcceptInvitationCommand(
    string Token,
    Guid UserId,
    string UserEmail
) : IRequest<TeamMemberResponse>;
