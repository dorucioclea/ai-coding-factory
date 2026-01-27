using MediatR;
using VlogForge.Application.Tasks.DTOs;

namespace VlogForge.Application.Tasks.Commands.AddTaskComment;

/// <summary>
/// Command to add a comment to a task.
/// Story: ACF-008
/// </summary>
public sealed record AddTaskCommentCommand(
    Guid TaskId,
    Guid AuthorId,
    string Content,
    Guid? ParentCommentId = null
) : IRequest<TaskCommentResponse>;
