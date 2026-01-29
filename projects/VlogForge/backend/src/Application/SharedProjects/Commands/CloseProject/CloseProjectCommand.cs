using MediatR;

namespace VlogForge.Application.SharedProjects.Commands.CloseProject;

/// <summary>
/// Command to close a shared project (owner only).
/// Story: ACF-013
/// </summary>
public sealed record CloseProjectCommand(Guid ProjectId, Guid UserId) : IRequest;
