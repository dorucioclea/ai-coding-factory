using MediatR;
using VlogForge.Application.SharedProjects.DTOs;

namespace VlogForge.Application.SharedProjects.Commands.CreateSharedProject;

/// <summary>
/// Command to create a shared project when a collaboration request is accepted.
/// Story: ACF-013
/// </summary>
public sealed record CreateSharedProjectCommand(
    Guid CollaborationRequestId,
    Guid SenderId,
    Guid RecipientId,
    string Name,
    string? Description = null) : IRequest<SharedProjectResponse>;
