using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for ApprovalRecord operations.
/// Story: ACF-009
/// </summary>
public interface IApprovalRecordRepository
{
    /// <summary>
    /// Gets all approval records for a content item.
    /// </summary>
    /// <param name="contentItemId">The content item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<ApprovalRecord>> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent approval record for a content item.
    /// </summary>
    /// <param name="contentItemId">The content item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ApprovalRecord?> GetLatestByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent approval record for multiple content items (batch fetch to avoid N+1 queries).
    /// </summary>
    /// <param name="contentItemIds">The content item IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyDictionary<Guid, ApprovalRecord>> GetLatestByContentItemIdsAsync(IEnumerable<Guid> contentItemIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new approval record to the repository.
    /// </summary>
    Task AddAsync(ApprovalRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the repository.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
