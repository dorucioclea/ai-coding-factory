using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Commands.ChangeMemberRole;

/// <summary>
/// Handler for ChangeMemberRoleCommand.
/// Story: ACF-007
/// </summary>
public sealed partial class ChangeMemberRoleCommandHandler : IRequestHandler<ChangeMemberRoleCommand, TeamMemberResponse>
{
    private readonly ITeamRepository _repository;
    private readonly ILogger<ChangeMemberRoleCommandHandler> _logger;

    public ChangeMemberRoleCommandHandler(
        ITeamRepository repository,
        ILogger<ChangeMemberRoleCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TeamMemberResponse> Handle(ChangeMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            throw new InvalidOperationException("Team not found.");
        }

        // Change member role (domain validates permissions)
        team.ChangeMemberRole(request.MemberUserId, request.NewRole, request.ChangedByUserId);

        await _repository.UpdateAsync(team, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var member = team.GetMember(request.MemberUserId)!;

        LogRoleChanged(_logger, request.TeamId, request.MemberUserId, request.NewRole.ToString());

        return TeamMemberResponse.FromEntity(member);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Changed role of user {UserId} in team {TeamId} to {NewRole}")]
    private static partial void LogRoleChanged(ILogger logger, Guid teamId, Guid userId, string newRole);
}
