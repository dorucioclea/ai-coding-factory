using Microsoft.EntityFrameworkCore;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Message entity.
/// Story: ACF-012
/// </summary>
public sealed class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _context;

    public MessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Message> Messages, int TotalCount)> GetConversationMessagesAsync(
        Guid conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        var totalCount = await query.CountAsync(cancellationToken);

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (messages, totalCount);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => !m.IsRead && m.SenderId != userId)
            .Join(
                _context.Conversations,
                m => m.ConversationId,
                c => c.Id,
                (m, c) => new { Message = m, Conversation = c })
            .Where(mc => mc.Conversation.Participant1Id == userId || mc.Conversation.Participant2Id == userId)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountForConversationAsync(
        Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId &&
                        !m.IsRead &&
                        m.SenderId != userId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetUnreadCountsForConversationsAsync(
        IReadOnlyCollection<Guid> conversationIds, Guid userId, CancellationToken cancellationToken = default)
    {
        if (conversationIds.Count == 0)
            return new Dictionary<Guid, int>();

        var counts = await _context.Messages
            .AsNoTracking()
            .Where(m => conversationIds.Contains(m.ConversationId) &&
                        !m.IsRead &&
                        m.SenderId != userId)
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Count, cancellationToken);

        return counts;
    }

    public async Task<int> CountSentInLastMinuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.SenderId == userId && m.CreatedAt > oneMinuteAgo)
            .CountAsync(cancellationToken);
    }

    public async Task<int> MarkConversationMessagesAsReadAsync(
        Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId &&
                        !m.IsRead &&
                        m.SenderId != userId)
            .ExecuteUpdateAsync(
                setter => setter
                    .SetProperty(m => m.IsRead, true)
                    .SetProperty(m => m.ReadAt, now),
                cancellationToken);
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        await _context.Messages.AddAsync(message, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
