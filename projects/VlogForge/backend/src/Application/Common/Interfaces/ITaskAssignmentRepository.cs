using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for TaskAssignment aggregate operations.
/// Story: ACF-008
/// </summary>
public interface ITaskAssignmentRepository
{
    /// <summary>
    /// Gets a task assignment by its unique identifier.
    /// </summary>
    /// <param name="id">The task assignment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TaskAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task assignment by ID with comments included.
    /// </summary>
    /// <param name="id">The task assignment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TaskAssignment?> GetByIdWithCommentsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task assignment by ID with history included.
    /// </summary>
    /// <param name="id">The task assignment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TaskAssignment?> GetByIdWithHistoryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all task assignments for an assignee (sorted by due date).
    /// </summary>
    /// <param name="assigneeId">The assignee's user ID.</param>
    /// <param name="includeCompleted">Whether to include completed tasks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<TaskAssignment>> GetByAssigneeIdAsync(
        Guid assigneeId,
        bool includeCompleted = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets task assignments for an assignee with pagination.
    /// </summary>
    /// <param name="assigneeId">The assignee's user ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="includeCompleted">Whether to include completed tasks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of tasks list and total count.</returns>
    Task<(IReadOnlyList<TaskAssignment> Tasks, int TotalCount)> GetByAssigneeIdPagedAsync(
        Guid assigneeId,
        int page,
        int pageSize,
        bool includeCompleted = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets task assignments for an assignee with pagination, optional status filter, and sorting.
    /// Story: ACF-014
    /// </summary>
    /// <param name="assigneeId">The assignee's user ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="sortBy">Sort field: dueDate, createdAt, or status.</param>
    /// <param name="sortDirection">Sort direction: asc or desc.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of tasks list and total count.</returns>
    Task<(IReadOnlyList<TaskAssignment> Tasks, int TotalCount)> GetByAssigneeIdFilteredPagedAsync(
        Guid assigneeId,
        int page,
        int pageSize,
        AssignmentStatus? status = null,
        string sortBy = "dueDate",
        string sortDirection = "asc",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task assignment by ID with both comments and history included.
    /// Story: ACF-014
    /// </summary>
    /// <param name="id">The task assignment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TaskAssignment?> GetByIdWithCommentsAndHistoryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all task assignments for a content item.
    /// </summary>
    /// <param name="contentItemId">The content item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<TaskAssignment>> GetByContentItemIdAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all task assignments for a team.
    /// </summary>
    /// <param name="teamId">The team ID.</param>
    /// <param name="includeCompleted">Whether to include completed tasks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<TaskAssignment>> GetByTeamIdAsync(
        Guid teamId,
        bool includeCompleted = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a content item already has an active assignment.
    /// </summary>
    /// <param name="contentItemId">The content item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsActiveForContentItemAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overdue tasks for a team or assignee.
    /// </summary>
    /// <param name="teamId">Optional team ID filter.</param>
    /// <param name="assigneeId">Optional assignee ID filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<TaskAssignment>> GetOverdueTasksAsync(
        Guid? teamId = null,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new task assignment to the repository.
    /// </summary>
    Task AddAsync(TaskAssignment task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task assignment.
    /// </summary>
    Task UpdateAsync(TaskAssignment task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task assignment.
    /// </summary>
    Task DeleteAsync(TaskAssignment task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
