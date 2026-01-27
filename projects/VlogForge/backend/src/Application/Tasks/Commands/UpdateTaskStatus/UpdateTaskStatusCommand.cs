using MediatR;
using VlogForge.Application.Tasks.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Tasks.Commands.UpdateTaskStatus;

/// <summary>
/// Command to update the status of a task assignment.
/// Story: ACF-008
/// </summary>
public sealed record UpdateTaskStatusCommand(
    Guid TaskId,
    AssignmentStatus NewStatus,
    Guid UpdatedByUserId
) : IRequest<TaskAssignmentResponse>;
