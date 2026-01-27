using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Team aggregate operations.
/// Story: ACF-007
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Gets a team by its unique identifier.
    /// </summary>
    /// <param name="id">The team ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a team by ID with members included.
    /// </summary>
    /// <param name="id">The team ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Team?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a team by ID with invitations included.
    /// </summary>
    /// <param name="id">The team ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Team?> GetByIdWithInvitationsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all teams owned by a user.
    /// </summary>
    /// <param name="ownerId">The owner's user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<Team>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all teams where a user is a member.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<Team>> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets teams where a user is a member with pagination.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of teams list and total count.</returns>
    Task<(IReadOnlyList<Team> Teams, int TotalCount)> GetByMemberUserIdPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a team by invitation token.
    /// </summary>
    /// <param name="token">The invitation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Team?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a team with the given name exists for an owner.
    /// </summary>
    /// <param name="ownerId">The owner's user ID.</param>
    /// <param name="name">The team name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsWithNameAsync(Guid ownerId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new team to the repository.
    /// </summary>
    Task AddAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing team.
    /// </summary>
    Task UpdateAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a team.
    /// </summary>
    Task DeleteAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
