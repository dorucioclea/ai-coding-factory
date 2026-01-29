using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Conversation aggregate operations.
/// Story: ACF-012
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Gets a conversation by ID.
    /// </summary>
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing conversation between two participants (order-independent).
    /// </summary>
    Task<Conversation?> GetByParticipantsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated conversations for a user, sorted by most recent message.
    /// </summary>
    Task<(IReadOnlyList<Conversation> Conversations, int TotalCount)> GetUserConversationsAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new conversation.
    /// </summary>
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing conversation.
    /// </summary>
    Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
