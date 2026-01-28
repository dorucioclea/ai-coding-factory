using VlogForge.Domain.Common;

namespace VlogForge.Domain.Events;

/// <summary>
/// Raised when a collaboration request is created.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestCreatedEvent : DomainEvent
{
    public Guid RequestId { get; }
    public Guid SenderId { get; }
    public Guid RecipientId { get; }

    public CollaborationRequestCreatedEvent(Guid requestId, Guid senderId, Guid recipientId)
    {
        RequestId = requestId;
        SenderId = senderId;
        RecipientId = recipientId;
    }
}

/// <summary>
/// Raised when a collaboration request is accepted.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestAcceptedEvent : DomainEvent
{
    public Guid RequestId { get; }
    public Guid SenderId { get; }
    public Guid RecipientId { get; }

    public CollaborationRequestAcceptedEvent(Guid requestId, Guid senderId, Guid recipientId)
    {
        RequestId = requestId;
        SenderId = senderId;
        RecipientId = recipientId;
    }
}

/// <summary>
/// Raised when a collaboration request is declined.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestDeclinedEvent : DomainEvent
{
    public Guid RequestId { get; }
    public Guid SenderId { get; }
    public Guid RecipientId { get; }

    public CollaborationRequestDeclinedEvent(Guid requestId, Guid senderId, Guid recipientId)
    {
        RequestId = requestId;
        SenderId = senderId;
        RecipientId = recipientId;
    }
}

/// <summary>
/// Raised when a collaboration request is withdrawn by the sender.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestWithdrawnEvent : DomainEvent
{
    public Guid RequestId { get; }
    public Guid SenderId { get; }
    public Guid RecipientId { get; }

    public CollaborationRequestWithdrawnEvent(Guid requestId, Guid senderId, Guid recipientId)
    {
        RequestId = requestId;
        SenderId = senderId;
        RecipientId = recipientId;
    }
}

/// <summary>
/// Raised when a collaboration request expires.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestExpiredEvent : DomainEvent
{
    public Guid RequestId { get; }
    public Guid SenderId { get; }
    public Guid RecipientId { get; }

    public CollaborationRequestExpiredEvent(Guid requestId, Guid senderId, Guid recipientId)
    {
        RequestId = requestId;
        SenderId = senderId;
        RecipientId = recipientId;
    }
}
