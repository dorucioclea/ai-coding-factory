using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.ContentIdeas.Commands.CreateContentIdea;

/// <summary>
/// Command to create a new content idea.
/// Story: ACF-005
/// </summary>
public sealed record CreateContentIdeaCommand(
    Guid UserId,
    string Title,
    string? Notes = null,
    IReadOnlyList<string>? PlatformTags = null
) : IRequest<ContentIdeaResponse>;
