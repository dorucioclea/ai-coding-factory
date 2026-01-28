using VlogForge.Domain.Common;

namespace VlogForge.Domain.Events;

/// <summary>
/// Raised when a collaboration request is created.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestCreatedEvent(
    Guid requestId, Guid senderId, Guid recipientId) : DomainEvent
{
    public Guid RequestId { get; } = requestId;
    public Guid SenderId { get; } = senderId;
    public Guid RecipientId { get; } = recipientId;
}

/// <summary>
/// Raised when a collaboration request is accepted.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestAcceptedEvent(
    Guid requestId, Guid senderId, Guid recipientId) : DomainEvent
{
    public Guid RequestId { get; } = requestId;
    public Guid SenderId { get; } = senderId;
    public Guid RecipientId { get; } = recipientId;
}

/// <summary>
/// Raised when a collaboration request is declined.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestDeclinedEvent(
    Guid requestId, Guid senderId, Guid recipientId) : DomainEvent
{
    public Guid RequestId { get; } = requestId;
    public Guid SenderId { get; } = senderId;
    public Guid RecipientId { get; } = recipientId;
}

/// <summary>
/// Raised when a collaboration request is withdrawn by the sender.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestWithdrawnEvent(
    Guid requestId, Guid senderId, Guid recipientId) : DomainEvent
{
    public Guid RequestId { get; } = requestId;
    public Guid SenderId { get; } = senderId;
    public Guid RecipientId { get; } = recipientId;
}

/// <summary>
/// Raised when a collaboration request expires.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestExpiredEvent(
    Guid requestId, Guid senderId, Guid recipientId) : DomainEvent
{
    public Guid RequestId { get; } = requestId;
    public Guid SenderId { get; } = senderId;
    public Guid RecipientId { get; } = recipientId;
}
