using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for PlatformConnection aggregate.
/// Story: ACF-003
/// </summary>
public class PlatformConnectionRepository : IPlatformConnectionRepository
{
    private readonly ApplicationDbContext _context;

    public PlatformConnectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformConnection?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlatformConnections
            .FirstOrDefaultAsync(pc => pc.Id == id, cancellationToken);
    }

    public async Task<PlatformConnection?> GetByUserAndPlatformAsync(
        Guid userId,
        PlatformType platformType,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlatformConnections
            .FirstOrDefaultAsync(
                pc => pc.UserId == userId && pc.PlatformType == platformType,
                cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformConnection>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlatformConnections
            .Where(pc => pc.UserId == userId)
            .OrderBy(pc => pc.PlatformType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformConnection>> GetConnectionsNeedingRefreshAsync(
        TimeSpan expirationThreshold,
        CancellationToken cancellationToken = default)
    {
        var thresholdTime = DateTime.UtcNow.Add(expirationThreshold);

        return await _context.PlatformConnections
            .Where(pc =>
                pc.Status == ConnectionStatus.Connected &&
                pc.TokenExpiresAt != null &&
                pc.TokenExpiresAt <= thresholdTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        PlatformConnection connection,
        CancellationToken cancellationToken = default)
    {
        await _context.PlatformConnections.AddAsync(connection, cancellationToken);
    }

    public Task UpdateAsync(
        PlatformConnection connection,
        CancellationToken cancellationToken = default)
    {
        _context.PlatformConnections.Update(connection);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        PlatformConnection connection,
        CancellationToken cancellationToken = default)
    {
        _context.PlatformConnections.Remove(connection);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            // Re-throw as ConflictException to keep Application layer clean of EF dependencies
            throw new ConflictException(
                "PlatformConnection",
                "duplicate",
                "A platform connection already exists. Please try again.");
        }
    }

    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}
