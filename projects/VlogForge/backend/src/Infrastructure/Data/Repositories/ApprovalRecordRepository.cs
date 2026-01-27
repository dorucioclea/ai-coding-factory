using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ApprovalRecord entity.
/// Story: ACF-009
/// </summary>
public sealed class ApprovalRecordRepository : IApprovalRecordRepository
{
    private readonly ApplicationDbContext _context;

    public ApprovalRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ApprovalRecord>> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRecords
            .Where(r => r.ContentItemId == contentItemId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApprovalRecord?> GetLatestByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRecords
            .Where(r => r.ContentItemId == contentItemId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, ApprovalRecord>> GetLatestByContentItemIdsAsync(IEnumerable<Guid> contentItemIds, CancellationToken cancellationToken = default)
    {
        var idList = contentItemIds.ToList();
        if (idList.Count == 0)
        {
            return new Dictionary<Guid, ApprovalRecord>();
        }

        // Get all records for the content items, then group and select latest
        var records = await _context.ApprovalRecords
            .Where(r => idList.Contains(r.ContentItemId))
            .ToListAsync(cancellationToken);

        return records
            .GroupBy(r => r.ContentItemId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(r => r.CreatedAt).First());
    }

    public async Task AddAsync(ApprovalRecord record, CancellationToken cancellationToken = default)
    {
        await _context.ApprovalRecords.AddAsync(record, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
