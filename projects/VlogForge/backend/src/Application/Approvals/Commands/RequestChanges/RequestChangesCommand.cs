using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.Approvals.Commands.RequestChanges;

/// <summary>
/// Command to request changes on content.
/// Story: ACF-009
/// </summary>
public sealed record RequestChangesCommand(
    Guid ContentItemId,
    Guid TeamId,
    Guid ReviewerId,
    string Feedback
) : IRequest<ContentIdeaResponse>;
