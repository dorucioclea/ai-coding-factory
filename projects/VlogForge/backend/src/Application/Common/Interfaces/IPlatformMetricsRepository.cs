using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for PlatformMetrics aggregate operations.
/// Story: ACF-004
/// </summary>
public interface IPlatformMetricsRepository
{
    /// <summary>
    /// Gets platform metrics by ID.
    /// </summary>
    Task<PlatformMetrics?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets platform metrics by connection ID.
    /// </summary>
    Task<PlatformMetrics?> GetByConnectionIdAsync(Guid connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all platform metrics for multiple connection IDs (batch operation).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, PlatformMetrics>> GetByConnectionIdsAsync(
        IEnumerable<Guid> connectionIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all platform metrics for a user (via their platform connections).
    /// </summary>
    Task<IReadOnlyList<PlatformMetrics>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets platform metrics for a specific platform type for a user.
    /// </summary>
    Task<PlatformMetrics?> GetByUserAndPlatformAsync(
        Guid userId,
        PlatformType platformType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds new platform metrics.
    /// </summary>
    Task AddAsync(PlatformMetrics metrics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing platform metrics.
    /// </summary>
    Task UpdateAsync(PlatformMetrics metrics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
