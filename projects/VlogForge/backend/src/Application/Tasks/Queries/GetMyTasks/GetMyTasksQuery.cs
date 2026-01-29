using MediatR;
using VlogForge.Application.Tasks.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Tasks.Queries.GetMyTasks;

/// <summary>
/// Query to get tasks assigned to the current user.
/// Stories: ACF-008, ACF-014
/// </summary>
public sealed record GetMyTasksQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20,
    AssignmentStatus? Status = null,
    string SortBy = "dueDate",
    string SortDirection = "asc"
) : IRequest<TaskListResponse>;
