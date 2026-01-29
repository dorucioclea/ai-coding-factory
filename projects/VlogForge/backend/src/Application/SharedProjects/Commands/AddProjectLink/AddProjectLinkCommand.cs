using MediatR;
using VlogForge.Application.SharedProjects.DTOs;

namespace VlogForge.Application.SharedProjects.Commands.AddProjectLink;

/// <summary>
/// Command to add a link to a shared project.
/// Story: ACF-013
/// </summary>
public sealed record AddProjectLinkCommand(
    Guid ProjectId,
    Guid UserId,
    string Title,
    string Url,
    string? Description = null) : IRequest<SharedProjectLinkResponse>;
