using MediatR;
using VlogForge.Application.Tasks.DTOs;

namespace VlogForge.Application.Tasks.Commands.AssignTask;

/// <summary>
/// Command to assign a content item to a team member.
/// Story: ACF-008
/// </summary>
public sealed record AssignTaskCommand(
    Guid ContentItemId,
    Guid TeamId,
    Guid AssigneeId,
    Guid AssignedById,
    DateTime DueDate,
    string? Notes = null
) : IRequest<TaskAssignmentResponse>;
