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

    /// <summary>
    /// Discovers creators with filtering and cursor-based pagination.
    /// Story: ACF-010
    /// </summary>
    /// <param name="excludeUserId">Optional user ID to exclude from results (typically the requesting user).</param>
    /// <param name="niches">Filter by niche tags.</param>
    /// <param name="platforms">Filter by connected platform types.</param>
    /// <param name="minFollowers">Minimum total follower count.</param>
    /// <param name="maxFollowers">Maximum total follower count.</param>
    /// <param name="searchTerm">Search term for name/username/bio.</param>
    /// <param name="openToCollaboration">Filter by collaboration availability.</param>
    /// <param name="cursor">Cursor for pagination (profile ID).</param>
    /// <param name="pageSize">Number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of profiles and whether more results exist.</returns>
    Task<(IReadOnlyList<CreatorProfile> Profiles, bool HasMore, int TotalCount)> DiscoverCreatorsAsync(
        Guid? excludeUserId = null,
        IReadOnlyList<string>? niches = null,
        IReadOnlyList<PlatformType>? platforms = null,
        int? minFollowers = null,
        int? maxFollowers = null,
        string? searchTerm = null,
        bool? openToCollaboration = null,
        Guid? cursor = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
