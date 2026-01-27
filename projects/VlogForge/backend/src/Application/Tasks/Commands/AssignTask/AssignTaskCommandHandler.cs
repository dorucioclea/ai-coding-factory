using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Tasks.Commands.AssignTask;

/// <summary>
/// Handler for AssignTaskCommand.
/// Story: ACF-008
/// </summary>
public sealed partial class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, TaskAssignmentResponse>
{
    private readonly ITaskAssignmentRepository _taskRepository;
    private readonly IContentItemRepository _contentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<AssignTaskCommandHandler> _logger;

    public AssignTaskCommandHandler(
        ITaskAssignmentRepository taskRepository,
        IContentItemRepository contentRepository,
        ITeamRepository teamRepository,
        ILogger<AssignTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _contentRepository = contentRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<TaskAssignmentResponse> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        // Verify content item exists
        var contentItem = await _contentRepository.GetByIdAsync(request.ContentItemId, cancellationToken: cancellationToken);
        if (contentItem is null)
        {
            throw new ContentItemNotFoundException(request.ContentItemId);
        }

        // Verify team exists and user has permission to assign
        var team = await _teamRepository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            throw new TeamNotFoundException(request.TeamId);
        }

        // Check if assigner has permission to assign tasks
        if (!team.HasPermission(request.AssignedById, TeamAccessRight.AssignTasks))
        {
            throw new TeamAccessDeniedException("You do not have permission to assign tasks in this team.");
        }

        // Check if assignee is a team member
        if (!team.IsMember(request.AssigneeId))
        {
            throw new TeamMemberNotFoundException(request.TeamId, request.AssigneeId);
        }

        // Check if content item already has an active assignment
        if (await _taskRepository.ExistsActiveForContentItemAsync(request.ContentItemId, cancellationToken))
        {
            throw new BusinessRuleException("DuplicateAssignment", "This content item already has an active task assignment.");
        }

        // Create the task assignment
        var task = TaskAssignment.Create(
            request.ContentItemId,
            request.TeamId,
            request.AssigneeId,
            request.AssignedById,
            request.DueDate,
            request.Notes);

        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        LogTaskAssigned(_logger, task.Id, request.ContentItemId, request.AssigneeId, request.AssignedById);

        return TaskAssignmentResponse.FromEntity(task);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Task {TaskId} assigned for content {ContentItemId} to user {AssigneeId} by user {AssignedById}")]
    private static partial void LogTaskAssigned(ILogger logger, Guid taskId, Guid contentItemId, Guid assigneeId, Guid assignedById);
}
