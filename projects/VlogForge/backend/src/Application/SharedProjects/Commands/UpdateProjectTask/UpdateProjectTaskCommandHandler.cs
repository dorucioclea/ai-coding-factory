using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.SharedProjects.Commands.UpdateProjectTask;

/// <summary>
/// Handler for updating a task in a shared project.
/// Story: ACF-013
/// </summary>
public sealed partial class UpdateProjectTaskCommandHandler
    : IRequestHandler<UpdateProjectTaskCommand, SharedProjectDetailResponse>
{
    private readonly ISharedProjectRepository _repository;
    private readonly ILogger<UpdateProjectTaskCommandHandler> _logger;

    public UpdateProjectTaskCommandHandler(
        ISharedProjectRepository repository,
        ILogger<UpdateProjectTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SharedProjectDetailResponse> Handle(
        UpdateProjectTaskCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            throw new SharedProjectNotFoundException(request.ProjectId);

        project.UpdateTask(request.TaskId, request.UserId, request.Title, request.Description,
            request.Status, request.AssigneeId, request.DueDate);

        await _repository.UpdateAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogTaskUpdated(_logger, request.TaskId, request.ProjectId, request.UserId);

        return SharedProjectDetailResponse.FromEntity(project);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Task {TaskId} updated in project {ProjectId} by user {UserId}")]
    private static partial void LogTaskUpdated(ILogger logger, Guid taskId, Guid projectId, Guid userId);
}
