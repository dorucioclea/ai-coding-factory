using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CollaborationRequest.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequestRepository : ICollaborationRequestRepository
{
    private readonly ApplicationDbContext _context;

    public CollaborationRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CollaborationRequest?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CollaborationRequests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<CollaborationRequest> Requests, int TotalCount)> GetReceivedRequestsAsync(
        Guid recipientId,
        CollaborationRequestStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CollaborationRequests
            .AsNoTracking()
            .Where(r => r.RecipientId == recipientId);

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (requests, totalCount);
    }

    public async Task<(IReadOnlyList<CollaborationRequest> Requests, int TotalCount)> GetSentRequestsAsync(
        Guid senderId,
        CollaborationRequestStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CollaborationRequests
            .AsNoTracking()
            .Where(r => r.SenderId == senderId);

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (requests, totalCount);
    }

    public async Task<bool> ExistsPendingBetweenAsync(
        Guid senderId,
        Guid recipientId,
        CancellationToken cancellationToken = default)
    {
        // Bidirectional check: prevent reciprocal pending requests
        return await _context.CollaborationRequests
            .AnyAsync(r =>
                ((r.SenderId == senderId && r.RecipientId == recipientId) ||
                 (r.SenderId == recipientId && r.RecipientId == senderId)) &&
                r.Status == CollaborationRequestStatus.Pending &&
                r.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    public async Task<int> CountSentTodayAsync(
        Guid senderId,
        CancellationToken cancellationToken = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        return await _context.CollaborationRequests
            .CountAsync(r =>
                r.SenderId == senderId &&
                r.CreatedAt >= todayUtc,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CollaborationRequest>> GetExpiredPendingRequestsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CollaborationRequests
            .Where(r =>
                r.Status == CollaborationRequestStatus.Pending &&
                r.ExpiresAt <= DateTime.UtcNow)
            .OrderBy(r => r.ExpiresAt)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CollaborationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _context.CollaborationRequests.AddAsync(request, cancellationToken);
    }

    public Task UpdateAsync(
        CollaborationRequest request,
        CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(request);
        if (entry.State == EntityState.Detached)
            _context.CollaborationRequests.Update(request);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
