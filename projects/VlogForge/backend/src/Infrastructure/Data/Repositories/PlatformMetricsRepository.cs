using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for PlatformMetrics aggregate.
/// Story: ACF-004
/// </summary>
public class PlatformMetricsRepository : IPlatformMetricsRepository
{
    private readonly ApplicationDbContext _context;

    public PlatformMetricsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformMetrics?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlatformMetrics
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);
    }

    public async Task<PlatformMetrics?> GetByConnectionIdAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlatformMetrics
            .FirstOrDefaultAsync(pm => pm.PlatformConnectionId == connectionId, cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformMetrics>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Join with PlatformConnections to find metrics for a user
        var connectionIds = await _context.PlatformConnections
            .Where(pc => pc.UserId == userId && pc.Status == ConnectionStatus.Connected)
            .Select(pc => pc.Id)
            .ToListAsync(cancellationToken);

        return await _context.PlatformMetrics
            .Where(pm => connectionIds.Contains(pm.PlatformConnectionId))
            .OrderBy(pm => pm.PlatformType)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlatformMetrics?> GetByUserAndPlatformAsync(
        Guid userId,
        PlatformType platformType,
        CancellationToken cancellationToken = default)
    {
        // Find the connection first, then get metrics
        var connection = await _context.PlatformConnections
            .FirstOrDefaultAsync(
                pc => pc.UserId == userId && pc.PlatformType == platformType,
                cancellationToken);

        if (connection == null)
            return null;

        return await GetByConnectionIdAsync(connection.Id, cancellationToken);
    }

    public async Task AddAsync(
        PlatformMetrics metrics,
        CancellationToken cancellationToken = default)
    {
        await _context.PlatformMetrics.AddAsync(metrics, cancellationToken);
    }

    public Task UpdateAsync(
        PlatformMetrics metrics,
        CancellationToken cancellationToken = default)
    {
        _context.PlatformMetrics.Update(metrics);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
