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

        // Escape LIKE pattern special characters to prevent LIKE injection
        var escapedTerm = EscapeLikePattern(searchTerm.Trim().ToLowerInvariant());

        return await _context.ContentItems
            .Where(c => c.UserId == userId)
            .Where(c => EF.Functions.ILike(c.Title, $"%{escapedTerm}%") ||
                        (c.Notes != null && EF.Functions.ILike(c.Notes, $"%{escapedTerm}%")))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Escapes LIKE pattern special characters to prevent injection.
    /// </summary>
    private static string EscapeLikePattern(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_")
            .Replace("[", "\\[");
    }

    public async Task<IReadOnlyList<ContentItem>> GetDeletedOlderThanAsync(int olderThanDays, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

        return await _context.ContentItems
            .IgnoreQueryFilters()
            .Where(c => c.IsDeleted && c.DeletedAt.HasValue && c.DeletedAt.Value < cutoffDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets content items scheduled within a date range.
    /// Story: ACF-006
    /// </summary>
    public async Task<IReadOnlyList<ContentItem>> GetScheduledForDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.ContentItems
            .Where(c => c.UserId == userId)
            .Where(c => c.ScheduledDate.HasValue &&
                        c.ScheduledDate.Value >= startDate &&
                        c.ScheduledDate.Value <= endDate)
            .OrderBy(c => c.ScheduledDate)
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

    /// <summary>
    /// Gets content items pending approval for a team.
    /// Story: ACF-009
    /// </summary>
    public async Task<IReadOnlyList<ContentItem>> GetPendingApprovalForTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        // Get content items in InReview status that have approval records for this team
        var contentItemIds = await _context.ApprovalRecords
            .Where(r => r.TeamId == teamId)
            .Select(r => r.ContentItemId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.ContentItems
            .Where(c => contentItemIds.Contains(c.Id) && c.Status == IdeaStatus.InReview)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
