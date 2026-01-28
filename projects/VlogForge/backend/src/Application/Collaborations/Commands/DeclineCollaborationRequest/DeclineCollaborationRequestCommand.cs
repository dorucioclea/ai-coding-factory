using MediatR;
using VlogForge.Application.Collaborations.DTOs;

namespace VlogForge.Application.Collaborations.Commands.DeclineCollaborationRequest;

/// <summary>
/// Command to decline a collaboration request.
/// Story: ACF-011
/// </summary>
public sealed record DeclineCollaborationRequestCommand(
    Guid RequestId,
    Guid UserId,
    string? Reason = null
) : IRequest<CollaborationRequestResponse>;
