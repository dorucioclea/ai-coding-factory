using VlogForge.Domain.Entities;

namespace VlogForge.Application.Messaging.DTOs;

/// <summary>
/// DTO for a conversation.
/// Story: ACF-012
/// </summary>
public sealed class ConversationResponse
{
    public Guid Id { get; init; }
    public Guid ParticipantId { get; init; }
    public string ParticipantDisplayName { get; init; } = string.Empty;
    public string ParticipantUsername { get; init; } = string.Empty;
    public string? ParticipantProfilePictureUrl { get; init; }
    public string? LastMessagePreview { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public int UnreadCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static ConversationResponse FromEntity(
        Conversation conversation,
        Guid currentUserId,
        CreatorProfile? participantProfile,
        int unreadCount)
    {
        var otherParticipantId = conversation.GetOtherParticipantId(currentUserId);

        return new ConversationResponse
        {
            Id = conversation.Id,
            ParticipantId = otherParticipantId,
            ParticipantDisplayName = participantProfile?.DisplayName ?? string.Empty,
            ParticipantUsername = participantProfile?.Username ?? string.Empty,
            ParticipantProfilePictureUrl = participantProfile?.ProfilePictureUrl,
            LastMessagePreview = conversation.LastMessagePreview,
            LastMessageAt = conversation.LastMessageAt,
            UnreadCount = unreadCount,
            CreatedAt = conversation.CreatedAt,
        };
    }
}

/// <summary>
/// Paginated list of conversations.
/// Story: ACF-012
/// </summary>
public sealed class ConversationListResponse
{
    public IReadOnlyList<ConversationResponse> Items { get; init; } = new List<ConversationResponse>();
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// DTO for a message.
/// Story: ACF-012
/// </summary>
public sealed class MessageResponse
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderDisplayName { get; init; } = string.Empty;
    public string SenderUsername { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime CreatedAt { get; init; }

    public static MessageResponse FromEntity(
        Message message,
        CreatorProfile? senderProfile = null)
    {
        return new MessageResponse
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderDisplayName = senderProfile?.DisplayName ?? string.Empty,
            SenderUsername = senderProfile?.Username ?? string.Empty,
            Content = message.Content,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            CreatedAt = message.CreatedAt,
        };
    }
}

/// <summary>
/// Paginated list of messages.
/// Story: ACF-012
/// </summary>
public sealed class MessageListResponse
{
    public IReadOnlyList<MessageResponse> Items { get; init; } = new List<MessageResponse>();
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
