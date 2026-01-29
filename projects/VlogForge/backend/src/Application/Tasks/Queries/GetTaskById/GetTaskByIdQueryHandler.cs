using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Handler for GetTaskByIdQuery.
/// Stories: ACF-008, ACF-014
/// </summary>
public sealed partial class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskAssignmentResponse>
{
    private readonly ITaskAssignmentRepository _taskRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<GetTaskByIdQueryHandler> _logger;

    public GetTaskByIdQueryHandler(
        ITaskAssignmentRepository taskRepository,
        ITeamRepository teamRepository,
        ILogger<GetTaskByIdQueryHandler> logger)
    {
        _taskRepository = taskRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<TaskAssignmentResponse> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await LoadTaskAsync(request, cancellationToken);

        if (task is null)
        {
            throw new TaskAssignmentNotFoundException(request.TaskId);
        }

        // Verify user has access (is a team member)
        var team = await _teamRepository.GetByIdWithMembersAsync(task.TeamId, cancellationToken);
        if (team is null || !team.IsMember(request.RequestingUserId))
        {
            throw new TeamAccessDeniedException("You must be a team member to view this task.");
        }

        LogTaskRetrieved(_logger, request.TaskId, request.RequestingUserId);

        return TaskAssignmentResponse.FromEntity(task, request.IncludeComments, request.IncludeHistory);
    }

    private async Task<Domain.Entities.TaskAssignment?> LoadTaskAsync(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.IncludeComments && request.IncludeHistory)
        {
            return await _taskRepository.GetByIdWithCommentsAndHistoryAsync(request.TaskId, cancellationToken);
        }

        if (request.IncludeHistory)
        {
            return await _taskRepository.GetByIdWithHistoryAsync(request.TaskId, cancellationToken);
        }

        if (request.IncludeComments)
        {
            return await _taskRepository.GetByIdWithCommentsAsync(request.TaskId, cancellationToken);
        }

        return await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Task {TaskId} retrieved by user {UserId}")]
    private static partial void LogTaskRetrieved(ILogger logger, Guid taskId, Guid userId);
}
