using MediatR;
using VlogForge.Application.SharedProjects.DTOs;

namespace VlogForge.Application.SharedProjects.Commands.AddProjectTask;

/// <summary>
/// Command to add a task to a shared project.
/// Story: ACF-013
/// </summary>
public sealed record AddProjectTaskCommand(
    Guid ProjectId,
    Guid UserId,
    string Title,
    string? Description = null,
    Guid? AssigneeId = null,
    DateTime? DueDate = null) : IRequest<SharedProjectTaskResponse>;
