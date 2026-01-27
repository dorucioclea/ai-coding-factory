using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Queries.GetTeam;

/// <summary>
/// Handler for GetTeamQuery.
/// Story: ACF-007
/// </summary>
public sealed class GetTeamQueryHandler : IRequestHandler<GetTeamQuery, TeamWithMembersResponse?>
{
    private readonly ITeamRepository _repository;

    public GetTeamQueryHandler(ITeamRepository repository)
    {
        _repository = repository;
    }

    public async Task<TeamWithMembersResponse?> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);

        if (team is null)
            return null;

        // Only members can view team details
        if (!team.IsMember(request.RequestingUserId))
            throw new UnauthorizedAccessException("You are not a member of this team.");

        return TeamWithMembersResponse.FromEntity(team);
    }
}
