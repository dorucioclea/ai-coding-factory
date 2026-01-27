using MediatR;
using VlogForge.Application.Tasks.DTOs;

namespace VlogForge.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Query to get a task by its ID.
/// Story: ACF-008
/// </summary>
public sealed record GetTaskByIdQuery(
    Guid TaskId,
    Guid RequestingUserId,
    bool IncludeComments = true
) : IRequest<TaskAssignmentResponse>;
