using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Queries.GetTeamMembers;

/// <summary>
/// Handler for GetTeamMembersQuery.
/// Story: ACF-007
/// </summary>
public sealed class GetTeamMembersQueryHandler : IRequestHandler<GetTeamMembersQuery, IReadOnlyList<TeamMemberResponse>>
{
    private readonly ITeamRepository _repository;

    public GetTeamMembersQueryHandler(ITeamRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TeamMemberResponse>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);

        if (team is null)
            throw new InvalidOperationException("Team not found.");

        // Only members can view other members
        if (!team.IsMember(request.RequestingUserId))
            throw new UnauthorizedAccessException("You are not a member of this team.");

        return team.Members.Select(TeamMemberResponse.FromEntity).ToList();
    }
}
