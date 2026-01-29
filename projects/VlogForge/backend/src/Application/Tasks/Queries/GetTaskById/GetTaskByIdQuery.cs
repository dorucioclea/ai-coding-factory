using MediatR;
using VlogForge.Application.Tasks.DTOs;

namespace VlogForge.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Query to get a task by its ID.
/// Stories: ACF-008, ACF-014
/// </summary>
public sealed record GetTaskByIdQuery(
    Guid TaskId,
    Guid RequestingUserId,
    bool IncludeComments = true,
    bool IncludeHistory = false
) : IRequest<TaskAssignmentResponse>;
