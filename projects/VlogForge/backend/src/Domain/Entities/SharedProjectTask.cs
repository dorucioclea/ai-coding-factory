using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Represents a task within a shared project.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectTask : Entity
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    /// <summary>
    /// Gets the shared project ID.
    /// </summary>
    public Guid SharedProjectId { get; private set; }

    /// <summary>
    /// Gets the user ID who created this task.
    /// </summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// Gets the task title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the task description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the current task status.
    /// </summary>
    public SharedProjectTaskStatus Status { get; private set; }

    /// <summary>
    /// Gets the assigned user ID.
    /// </summary>
    public Guid? AssigneeId { get; private set; }

    /// <summary>
    /// Gets the due date.
    /// </summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>
    /// Gets when the task was completed.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    private SharedProjectTask() : base()
    {
        Title = string.Empty;
    }

    private SharedProjectTask(Guid sharedProjectId, Guid createdByUserId, string title,
        string? description, Guid? assigneeId, DateTime? dueDate) : base()
    {
        SharedProjectId = sharedProjectId;
        CreatedByUserId = createdByUserId;
        Title = title;
        Description = description;
        Status = SharedProjectTaskStatus.Open;
        AssigneeId = assigneeId;
        DueDate = dueDate;
    }

    /// <summary>
    /// Creates a new shared project task.
    /// </summary>
    public static SharedProjectTask Create(Guid sharedProjectId, Guid createdByUserId,
        string title, string? description = null, Guid? assigneeId = null, DateTime? dueDate = null)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        if (dueDate.HasValue && dueDate.Value.Kind != DateTimeKind.Utc)
            dueDate = DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc);

        return new SharedProjectTask(sharedProjectId, createdByUserId, title.Trim(),
            description?.Trim(), assigneeId, dueDate);
    }

    /// <summary>
    /// Updates the task properties.
    /// </summary>
    public void Update(string? title = null, string? description = null,
        SharedProjectTaskStatus? status = null, Guid? assigneeId = null, DateTime? dueDate = null)
    {
        if (title is not null)
        {
            ValidateTitle(title);
            Title = title.Trim();
        }

        if (description is not null)
        {
            ValidateDescription(description);
            Description = description.Trim();
        }

        if (status.HasValue && status.Value != Status)
        {
            Status = status.Value;
            CompletedAt = status.Value == SharedProjectTaskStatus.Completed
                ? DateTime.UtcNow
                : null;
        }

        if (assigneeId.HasValue)
            AssigneeId = assigneeId.Value == Guid.Empty ? null : assigneeId;

        if (dueDate.HasValue)
        {
            DueDate = dueDate.Value.Kind == DateTimeKind.Utc
                ? dueDate.Value
                : DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc);
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty.", nameof(title));

        if (title.Trim().Length > MaxTitleLength)
            throw new ArgumentException($"Task title cannot exceed {MaxTitleLength} characters.", nameof(title));
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Task description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
    }
}
