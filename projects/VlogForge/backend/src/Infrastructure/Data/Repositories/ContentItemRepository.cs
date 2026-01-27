using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ContentItem aggregate.
/// Story: ACF-005
/// </summary>
public sealed class ContentItemRepository : IContentItemRepository
{
    private readonly ApplicationDbContext _context;

    public ContentItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ContentItem?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ContentItems.AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ContentItem>> GetByUserIdAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ContentItems.AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentItem>> GetByUserIdWithFiltersAsync(
        Guid userId,
        IdeaStatus? status = null,
        string? platformTag = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ContentItems
            .Where(c => c.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(platformTag))
        {
            var normalizedTag = platformTag.Trim().ToLowerInvariant();
            query = query.Where(c => c.PlatformTags.Contains(normalizedTag));
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentItem>> SearchAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetByUserIdAsync(userId, cancellationToken: cancellationToken);
        }

        var normalizedTerm = searchTerm.Trim().ToLowerInvariant();

        return await _context.ContentItems
            .Where(c => c.UserId == userId)
            .Where(c => EF.Functions.ILike(c.Title, $"%{normalizedTerm}%") ||
                        (c.Notes != null && EF.Functions.ILike(c.Notes, $"%{normalizedTerm}%")))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentItem>> GetDeletedOlderThanAsync(int olderThanDays, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

        return await _context.ContentItems
            .IgnoreQueryFilters()
            .Where(c => c.IsDeleted && c.DeletedAt.HasValue && c.DeletedAt.Value < cutoffDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ContentItem item, CancellationToken cancellationToken = default)
    {
        await _context.ContentItems.AddAsync(item, cancellationToken);
    }

    public Task UpdateAsync(ContentItem item, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(item);
        if (entry.State == EntityState.Detached)
        {
            _context.ContentItems.Update(item);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ContentItem item, CancellationToken cancellationToken = default)
    {
        _context.ContentItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
