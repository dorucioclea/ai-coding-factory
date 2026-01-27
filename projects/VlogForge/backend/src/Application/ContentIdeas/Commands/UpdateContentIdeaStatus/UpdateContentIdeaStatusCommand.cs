using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.ContentIdeas.Commands.UpdateContentIdeaStatus;

/// <summary>
/// Command to update a content idea's status.
/// Story: ACF-005
/// </summary>
public sealed record UpdateContentIdeaStatusCommand(
    Guid Id,
    Guid UserId,
    IdeaStatus NewStatus
) : IRequest<ContentIdeaResponse>;
