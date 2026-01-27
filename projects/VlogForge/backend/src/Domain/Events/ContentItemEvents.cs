using VlogForge.Domain.Common;
using VlogForge.Domain.Entities;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when a content item is created.
/// Story: ACF-005
/// </summary>
public sealed class ContentItemCreatedEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid UserId { get; }
    public string Title { get; }

    public ContentItemCreatedEvent(Guid contentItemId, Guid userId, string title)
    {
        ContentItemId = contentItemId;
        UserId = userId;
        Title = title;
    }
}

/// <summary>
/// Event raised when a content item is updated.
/// Story: ACF-005
/// </summary>
public sealed class ContentItemUpdatedEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid UserId { get; }

    public ContentItemUpdatedEvent(Guid contentItemId, Guid userId)
    {
        ContentItemId = contentItemId;
        UserId = userId;
    }
}

/// <summary>
/// Event raised when a content item's status changes.
/// Story: ACF-005
/// </summary>
public sealed class ContentItemStatusChangedEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid UserId { get; }
    public IdeaStatus OldStatus { get; }
    public IdeaStatus NewStatus { get; }

    public ContentItemStatusChangedEvent(Guid contentItemId, Guid userId, IdeaStatus oldStatus, IdeaStatus newStatus)
    {
        ContentItemId = contentItemId;
        UserId = userId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

/// <summary>
/// Event raised when a content item is soft-deleted.
/// Story: ACF-005
/// </summary>
public sealed class ContentItemDeletedEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid UserId { get; }

    public ContentItemDeletedEvent(Guid contentItemId, Guid userId)
    {
        ContentItemId = contentItemId;
        UserId = userId;
    }
}

/// <summary>
/// Event raised when a content item is restored from soft-delete.
/// Story: ACF-005
/// </summary>
public sealed class ContentItemRestoredEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid UserId { get; }

    public ContentItemRestoredEvent(Guid contentItemId, Guid userId)
    {
        ContentItemId = contentItemId;
        UserId = userId;
    }
}

/// <summary>
/// Event raised when a content item's scheduled date changes.
/// Story: ACF-006
/// </summary>
public sealed class ContentItemScheduledDateChangedEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid UserId { get; }
    public DateTime? OldScheduledDate { get; }
    public DateTime? NewScheduledDate { get; }

    public ContentItemScheduledDateChangedEvent(
        Guid contentItemId,
        Guid userId,
        DateTime? oldScheduledDate,
        DateTime? newScheduledDate)
    {
        ContentItemId = contentItemId;
        UserId = userId;
        OldScheduledDate = oldScheduledDate;
        NewScheduledDate = newScheduledDate;
    }
}
