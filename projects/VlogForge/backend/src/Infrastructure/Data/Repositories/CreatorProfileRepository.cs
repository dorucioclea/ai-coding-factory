using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CreatorProfile aggregate.
/// Story: ACF-002
/// </summary>
public sealed class CreatorProfileRepository : ICreatorProfileRepository
{
    private readonly ApplicationDbContext _context;

    public CreatorProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<CreatorProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CreatorProfiles
            .Include(p => p.ConnectedPlatforms)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CreatorProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CreatorProfiles
            .Include(p => p.ConnectedPlatforms)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CreatorProfile?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();
        return await _context.CreatorProfiles
            .Include(p => p.ConnectedPlatforms)
            .FirstOrDefaultAsync(p => p.Username == normalizedUsername, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();
        return await _context.CreatorProfiles
            .AnyAsync(p => p.Username == normalizedUsername, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UserHasProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CreatorProfiles
            .AnyAsync(p => p.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CreatorProfile profile, CancellationToken cancellationToken = default)
    {
        await _context.CreatorProfiles.AddAsync(profile, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(CreatorProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var entry = _context.Entry(profile);
        if (entry.State == EntityState.Detached)
        {
            _context.CreatorProfiles.Update(profile);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
