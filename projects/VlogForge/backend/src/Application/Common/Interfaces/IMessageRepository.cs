using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Message operations.
/// Story: ACF-012
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Gets a message by ID.
    /// </summary>
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated messages for a conversation, ordered by newest first.
    /// </summary>
    Task<(IReadOnlyList<Message> Messages, int TotalCount)> GetConversationMessagesAsync(
        Guid conversationId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total unread message count across all conversations for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unread message count for a specific conversation for a user.
    /// </summary>
    Task<int> GetUnreadCountForConversationAsync(
        Guid conversationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts messages sent by a user in the last minute (for rate limiting).
    /// </summary>
    Task<int> CountSentInLastMinuteAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all unread messages in a conversation as read for a user (bulk operation).
    /// Returns the number of messages marked as read.
    /// </summary>
    Task<int> MarkConversationMessagesAsReadAsync(
        Guid conversationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new message.
    /// </summary>
    Task AddAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
