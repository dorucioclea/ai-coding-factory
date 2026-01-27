using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Commands.UpdateContentIdea;

/// <summary>
/// Command to update a content idea.
/// Story: ACF-005
/// </summary>
public sealed record UpdateContentIdeaCommand(
    Guid Id,
    Guid UserId,
    string Title,
    string? Notes = null,
    IReadOnlyList<string>? PlatformTags = null,
    IdeaStatus? Status = null
) : IRequest<ContentIdeaResponse>;
