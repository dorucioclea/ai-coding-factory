using MediatR;

namespace VlogForge.Application.ContentIdeas.Commands.DeleteContentIdea;

/// <summary>
/// Command to soft-delete a content idea.
/// Story: ACF-005
/// </summary>
public sealed record DeleteContentIdeaCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;
