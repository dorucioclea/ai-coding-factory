using VlogForge.Domain.Common;
using VlogForge.Domain.Entities;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when a platform is connected via OAuth.
/// Story: ACF-003
/// </summary>
public sealed class PlatformConnectedEvent : DomainEvent
{
    public Guid ConnectionId { get; }
    public Guid UserId { get; }
    public PlatformType PlatformType { get; }
    public string PlatformAccountId { get; }
    public string PlatformAccountName { get; }

    public PlatformConnectedEvent(
        Guid connectionId,
        Guid userId,
        PlatformType platformType,
        string platformAccountId,
        string platformAccountName)
    {
        ConnectionId = connectionId;
        UserId = userId;
        PlatformType = platformType;
        PlatformAccountId = platformAccountId;
        PlatformAccountName = platformAccountName;
    }
}

/// <summary>
/// Event raised when a platform is disconnected.
/// Story: ACF-003
/// </summary>
public sealed class PlatformDisconnectedEvent : DomainEvent
{
    public Guid ConnectionId { get; }
    public Guid UserId { get; }
    public PlatformType PlatformType { get; }

    public PlatformDisconnectedEvent(Guid connectionId, Guid userId, PlatformType platformType)
    {
        ConnectionId = connectionId;
        UserId = userId;
        PlatformType = platformType;
    }
}
