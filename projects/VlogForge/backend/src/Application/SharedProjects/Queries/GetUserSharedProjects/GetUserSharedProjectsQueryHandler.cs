using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.DTOs;

namespace VlogForge.Application.SharedProjects.Queries.GetUserSharedProjects;

/// <summary>
/// Handler for getting shared projects for a user.
/// Story: ACF-013
/// </summary>
public sealed class GetUserSharedProjectsQueryHandler
    : IRequestHandler<GetUserSharedProjectsQuery, SharedProjectListResponse>
{
    private readonly ISharedProjectRepository _repository;

    public GetUserSharedProjectsQueryHandler(ISharedProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<SharedProjectListResponse> Handle(
        GetUserSharedProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var (projects, totalCount) = await _repository.GetByMemberUserIdPagedAsync(
            request.UserId, request.Status, request.Page, request.PageSize, cancellationToken);

        return new SharedProjectListResponse
        {
            Items = projects.Select(SharedProjectResponse.FromEntity).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
