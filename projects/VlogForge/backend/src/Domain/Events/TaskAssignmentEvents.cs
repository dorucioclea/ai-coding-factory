using VlogForge.Domain.Common;
using VlogForge.Domain.Entities;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when a task is assigned.
/// Story: ACF-008
/// </summary>
public sealed class TaskAssignedEvent : DomainEvent
{
    public Guid TaskAssignmentId { get; }
    public Guid ContentItemId { get; }
    public Guid TeamId { get; }
    public Guid AssigneeId { get; }
    public Guid AssignedById { get; }
    public DateTime DueDate { get; }

    public TaskAssignedEvent(
        Guid taskAssignmentId,
        Guid contentItemId,
        Guid teamId,
        Guid assigneeId,
        Guid assignedById,
        DateTime dueDate)
    {
        TaskAssignmentId = taskAssignmentId;
        ContentItemId = contentItemId;
        TeamId = teamId;
        AssigneeId = assigneeId;
        AssignedById = assignedById;
        DueDate = dueDate;
    }
}

/// <summary>
/// Event raised when a task status changes.
/// Story: ACF-008
/// </summary>
public sealed class TaskStatusChangedEvent : DomainEvent
{
    public Guid TaskAssignmentId { get; }
    public Guid ContentItemId { get; }
    public Guid TeamId { get; }
    public Guid AssigneeId { get; }
    public Guid AssignedById { get; }
    public AssignmentStatus OldStatus { get; }
    public AssignmentStatus NewStatus { get; }
    public Guid ChangedByUserId { get; }

    public TaskStatusChangedEvent(
        Guid taskAssignmentId,
        Guid contentItemId,
        Guid teamId,
        Guid assigneeId,
        Guid assignedById,
        AssignmentStatus oldStatus,
        AssignmentStatus newStatus,
        Guid changedByUserId)
    {
        TaskAssignmentId = taskAssignmentId;
        ContentItemId = contentItemId;
        TeamId = teamId;
        AssigneeId = assigneeId;
        AssignedById = assignedById;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
    }
}

/// <summary>
/// Event raised when a task due date changes.
/// Story: ACF-008
/// </summary>
public sealed class TaskDueDateChangedEvent : DomainEvent
{
    public Guid TaskAssignmentId { get; }
    public DateTime OldDueDate { get; }
    public DateTime NewDueDate { get; }
    public Guid ChangedByUserId { get; }

    public TaskDueDateChangedEvent(
        Guid taskAssignmentId,
        DateTime oldDueDate,
        DateTime newDueDate,
        Guid changedByUserId)
    {
        TaskAssignmentId = taskAssignmentId;
        OldDueDate = oldDueDate;
        NewDueDate = newDueDate;
        ChangedByUserId = changedByUserId;
    }
}

/// <summary>
/// Event raised when a task is reassigned.
/// Story: ACF-008
/// </summary>
public sealed class TaskReassignedEvent : DomainEvent
{
    public Guid TaskAssignmentId { get; }
    public Guid OldAssigneeId { get; }
    public Guid NewAssigneeId { get; }
    public Guid ReassignedByUserId { get; }

    public TaskReassignedEvent(
        Guid taskAssignmentId,
        Guid oldAssigneeId,
        Guid newAssigneeId,
        Guid reassignedByUserId)
    {
        TaskAssignmentId = taskAssignmentId;
        OldAssigneeId = oldAssigneeId;
        NewAssigneeId = newAssigneeId;
        ReassignedByUserId = reassignedByUserId;
    }
}

/// <summary>
/// Event raised when a comment is added to a task.
/// Story: ACF-008
/// </summary>
public sealed class TaskCommentAddedEvent : DomainEvent
{
    public Guid TaskAssignmentId { get; }
    public Guid CommentId { get; }
    public Guid TeamId { get; }
    public Guid AuthorId { get; }
    public Guid? ParentCommentId { get; }

    public TaskCommentAddedEvent(
        Guid taskAssignmentId,
        Guid commentId,
        Guid teamId,
        Guid authorId,
        Guid? parentCommentId)
    {
        TaskAssignmentId = taskAssignmentId;
        CommentId = commentId;
        TeamId = teamId;
        AuthorId = authorId;
        ParentCommentId = parentCommentId;
    }
}
