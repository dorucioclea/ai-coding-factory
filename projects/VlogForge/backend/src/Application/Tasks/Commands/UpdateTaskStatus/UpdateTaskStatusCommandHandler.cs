using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Tasks.Commands.UpdateTaskStatus;

/// <summary>
/// Handler for UpdateTaskStatusCommand.
/// Story: ACF-008
/// </summary>
public sealed partial class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TaskAssignmentResponse>
{
    private readonly ITaskAssignmentRepository _taskRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<UpdateTaskStatusCommandHandler> _logger;

    public UpdateTaskStatusCommandHandler(
        ITaskAssignmentRepository taskRepository,
        ITeamRepository teamRepository,
        ILogger<UpdateTaskStatusCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<TaskAssignmentResponse> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            throw new TaskAssignmentNotFoundException(request.TaskId);
        }

        // Check if user has permission to update this task
        var team = await _teamRepository.GetByIdWithMembersAsync(task.TeamId, cancellationToken);
        var isTeamAdminOrOwner = team?.HasPermission(request.UpdatedByUserId, TeamAccessRight.ManageTeamSettings) ?? false;

        if (!task.CanModify(request.UpdatedByUserId, isTeamAdminOrOwner))
        {
            throw new TaskAccessDeniedException(request.TaskId, request.UpdatedByUserId);
        }

        var oldStatus = task.Status;
        task.UpdateStatus(request.NewStatus, request.UpdatedByUserId);

        await _taskRepository.UpdateAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        LogStatusUpdated(_logger, request.TaskId, oldStatus, request.NewStatus, request.UpdatedByUserId);

        return TaskAssignmentResponse.FromEntity(task);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Task {TaskId} status changed from {OldStatus} to {NewStatus} by user {UserId}")]
    private static partial void LogStatusUpdated(ILogger logger, Guid taskId, AssignmentStatus oldStatus, AssignmentStatus newStatus, Guid userId);
}
