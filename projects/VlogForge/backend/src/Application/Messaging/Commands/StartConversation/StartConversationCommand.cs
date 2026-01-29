using MediatR;
using VlogForge.Application.Messaging.DTOs;

namespace VlogForge.Application.Messaging.Commands.StartConversation;

/// <summary>
/// Command to start a conversation with another creator.
/// Story: ACF-012
/// </summary>
public sealed record StartConversationCommand(
    Guid UserId,
    Guid ParticipantId
) : IRequest<ConversationResponse>;
