using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for CollaborationRequest aggregate operations.
/// Story: ACF-011
/// </summary>
public interface ICollaborationRequestRepository
{
    /// <summary>
    /// Gets a collaboration request by ID.
    /// </summary>
    Task<CollaborationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets received requests for a user (inbox) with pagination.
    /// </summary>
    Task<(IReadOnlyList<CollaborationRequest> Requests, int TotalCount)> GetReceivedRequestsAsync(
        Guid recipientId,
        CollaborationRequestStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sent requests for a user with pagination.
    /// </summary>
    Task<(IReadOnlyList<CollaborationRequest> Requests, int TotalCount)> GetSentRequestsAsync(
        Guid senderId,
        CollaborationRequestStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a pending request already exists between two users.
    /// </summary>
    Task<bool> ExistsPendingBetweenAsync(
        Guid senderId,
        Guid recipientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts how many requests a user has sent today (for rate limiting).
    /// </summary>
    Task<int> CountSentTodayAsync(Guid senderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all expired pending requests that need to be marked as expired.
    /// </summary>
    Task<IReadOnlyList<CollaborationRequest>> GetExpiredPendingRequestsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new collaboration request.
    /// </summary>
    Task AddAsync(CollaborationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing collaboration request.
    /// </summary>
    Task UpdateAsync(CollaborationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
