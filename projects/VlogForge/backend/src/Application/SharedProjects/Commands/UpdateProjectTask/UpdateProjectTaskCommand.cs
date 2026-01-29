using MediatR;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.SharedProjects.Commands.UpdateProjectTask;

/// <summary>
/// Command to update a task in a shared project.
/// Story: ACF-013
/// </summary>
public sealed record UpdateProjectTaskCommand(
    Guid ProjectId,
    Guid TaskId,
    Guid UserId,
    string? Title = null,
    string? Description = null,
    SharedProjectTaskStatus? Status = null,
    Guid? AssigneeId = null,
    DateTime? DueDate = null) : IRequest<SharedProjectDetailResponse>;
