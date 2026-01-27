using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for ContentItem aggregate operations.
/// Story: ACF-005
/// </summary>
public interface IContentItemRepository
{
    /// <summary>
    /// Gets a content item by its unique identifier.
    /// </summary>
    /// <param name="id">The content item ID.</param>
    /// <param name="includeDeleted">Include soft-deleted items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ContentItem?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all content items for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="includeDeleted">Include soft-deleted items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<ContentItem>> GetByUserIdAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets content items for a user with filtering options.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="status">Filter by status (optional).</param>
    /// <param name="platformTag">Filter by platform tag (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<ContentItem>> GetByUserIdWithFiltersAsync(
        Guid userId,
        IdeaStatus? status = null,
        string? platformTag = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches content items by title or notes.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<ContentItem>> SearchAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets soft-deleted items older than the specified days.
    /// </summary>
    /// <param name="olderThanDays">Days since deletion.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<ContentItem>> GetDeletedOlderThanAsync(int olderThanDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets content items scheduled within a date range.
    /// Story: ACF-006
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="startDate">Start of the date range (inclusive).</param>
    /// <param name="endDate">End of the date range (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<ContentItem>> GetScheduledForDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new content item to the repository.
    /// </summary>
    Task AddAsync(ContentItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing content item.
    /// </summary>
    Task UpdateAsync(ContentItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes a content item.
    /// </summary>
    Task DeleteAsync(ContentItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets content items pending approval for a team.
    /// Story: ACF-009
    /// </summary>
    /// <param name="teamId">The team ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<ContentItem>> GetPendingApprovalForTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
}
