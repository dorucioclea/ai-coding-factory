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

    /// <inheritdoc />
    public async Task<(IReadOnlyList<CreatorProfile> Profiles, bool HasMore, int TotalCount)> DiscoverCreatorsAsync(
        Guid? excludeUserId = null,
        IReadOnlyList<string>? niches = null,
        IReadOnlyList<PlatformType>? platforms = null,
        int? minFollowers = null,
        int? maxFollowers = null,
        string? searchTerm = null,
        bool? openToCollaboration = null,
        Guid? cursor = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CreatorProfiles
            .Include(p => p.ConnectedPlatforms)
            .AsQueryable();

        // Exclude requesting user's profile
        if (excludeUserId.HasValue)
        {
            query = query.Where(p => p.UserId != excludeUserId.Value);
        }

        // Filter by niche tags
        if (niches is { Count: > 0 })
        {
            var normalizedNiches = niches.Select(n => n.ToLowerInvariant()).ToList();
            query = query.Where(p => p.NicheTags.Any(t => normalizedNiches.Contains(t.Value.ToLower())));
        }

        // Filter by platforms
        if (platforms is { Count: > 0 })
        {
            query = query.Where(p => p.ConnectedPlatforms.Any(cp => platforms.Contains(cp.PlatformType)));
        }

        // Filter by open to collaboration
        if (openToCollaboration.HasValue)
        {
            query = query.Where(p => p.OpenToCollaborations == openToCollaboration.Value);
        }

        // Search by name, username, or bio using full-text search pattern
        // SECURITY: Escape special LIKE characters to prevent injection
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var escapedTerm = EscapeLikePattern(searchTerm.ToLowerInvariant());
            query = query.Where(p =>
                EF.Functions.ILike(p.DisplayName, $"%{escapedTerm}%") ||
                EF.Functions.ILike(p.Username, $"%{escapedTerm}%") ||
                EF.Functions.ILike(p.Bio.Value, $"%{escapedTerm}%"));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Order by creation date descending for consistent pagination
        query = query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id);

        // Apply cursor-based pagination
        if (cursor.HasValue)
        {
            var cursorProfile = await _context.CreatorProfiles
                .Where(p => p.Id == cursor.Value)
                .Select(p => new { p.CreatedAt, p.Id })
                .FirstOrDefaultAsync(cancellationToken);

            if (cursorProfile != null)
            {
                query = query.Where(p =>
                    p.CreatedAt < cursorProfile.CreatedAt ||
                    (p.CreatedAt == cursorProfile.CreatedAt && p.Id.CompareTo(cursorProfile.Id) > 0));
            }
        }

        // Fetch one extra to check if there are more results
        var profiles = await query
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var hasMore = profiles.Count > pageSize;
        var result = profiles.Take(pageSize).ToList();

        // Post-filter by follower count (done in memory after fetching connected platforms)
        // Note: For accurate pagination with follower filter, we recalculate hasMore
        if (minFollowers.HasValue || maxFollowers.HasValue)
        {
            var filteredResult = result.Where(p =>
            {
                var total = p.ConnectedPlatforms.Sum(cp => cp.FollowerCount ?? 0);
                var meetsMin = !minFollowers.HasValue || total >= minFollowers.Value;
                var meetsMax = !maxFollowers.HasValue || total <= maxFollowers.Value;
                return meetsMin && meetsMax;
            }).ToList();

            // Recalculate hasMore: if we filtered out items, we may have more to fetch
            // hasMore is true if original query had more OR we still have enough after filter
            hasMore = hasMore || filteredResult.Count < result.Count;
            result = filteredResult;
        }

        return (result.AsReadOnly(), hasMore, totalCount);
    }

    /// <summary>
    /// Escapes special characters in LIKE/ILIKE patterns to prevent SQL injection.
    /// Story: ACF-010
    /// </summary>
    private static string EscapeLikePattern(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}
