using VlogForge.Domain.Common;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when a creator profile is created.
/// Story: ACF-002
/// </summary>
public sealed class CreatorProfileCreatedEvent : DomainEvent
{
    public Guid ProfileId { get; }
    public Guid UserId { get; }
    public string Username { get; }

    public CreatorProfileCreatedEvent(Guid profileId, Guid userId, string username)
    {
        ProfileId = profileId;
        UserId = userId;
        Username = username;
    }
}

/// <summary>
/// Event raised when a creator profile is updated.
/// Story: ACF-002
/// </summary>
public sealed class CreatorProfileUpdatedEvent : DomainEvent
{
    public Guid ProfileId { get; }
    public Guid UserId { get; }

    public CreatorProfileUpdatedEvent(Guid profileId, Guid userId)
    {
        ProfileId = profileId;
        UserId = userId;
    }
}

/// <summary>
/// Event raised when collaboration availability changes.
/// Story: ACF-002
/// </summary>
public sealed class CollaborationAvailabilityChangedEvent : DomainEvent
{
    public Guid ProfileId { get; }
    public Guid UserId { get; }
    public bool IsOpenToCollaborations { get; }

    public CollaborationAvailabilityChangedEvent(Guid profileId, Guid userId, bool isOpenToCollaborations)
    {
        ProfileId = profileId;
        UserId = userId;
        IsOpenToCollaborations = isOpenToCollaborations;
    }
}
