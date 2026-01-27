using VlogForge.Domain.Entities;

namespace VlogForge.Api.Controllers.Tasks.Requests;

/// <summary>
/// Request to update task status.
/// Story: ACF-008
/// </summary>
public class UpdateTaskStatusRequest
{
    /// <summary>
    /// The new status for the task.
    /// </summary>
    public AssignmentStatus Status { get; init; }
}

/// <summary>
/// Request to add a comment to a task.
/// Story: ACF-008
/// </summary>
public class AddTaskCommentRequest
{
    /// <summary>
    /// The comment content.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Optional parent comment ID for threading.
    /// </summary>
    public Guid? ParentCommentId { get; init; }
}
