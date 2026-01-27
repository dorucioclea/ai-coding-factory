using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ContentPerformance aggregate.
/// Story: ACF-004
/// </summary>
public class ContentPerformanceRepository : IContentPerformanceRepository
{
    private readonly ApplicationDbContext _context;

    public ContentPerformanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ContentPerformance?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ContentPerformances
            .FirstOrDefaultAsync(cp => cp.Id == id, cancellationToken);
    }

    public async Task<ContentPerformance?> GetByContentIdAsync(
        Guid connectionId,
        string contentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ContentPerformances
            .FirstOrDefaultAsync(
                cp => cp.PlatformConnectionId == connectionId && cp.ContentId == contentId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ContentPerformance>> GetTopContentAsync(
        Guid userId,
        int limit = 10,
        string sortBy = "views",
        CancellationToken cancellationToken = default)
    {
        // Get connection IDs for this user
        var connectionIds = await _context.PlatformConnections
            .Where(pc => pc.UserId == userId && pc.Status == ConnectionStatus.Connected)
            .Select(pc => pc.Id)
            .ToListAsync(cancellationToken);

        var query = _context.ContentPerformances
            .Where(cp => connectionIds.Contains(cp.PlatformConnectionId));

        query = sortBy.ToLowerInvariant() switch
        {
            "engagement" => query.OrderByDescending(cp => cp.EngagementRate),
            "likes" => query.OrderByDescending(cp => cp.LikeCount),
            "comments" => query.OrderByDescending(cp => cp.CommentCount),
            _ => query.OrderByDescending(cp => cp.ViewCount) // Default to views
        };

        return await query
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentPerformance>> GetTopContentByPlatformAsync(
        Guid userId,
        PlatformType platformType,
        int limit = 10,
        string sortBy = "views",
        CancellationToken cancellationToken = default)
    {
        // Get connection ID for this user and platform
        var connection = await _context.PlatformConnections
            .FirstOrDefaultAsync(
                pc => pc.UserId == userId && pc.PlatformType == platformType,
                cancellationToken);

        if (connection == null)
            return Array.Empty<ContentPerformance>();

        var query = _context.ContentPerformances
            .Where(cp => cp.PlatformConnectionId == connection.Id);

        query = sortBy.ToLowerInvariant() switch
        {
            "engagement" => query.OrderByDescending(cp => cp.EngagementRate),
            "likes" => query.OrderByDescending(cp => cp.LikeCount),
            "comments" => query.OrderByDescending(cp => cp.CommentCount),
            _ => query.OrderByDescending(cp => cp.ViewCount) // Default to views
        };

        return await query
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentPerformance>> GetByConnectionIdAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ContentPerformances
            .Where(cp => cp.PlatformConnectionId == connectionId)
            .OrderByDescending(cp => cp.ViewCount)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ContentPerformance content,
        CancellationToken cancellationToken = default)
    {
        await _context.ContentPerformances.AddAsync(content, cancellationToken);
    }

    public Task UpdateAsync(
        ContentPerformance content,
        CancellationToken cancellationToken = default)
    {
        _context.ContentPerformances.Update(content);
        return Task.CompletedTask;
    }

    public async Task UpsertAsync(
        ContentPerformance content,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByContentIdAsync(
            content.PlatformConnectionId,
            content.ContentId,
            cancellationToken);

        if (existing == null)
        {
            await AddAsync(content, cancellationToken);
        }
        else
        {
            existing.UpdateMetrics(
                content.ViewCount,
                content.LikeCount,
                content.CommentCount,
                content.ShareCount);
            existing.UpdateMetadata(content.Title, content.ThumbnailUrl);
            await UpdateAsync(existing, cancellationToken);
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
