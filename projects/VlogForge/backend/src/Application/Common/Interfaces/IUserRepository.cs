using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for User aggregate operations.
/// Story: ACF-001
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their refresh token hash.
    /// </summary>
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already registered.
    /// </summary>
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
