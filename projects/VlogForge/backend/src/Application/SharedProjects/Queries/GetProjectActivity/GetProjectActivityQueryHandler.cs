using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.SharedProjects.Queries.GetProjectActivity;

/// <summary>
/// Handler for getting activity feed for a shared project.
/// Story: ACF-013
/// </summary>
public sealed class GetProjectActivityQueryHandler
    : IRequestHandler<GetProjectActivityQuery, SharedProjectActivityListResponse>
{
    private readonly ISharedProjectRepository _repository;

    public GetProjectActivityQueryHandler(ISharedProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<SharedProjectActivityListResponse> Handle(
        GetProjectActivityQuery request,
        CancellationToken cancellationToken)
    {
        // Verify project exists and user has access
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            throw new SharedProjectNotFoundException(request.ProjectId);

        if (!project.IsMember(request.UserId))
            throw new SharedProjectAccessDeniedException(request.ProjectId, request.UserId);

        var (activities, totalCount) = await _repository.GetProjectActivityPagedAsync(
            request.ProjectId, request.Page, request.PageSize, cancellationToken);

        return new SharedProjectActivityListResponse
        {
            Items = activities.Select(SharedProjectActivityResponse.FromEntity).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
