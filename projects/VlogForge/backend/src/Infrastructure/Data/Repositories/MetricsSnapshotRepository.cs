using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for MetricsSnapshot aggregate.
/// Story: ACF-004
/// </summary>
public class MetricsSnapshotRepository : IMetricsSnapshotRepository
{
    private readonly ApplicationDbContext _context;

    public MetricsSnapshotRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MetricsSnapshot>> GetSnapshotsAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        PlatformType? platformType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MetricsSnapshots
            .Where(ms => ms.UserId == userId &&
                         ms.SnapshotDate >= startDate.Date &&
                         ms.SnapshotDate <= endDate.Date);

        if (platformType.HasValue)
        {
            query = query.Where(ms => ms.PlatformType == platformType.Value);
        }

        return await query
            .OrderBy(ms => ms.SnapshotDate)
            .ThenBy(ms => ms.PlatformType)
            .ToListAsync(cancellationToken);
    }

    public async Task<MetricsSnapshot?> GetLatestSnapshotAsync(
        Guid userId,
        PlatformType platformType,
        CancellationToken cancellationToken = default)
    {
        return await _context.MetricsSnapshots
            .Where(ms => ms.UserId == userId && ms.PlatformType == platformType)
            .OrderByDescending(ms => ms.SnapshotDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MetricsSnapshot?> GetByDateAsync(
        Guid userId,
        PlatformType platformType,
        DateTime snapshotDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.MetricsSnapshots
            .FirstOrDefaultAsync(
                ms => ms.UserId == userId &&
                      ms.PlatformType == platformType &&
                      ms.SnapshotDate == snapshotDate.Date,
                cancellationToken);
    }

    public async Task<IReadOnlySet<(Guid UserId, PlatformType Platform)>> GetExistingForDateAsync(
        DateTime snapshotDate,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.MetricsSnapshots
            .Where(ms => ms.SnapshotDate == snapshotDate.Date)
            .Select(ms => new { ms.UserId, ms.PlatformType })
            .ToListAsync(cancellationToken);

        return existing.Select(x => (x.UserId, x.PlatformType)).ToHashSet();
    }

    public async Task AddAsync(
        MetricsSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        await _context.MetricsSnapshots.AddAsync(snapshot, cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<MetricsSnapshot> snapshots,
        CancellationToken cancellationToken = default)
    {
        await _context.MetricsSnapshots.AddRangeAsync(snapshots, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
