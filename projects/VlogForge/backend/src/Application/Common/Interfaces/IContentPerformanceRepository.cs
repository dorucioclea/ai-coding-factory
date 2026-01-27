using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for ContentPerformance aggregate operations.
/// Story: ACF-004
/// </summary>
public interface IContentPerformanceRepository
{
    /// <summary>
    /// Gets content performance by ID.
    /// </summary>
    Task<ContentPerformance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets content performance by platform-specific content ID.
    /// </summary>
    Task<ContentPerformance?> GetByContentIdAsync(
        Guid connectionId,
        string contentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top performing content for a user across all platforms.
    /// </summary>
    Task<IReadOnlyList<ContentPerformance>> GetTopContentAsync(
        Guid userId,
        int limit = 10,
        string sortBy = "views",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top performing content for a specific platform.
    /// </summary>
    Task<IReadOnlyList<ContentPerformance>> GetTopContentByPlatformAsync(
        Guid userId,
        PlatformType platformType,
        int limit = 10,
        string sortBy = "views",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all content for a platform connection.
    /// </summary>
    Task<IReadOnlyList<ContentPerformance>> GetByConnectionIdAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds new content performance record.
    /// </summary>
    Task AddAsync(ContentPerformance content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing content performance record.
    /// </summary>
    Task UpdateAsync(ContentPerformance content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts content (add or update based on content ID).
    /// </summary>
    Task UpsertAsync(ContentPerformance content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
