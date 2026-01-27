using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.DTOs;

namespace VlogForge.Application.Teams.Queries.GetUserTeams;

/// <summary>
/// Handler for GetUserTeamsQuery with pagination support.
/// Story: ACF-007
/// </summary>
public sealed class GetUserTeamsQueryHandler : IRequestHandler<GetUserTeamsQuery, TeamListResponse>
{
    private const int MaxPageSize = 100;
    private readonly ITeamRepository _repository;

    public GetUserTeamsQueryHandler(ITeamRepository repository)
    {
        _repository = repository;
    }

    public async Task<TeamListResponse> Handle(GetUserTeamsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var (teams, totalCount) = await _repository.GetByMemberUserIdPagedAsync(
            request.UserId,
            page,
            pageSize,
            cancellationToken);

        return new TeamListResponse
        {
            Items = teams.Select(TeamResponse.FromEntity).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
