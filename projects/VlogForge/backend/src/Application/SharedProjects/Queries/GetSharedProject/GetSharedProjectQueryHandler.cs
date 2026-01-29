using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.SharedProjects.Queries.GetSharedProject;

/// <summary>
/// Handler for getting a shared project by ID.
/// Story: ACF-013
/// </summary>
public sealed class GetSharedProjectQueryHandler
    : IRequestHandler<GetSharedProjectQuery, SharedProjectDetailResponse?>
{
    private readonly ISharedProjectRepository _repository;

    public GetSharedProjectQueryHandler(ISharedProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<SharedProjectDetailResponse?> Handle(
        GetSharedProjectQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            return null;

        if (!project.IsMember(request.UserId))
            throw new SharedProjectAccessDeniedException(request.ProjectId, request.UserId);

        return SharedProjectDetailResponse.FromEntity(project);
    }
}
