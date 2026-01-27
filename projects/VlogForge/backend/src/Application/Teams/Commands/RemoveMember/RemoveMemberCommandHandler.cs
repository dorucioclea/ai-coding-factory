using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Teams.Commands.RemoveMember;

/// <summary>
/// Handler for RemoveMemberCommand.
/// Story: ACF-007
/// </summary>
public sealed partial class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, bool>
{
    private readonly ITeamRepository _repository;
    private readonly ILogger<RemoveMemberCommandHandler> _logger;

    public RemoveMemberCommandHandler(
        ITeamRepository repository,
        ILogger<RemoveMemberCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            throw new InvalidOperationException("Team not found.");
        }

        // Remove member (domain validates permissions)
        team.RemoveMember(request.MemberUserId, request.RemovedByUserId);

        await _repository.UpdateAsync(team, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogMemberRemoved(_logger, request.TeamId, request.MemberUserId, request.RemovedByUserId);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Removed user {UserId} from team {TeamId} by {RemovedByUserId}")]
    private static partial void LogMemberRemoved(ILogger logger, Guid teamId, Guid userId, Guid removedByUserId);
}
