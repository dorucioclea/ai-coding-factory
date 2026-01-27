using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for PlatformConnection aggregate operations.
/// Story: ACF-003
/// </summary>
public interface IPlatformConnectionRepository
{
    /// <summary>
    /// Gets a platform connection by ID.
    /// </summary>
    Task<PlatformConnection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a platform connection for a user and platform type.
    /// </summary>
    Task<PlatformConnection?> GetByUserAndPlatformAsync(
        Guid userId,
        PlatformType platformType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all platform connections for a user.
    /// </summary>
    Task<IReadOnlyList<PlatformConnection>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all connections with expired or expiring tokens.
    /// </summary>
    Task<IReadOnlyList<PlatformConnection>> GetConnectionsNeedingRefreshAsync(
        TimeSpan expirationThreshold,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new platform connection.
    /// </summary>
    Task AddAsync(PlatformConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing platform connection.
    /// </summary>
    Task UpdateAsync(PlatformConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a platform connection.
    /// </summary>
    Task DeleteAsync(PlatformConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
