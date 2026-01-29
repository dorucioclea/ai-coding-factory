using MediatR;

namespace VlogForge.Application.SharedProjects.Commands.LeaveProject;

/// <summary>
/// Command to leave a shared project.
/// Story: ACF-013
/// </summary>
public sealed record LeaveProjectCommand(Guid ProjectId, Guid UserId) : IRequest;
