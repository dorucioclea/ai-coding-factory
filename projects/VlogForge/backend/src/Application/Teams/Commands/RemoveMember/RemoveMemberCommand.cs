using MediatR;

namespace VlogForge.Application.Teams.Commands.RemoveMember;

/// <summary>
/// Command to remove a member from a team.
/// Story: ACF-007
/// </summary>
public sealed record RemoveMemberCommand(
    Guid TeamId,
    Guid MemberUserId,
    Guid RemovedByUserId
) : IRequest<bool>;
