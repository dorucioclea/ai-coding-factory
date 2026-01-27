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

    public async Task<IReadOnlyDictionary<Guid, PlatformMetrics>> GetByConnectionIdsAsync(
        IEnumerable<Guid> connectionIds,
        CancellationToken cancellationToken = default)
    {
        var connectionIdList = connectionIds.ToList();
        if (connectionIdList.Count == 0)
            return new Dictionary<Guid, PlatformMetrics>();

        var metrics = await _context.PlatformMetrics
            .Where(pm => connectionIdList.Contains(pm.PlatformConnectionId))
            .ToListAsync(cancellationToken);

        return metrics.ToDictionary(m => m.PlatformConnectionId);
    }

    public async Task<IReadOnlyList<PlatformMetrics>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Single query with join to avoid N+1 problem
        return await _context.PlatformMetrics
            .Join(
                _context.PlatformConnections.Where(pc =>
                    pc.UserId == userId && pc.Status == ConnectionStatus.Connected),
                pm => pm.PlatformConnectionId,
                pc => pc.Id,
                (pm, pc) => pm)
            .OrderBy(pm => pm.PlatformType)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlatformMetrics?> GetByUserAndPlatformAsync(
        Guid userId,
        PlatformType platformType,
        CancellationToken cancellationToken = default)
    {
        // Single query with join to avoid N+1 problem
        return await _context.PlatformMetrics
            .Join(
                _context.PlatformConnections.Where(pc =>
                    pc.UserId == userId && pc.PlatformType == platformType),
                pm => pm.PlatformConnectionId,
                pc => pc.Id,
                (pm, pc) => pm)
            .FirstOrDefaultAsync(cancellationToken);
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
