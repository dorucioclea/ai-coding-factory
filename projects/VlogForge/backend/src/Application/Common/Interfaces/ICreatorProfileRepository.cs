using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for CreatorProfile aggregate operations.
/// Story: ACF-002
/// </summary>
public interface ICreatorProfileRepository
{
    /// <summary>
    /// Gets a profile by its unique identifier.
    /// </summary>
    Task<CreatorProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a profile by the user's unique identifier.
    /// </summary>
    Task<CreatorProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a profile by username.
    /// </summary>
    Task<CreatorProfile?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a username is already taken.
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user already has a profile.
    /// </summary>
    Task<bool> UserHasProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new profile to the repository.
    /// </summary>
    Task AddAsync(CreatorProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing profile.
    /// </summary>
    Task UpdateAsync(CreatorProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
