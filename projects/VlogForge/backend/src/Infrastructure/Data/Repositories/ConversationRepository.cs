using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Conversation aggregate.
/// Story: ACF-012
/// </summary>
public sealed class ConversationRepository : IConversationRepository
{
    private readonly ApplicationDbContext _context;

    public ConversationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Conversation?> GetByParticipantsAsync(
        Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .FirstOrDefaultAsync(c =>
                (c.Participant1Id == userId1 && c.Participant2Id == userId2) ||
                (c.Participant1Id == userId2 && c.Participant2Id == userId1),
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Conversation> Conversations, int TotalCount)> GetUserConversationsAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Conversations
            .AsNoTracking()
            .Where(c => c.Participant1Id == userId || c.Participant2Id == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (conversations, totalCount);
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _context.Conversations.AddAsync(conversation, cancellationToken);
    }

    public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(conversation);
        if (entry.State == EntityState.Detached)
        {
            _context.Conversations.Update(conversation);
        }
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
