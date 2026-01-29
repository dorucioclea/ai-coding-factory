using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.SharedProjects.Commands.AddProjectTask;

/// <summary>
/// Handler for adding a task to a shared project.
/// Story: ACF-013
/// </summary>
public sealed partial class AddProjectTaskCommandHandler
    : IRequestHandler<AddProjectTaskCommand, SharedProjectTaskResponse>
{
    private readonly ISharedProjectRepository _repository;
    private readonly ILogger<AddProjectTaskCommandHandler> _logger;

    public AddProjectTaskCommandHandler(
        ISharedProjectRepository repository,
        ILogger<AddProjectTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SharedProjectTaskResponse> Handle(
        AddProjectTaskCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            throw new SharedProjectNotFoundException(request.ProjectId);

        var task = project.AddTask(request.UserId, request.Title, request.Description,
            request.AssigneeId, request.DueDate);

        await _repository.UpdateAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        LogTaskAdded(_logger, task.Id, request.ProjectId, request.UserId);

        return SharedProjectTaskResponse.FromEntity(task);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Task {TaskId} added to project {ProjectId} by user {UserId}")]
    private static partial void LogTaskAdded(ILogger logger, Guid taskId, Guid projectId, Guid userId);
}
