using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Application.Tasks.Commands.AddTaskComment;

/// <summary>
/// Handler for AddTaskCommentCommand.
/// Story: ACF-008
/// </summary>
public sealed partial class AddTaskCommentCommandHandler : IRequestHandler<AddTaskCommentCommand, TaskCommentResponse>
{
    private readonly ITaskAssignmentRepository _taskRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<AddTaskCommentCommandHandler> _logger;

    public AddTaskCommentCommandHandler(
        ITaskAssignmentRepository taskRepository,
        ITeamRepository teamRepository,
        ILogger<AddTaskCommentCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<TaskCommentResponse> Handle(AddTaskCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdWithCommentsAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            throw new TaskAssignmentNotFoundException(request.TaskId);
        }

        // Verify user is a team member (all team members can comment)
        var team = await _teamRepository.GetByIdWithMembersAsync(task.TeamId, cancellationToken);
        if (team is null)
        {
            throw new TeamNotFoundException(task.TeamId);
        }

        if (!team.IsMember(request.AuthorId))
        {
            throw new TeamAccessDeniedException("You must be a team member to comment on tasks.");
        }

        var comment = task.AddComment(request.AuthorId, request.Content, request.ParentCommentId);

        await _taskRepository.UpdateAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        LogCommentAdded(_logger, comment.Id, request.TaskId, request.AuthorId);

        return TaskCommentResponse.FromEntity(comment);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Comment {CommentId} added to task {TaskId} by user {AuthorId}")]
    private static partial void LogCommentAdded(ILogger logger, Guid commentId, Guid taskId, Guid authorId);
}
