using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for MetricsSnapshot aggregate operations.
/// Story: ACF-004
/// </summary>
public interface IMetricsSnapshotRepository
{
    /// <summary>
    /// Gets metrics snapshots for a user within a date range.
    /// </summary>
    Task<IReadOnlyList<MetricsSnapshot>> GetSnapshotsAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        PlatformType? platformType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent snapshot for a user and platform.
    /// </summary>
    Task<MetricsSnapshot?> GetLatestSnapshotAsync(
        Guid userId,
        PlatformType platformType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a snapshot for a specific date (if exists).
    /// </summary>
    Task<MetricsSnapshot?> GetByDateAsync(
        Guid userId,
        PlatformType platformType,
        DateTime snapshotDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new metrics snapshot.
    /// </summary>
    Task AddAsync(MetricsSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple snapshots in batch.
    /// </summary>
    Task AddRangeAsync(IEnumerable<MetricsSnapshot> snapshots, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
