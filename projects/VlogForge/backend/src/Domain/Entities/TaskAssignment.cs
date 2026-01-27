using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Assignment status workflow for content tasks.
/// Story: ACF-008
/// </summary>
public enum AssignmentStatus
{
    /// <summary>Task has not been started.</summary>
    NotStarted = 0,

    /// <summary>Task is currently being worked on.</summary>
    InProgress = 1,

    /// <summary>Task has been completed.</summary>
    Completed = 2
}

/// <summary>
/// Entity representing a task assignment for content items.
/// Story: ACF-008
/// </summary>
public sealed class TaskAssignment : AggregateRoot
{
    public const int MaxNotesLength = 2000;
    public const int MaxComments = 100;

    private readonly List<TaskComment> _comments = new();
    private readonly List<TaskHistory> _history = new();

    /// <summary>
    /// Gets the content item ID this task is for.
    /// </summary>
    public Guid ContentItemId { get; private set; }

    /// <summary>
    /// Gets the team ID this task belongs to.
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the user ID of the assigned team member.
    /// </summary>
    public Guid AssigneeId { get; private set; }

    /// <summary>
    /// Gets the user ID of who assigned the task.
    /// </summary>
    public Guid AssignedById { get; private set; }

    /// <summary>
    /// Gets the due date for the task.
    /// </summary>
    public DateTime DueDate { get; private set; }

    /// <summary>
    /// Gets the current status of the task.
    /// </summary>
    public AssignmentStatus Status { get; private set; }

    /// <summary>
    /// Gets optional notes for the task.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets when the task was completed.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Gets the comments on this task.
    /// </summary>
    public IReadOnlyCollection<TaskComment> Comments => _comments.AsReadOnly();

    /// <summary>
    /// Gets the history entries for this task.
    /// </summary>
    public IReadOnlyCollection<TaskHistory> History => _history.AsReadOnly();

    private TaskAssignment() : base()
    {
    }

    private TaskAssignment(
        Guid contentItemId,
        Guid teamId,
        Guid assigneeId,
        Guid assignedById,
        DateTime dueDate,
        string? notes) : base()
    {
        ContentItemId = contentItemId;
        TeamId = teamId;
        AssigneeId = assigneeId;
        AssignedById = assignedById;
        DueDate = dueDate;
        Notes = notes;
        Status = AssignmentStatus.NotStarted;
    }

    /// <summary>
    /// Creates a new task assignment.
    /// </summary>
    /// <param name="contentItemId">The content item to assign.</param>
    /// <param name="teamId">The team the task belongs to.</param>
    /// <param name="assigneeId">The user ID of the assignee.</param>
    /// <param name="assignedById">The user ID of who is assigning.</param>
    /// <param name="dueDate">The due date for the task.</param>
    /// <param name="notes">Optional notes for the task.</param>
    /// <returns>A new TaskAssignment instance.</returns>
    public static TaskAssignment Create(
        Guid contentItemId,
        Guid teamId,
        Guid assigneeId,
        Guid assignedById,
        DateTime dueDate,
        string? notes = null)
    {
        if (contentItemId == Guid.Empty)
            throw new ArgumentException("Content item ID cannot be empty.", nameof(contentItemId));

        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));

        if (assigneeId == Guid.Empty)
            throw new ArgumentException("Assignee ID cannot be empty.", nameof(assigneeId));

        if (assignedById == Guid.Empty)
            throw new ArgumentException("Assigned by ID cannot be empty.", nameof(assignedById));

        ValidateNotes(notes);

        // Normalize to UTC - convert if not already UTC
        var normalizedDueDate = dueDate.Kind == DateTimeKind.Utc
            ? dueDate
            : dueDate.ToUniversalTime();

        var task = new TaskAssignment(
            contentItemId,
            teamId,
            assigneeId,
            assignedById,
            normalizedDueDate,
            notes?.Trim());

        // Record initial history
        task._history.Add(TaskHistory.Create(
            task.Id,
            assignedById,
            TaskHistoryAction.Created,
            $"Task created and assigned"));

        task.RaiseDomainEvent(new TaskAssignedEvent(
            task.Id,
            contentItemId,
            teamId,
            assigneeId,
            assignedById,
            normalizedDueDate));

        return task;
    }

    /// <summary>
    /// Updates the task status.
    /// </summary>
    /// <param name="newStatus">The new status.</param>
    /// <param name="updatedByUserId">The user making the update.</param>
    public void UpdateStatus(AssignmentStatus newStatus, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty.", nameof(updatedByUserId));

        if (!Enum.IsDefined(newStatus))
            throw new ArgumentException("Invalid status value.", nameof(newStatus));

        if (Status == newStatus)
            return;

        var oldStatus = Status;
        Status = newStatus;
        IncrementVersion();

        if (newStatus == AssignmentStatus.Completed)
        {
            CompletedAt = DateTime.UtcNow;
        }
        else if (oldStatus == AssignmentStatus.Completed)
        {
            // Reopening the task
            CompletedAt = null;
        }

        _history.Add(TaskHistory.Create(
            Id,
            updatedByUserId,
            TaskHistoryAction.StatusChanged,
            $"Status changed from {oldStatus} to {newStatus}"));

        RaiseDomainEvent(new TaskStatusChangedEvent(
            Id,
            ContentItemId,
            TeamId,
            AssigneeId,
            AssignedById,
            oldStatus,
            newStatus,
            updatedByUserId));
    }

    /// <summary>
    /// Updates the due date.
    /// </summary>
    /// <param name="newDueDate">The new due date.</param>
    /// <param name="updatedByUserId">The user making the update.</param>
    public void UpdateDueDate(DateTime newDueDate, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty.", nameof(updatedByUserId));

        var normalizedDueDate = newDueDate.Kind == DateTimeKind.Utc
            ? newDueDate
            : newDueDate.ToUniversalTime();

        if (Math.Abs((DueDate - normalizedDueDate).TotalSeconds) < 1)
            return;

        var oldDueDate = DueDate;
        DueDate = normalizedDueDate;
        IncrementVersion();

        _history.Add(TaskHistory.Create(
            Id,
            updatedByUserId,
            TaskHistoryAction.DueDateChanged,
            $"Due date changed from {oldDueDate:yyyy-MM-dd} to {normalizedDueDate:yyyy-MM-dd}"));

        RaiseDomainEvent(new TaskDueDateChangedEvent(
            Id,
            oldDueDate,
            normalizedDueDate,
            updatedByUserId));
    }

    /// <summary>
    /// Reassigns the task to a different team member.
    /// </summary>
    /// <param name="newAssigneeId">The new assignee's user ID.</param>
    /// <param name="reassignedByUserId">The user making the reassignment.</param>
    public void Reassign(Guid newAssigneeId, Guid reassignedByUserId)
    {
        if (newAssigneeId == Guid.Empty)
            throw new ArgumentException("New assignee ID cannot be empty.", nameof(newAssigneeId));

        if (reassignedByUserId == Guid.Empty)
            throw new ArgumentException("Reassigned by user ID cannot be empty.", nameof(reassignedByUserId));

        if (AssigneeId == newAssigneeId)
            return;

        var oldAssigneeId = AssigneeId;
        AssigneeId = newAssigneeId;
        IncrementVersion();

        _history.Add(TaskHistory.Create(
            Id,
            reassignedByUserId,
            TaskHistoryAction.Reassigned,
            "Task reassigned"));

        RaiseDomainEvent(new TaskReassignedEvent(
            Id,
            oldAssigneeId,
            newAssigneeId,
            reassignedByUserId));
    }

    /// <summary>
    /// Adds a comment to the task.
    /// </summary>
    /// <param name="authorId">The user ID of the comment author.</param>
    /// <param name="content">The comment content.</param>
    /// <param name="parentCommentId">Optional parent comment ID for threading.</param>
    /// <returns>The created comment.</returns>
    public TaskComment AddComment(Guid authorId, string content, Guid? parentCommentId = null)
    {
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty.", nameof(authorId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content cannot be empty.", nameof(content));

        if (_comments.Count >= MaxComments)
            throw new InvalidOperationException($"Task cannot have more than {MaxComments} comments.");

        // Validate parent comment exists if specified
        if (parentCommentId.HasValue)
        {
            var parentExists = _comments.Any(c => c.Id == parentCommentId.Value);
            if (!parentExists)
                throw new ArgumentException("Parent comment not found.", nameof(parentCommentId));
        }

        var comment = TaskComment.Create(Id, authorId, content, parentCommentId);
        _comments.Add(comment);
        IncrementVersion();

        _history.Add(TaskHistory.Create(
            Id,
            authorId,
            TaskHistoryAction.CommentAdded,
            "Comment added"));

        RaiseDomainEvent(new TaskCommentAddedEvent(
            Id,
            comment.Id,
            TeamId,
            authorId,
            parentCommentId));

        return comment;
    }

    /// <summary>
    /// Checks if the task is overdue.
    /// </summary>
    /// <returns>True if the task is overdue and not completed.</returns>
    public bool IsOverdue()
    {
        return Status != AssignmentStatus.Completed && DueDate < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a user can modify this task.
    /// Only assignee, assigner, or team admins/owners can modify.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="isTeamAdminOrOwner">Whether the user is a team admin or owner.</param>
    /// <returns>True if the user can modify the task.</returns>
    public bool CanModify(Guid userId, bool isTeamAdminOrOwner)
    {
        return userId == AssigneeId ||
               userId == AssignedById ||
               isTeamAdminOrOwner;
    }

    private static void ValidateNotes(string? notes)
    {
        if (notes is not null && notes.Length > MaxNotesLength)
            throw new ArgumentException($"Notes cannot exceed {MaxNotesLength} characters.", nameof(notes));
    }
}

/// <summary>
/// Entity representing a comment on a task.
/// Story: ACF-008
/// </summary>
public sealed class TaskComment : Entity
{
    public const int MaxContentLength = 2000;

    /// <summary>
    /// Gets the task ID this comment belongs to.
    /// </summary>
    public Guid TaskAssignmentId { get; private set; }

    /// <summary>
    /// Gets the user ID of the author.
    /// </summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// Gets the comment content.
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Gets the parent comment ID for threading.
    /// </summary>
    public Guid? ParentCommentId { get; private set; }

    /// <summary>
    /// Gets whether the comment was edited.
    /// </summary>
    public bool IsEdited { get; private set; }

    /// <summary>
    /// Gets when the comment was edited.
    /// </summary>
    public DateTime? EditedAt { get; private set; }

    private TaskComment() : base()
    {
        Content = string.Empty;
    }

    private TaskComment(
        Guid taskAssignmentId,
        Guid authorId,
        string content,
        Guid? parentCommentId) : base()
    {
        TaskAssignmentId = taskAssignmentId;
        AuthorId = authorId;
        Content = content;
        ParentCommentId = parentCommentId;
        IsEdited = false;
    }

    internal static TaskComment Create(
        Guid taskAssignmentId,
        Guid authorId,
        string content,
        Guid? parentCommentId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        if (content.Length > MaxContentLength)
            throw new ArgumentException($"Content cannot exceed {MaxContentLength} characters.", nameof(content));

        return new TaskComment(taskAssignmentId, authorId, content.Trim(), parentCommentId);
    }

    /// <summary>
    /// Edits the comment content.
    /// </summary>
    /// <param name="newContent">The new content.</param>
    /// <param name="editorId">The user making the edit.</param>
    public void Edit(string newContent, Guid editorId)
    {
        if (editorId != AuthorId)
            throw new UnauthorizedAccessException("Only the author can edit this comment.");

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Content cannot be empty.", nameof(newContent));

        if (newContent.Length > MaxContentLength)
            throw new ArgumentException($"Content cannot exceed {MaxContentLength} characters.", nameof(newContent));

        Content = newContent.Trim();
        IsEdited = true;
        EditedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Entity representing a history entry for task changes.
/// Story: ACF-008
/// </summary>
public sealed class TaskHistory : Entity
{
    /// <summary>
    /// Gets the task ID this history entry belongs to.
    /// </summary>
    public Guid TaskAssignmentId { get; private set; }

    /// <summary>
    /// Gets the user ID who made the change.
    /// </summary>
    public Guid ChangedByUserId { get; private set; }

    /// <summary>
    /// Gets the action that was performed.
    /// </summary>
    public TaskHistoryAction Action { get; private set; }

    /// <summary>
    /// Gets the description of the change.
    /// </summary>
    public string Description { get; private set; }

    private TaskHistory() : base()
    {
        Description = string.Empty;
    }

    private TaskHistory(
        Guid taskAssignmentId,
        Guid changedByUserId,
        TaskHistoryAction action,
        string description) : base()
    {
        TaskAssignmentId = taskAssignmentId;
        ChangedByUserId = changedByUserId;
        Action = action;
        Description = description;
    }

    internal static TaskHistory Create(
        Guid taskAssignmentId,
        Guid changedByUserId,
        TaskHistoryAction action,
        string description)
    {
        return new TaskHistory(taskAssignmentId, changedByUserId, action, description);
    }
}

/// <summary>
/// Types of actions recorded in task history.
/// Story: ACF-008
/// </summary>
public enum TaskHistoryAction
{
    /// <summary>Task was created.</summary>
    Created = 0,

    /// <summary>Task status was changed.</summary>
    StatusChanged = 1,

    /// <summary>Task was reassigned.</summary>
    Reassigned = 2,

    /// <summary>Due date was changed.</summary>
    DueDateChanged = 3,

    /// <summary>Comment was added.</summary>
    CommentAdded = 4
}
