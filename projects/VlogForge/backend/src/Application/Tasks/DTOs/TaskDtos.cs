using VlogForge.Domain.Entities;

namespace VlogForge.Application.Tasks.DTOs;

/// <summary>
/// Response DTO for task assignment information.
/// Story: ACF-008
/// </summary>
public sealed class TaskAssignmentResponse
{
    public Guid Id { get; init; }
    public Guid ContentItemId { get; init; }
    public Guid TeamId { get; init; }
    public Guid AssigneeId { get; init; }
    public Guid AssignedById { get; init; }
    public DateTime DueDate { get; init; }
    public AssignmentStatus Status { get; init; }
    public string? Notes { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsOverdue { get; init; }
    public IReadOnlyList<TaskCommentResponse> Comments { get; init; } = new List<TaskCommentResponse>();

    public static TaskAssignmentResponse FromEntity(TaskAssignment task, bool includeComments = false)
    {
        return new TaskAssignmentResponse
        {
            Id = task.Id,
            ContentItemId = task.ContentItemId,
            TeamId = task.TeamId,
            AssigneeId = task.AssigneeId,
            AssignedById = task.AssignedById,
            DueDate = task.DueDate,
            Status = task.Status,
            Notes = task.Notes,
            CompletedAt = task.CompletedAt,
            CreatedAt = task.CreatedAt,
            IsOverdue = task.IsOverdue(),
            Comments = includeComments
                ? task.Comments.Select(TaskCommentResponse.FromEntity).ToList()
                : new List<TaskCommentResponse>()
        };
    }
}

/// <summary>
/// Response DTO for task comment.
/// Story: ACF-008
/// </summary>
public sealed class TaskCommentResponse
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid? ParentCommentId { get; init; }
    public bool IsEdited { get; init; }
    public DateTime? EditedAt { get; init; }
    public DateTime CreatedAt { get; init; }

    public static TaskCommentResponse FromEntity(TaskComment comment)
    {
        return new TaskCommentResponse
        {
            Id = comment.Id,
            AuthorId = comment.AuthorId,
            Content = comment.Content,
            ParentCommentId = comment.ParentCommentId,
            IsEdited = comment.IsEdited,
            EditedAt = comment.EditedAt,
            CreatedAt = comment.CreatedAt
        };
    }
}

/// <summary>
/// Response for task list with pagination.
/// Story: ACF-008
/// </summary>
public sealed class TaskListResponse
{
    public IReadOnlyList<TaskAssignmentResponse> Items { get; init; } = new List<TaskAssignmentResponse>();
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Response for task history entry.
/// Story: ACF-008
/// </summary>
public sealed class TaskHistoryResponse
{
    public Guid Id { get; init; }
    public Guid ChangedByUserId { get; init; }
    public TaskHistoryAction Action { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public static TaskHistoryResponse FromEntity(TaskHistory history)
    {
        return new TaskHistoryResponse
        {
            Id = history.Id,
            ChangedByUserId = history.ChangedByUserId,
            Action = history.Action,
            Description = history.Description,
            CreatedAt = history.CreatedAt
        };
    }
}
