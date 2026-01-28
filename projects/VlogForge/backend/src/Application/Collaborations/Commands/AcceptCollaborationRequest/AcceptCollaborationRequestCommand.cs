using MediatR;
using VlogForge.Application.Collaborations.DTOs;

namespace VlogForge.Application.Collaborations.Commands.AcceptCollaborationRequest;

/// <summary>
/// Command to accept a collaboration request.
/// Story: ACF-011
/// </summary>
public sealed record AcceptCollaborationRequestCommand(
    Guid RequestId,
    Guid UserId
) : IRequest<CollaborationRequestResponse>;
