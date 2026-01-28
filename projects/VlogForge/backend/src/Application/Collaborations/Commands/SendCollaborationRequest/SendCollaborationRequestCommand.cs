using MediatR;
using VlogForge.Application.Collaborations.DTOs;

namespace VlogForge.Application.Collaborations.Commands.SendCollaborationRequest;

/// <summary>
/// Command to send a collaboration request to another creator.
/// Story: ACF-011
/// </summary>
public sealed record SendCollaborationRequestCommand(
    Guid SenderId,
    Guid RecipientId,
    string Message
) : IRequest<CollaborationRequestResponse>;
