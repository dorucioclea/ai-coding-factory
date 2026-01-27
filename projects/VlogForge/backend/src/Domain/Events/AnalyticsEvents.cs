using VlogForge.Domain.Common;
using VlogForge.Domain.Entities;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when platform metrics are synced from the platform API.
/// Story: ACF-004
/// </summary>
public sealed class MetricsSyncedEvent : DomainEvent
{
    public Guid MetricsId { get; }
    public Guid PlatformConnectionId { get; }
    public PlatformType PlatformType { get; }
    public DateTime SyncedAt { get; }

    public MetricsSyncedEvent(
        Guid metricsId,
        Guid platformConnectionId,
        PlatformType platformType,
        DateTime syncedAt)
    {
        MetricsId = metricsId;
        PlatformConnectionId = platformConnectionId;
        PlatformType = platformType;
        SyncedAt = syncedAt;
    }
}

/// <summary>
/// Event raised when a daily metrics snapshot is created.
/// Story: ACF-004
/// </summary>
public sealed class DailySnapshotCreatedEvent : DomainEvent
{
    public Guid SnapshotId { get; }
    public Guid UserId { get; }
    public PlatformType PlatformType { get; }
    public DateTime SnapshotDate { get; }

    public DailySnapshotCreatedEvent(
        Guid snapshotId,
        Guid userId,
        PlatformType platformType,
        DateTime snapshotDate)
    {
        SnapshotId = snapshotId;
        UserId = userId;
        PlatformType = platformType;
        SnapshotDate = snapshotDate;
    }
}
