using MediatR;
using VlogForge.Application.Tasks.DTOs;

namespace VlogForge.Application.Tasks.Queries.GetMyTasks;

/// <summary>
/// Query to get tasks assigned to the current user.
/// Story: ACF-008
/// </summary>
public sealed record GetMyTasksQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20,
    bool IncludeCompleted = true
) : IRequest<TaskListResponse>;
