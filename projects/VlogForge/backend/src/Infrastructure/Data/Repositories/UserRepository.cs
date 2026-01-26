using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for User aggregate.
/// Story: ACF-001
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.TokenHash == tokenHash), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // Don't call Update() if the entity is already tracked.
        // EF Core will detect changes automatically.
        var entry = _context.Entry(user);
        if (entry.State == EntityState.Detached)
        {
            _context.Users.Update(user);
        }

        // Fix for EF Core backing field issue: when new RefreshTokens are added to
        // the _refreshTokens backing field after the parent entity was loaded, EF Core
        // incorrectly marks them as Modified instead of Added. We detect new tokens
        // by checking if RevokedAt is null (tokens are created without being revoked).
        _context.ChangeTracker.DetectChanges();
        foreach (var refreshToken in user.RefreshTokens)
        {
            var tokenEntry = _context.Entry(refreshToken);
            if (tokenEntry.State == EntityState.Modified)
            {
                // A newly created token will have both original and current RevokedAt as null.
                // A revoked token will have original as null but current as non-null.
                var originalRevokedAt = tokenEntry.OriginalValues.GetValue<DateTime?>("RevokedAt");
                var currentRevokedAt = refreshToken.RevokedAt;

                if (originalRevokedAt == null && currentRevokedAt == null)
                {
                    tokenEntry.State = EntityState.Added;
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
