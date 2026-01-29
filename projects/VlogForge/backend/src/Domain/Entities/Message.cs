using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Message entity representing a single message in a conversation.
/// Child entity of Conversation (not an aggregate root).
/// Story: ACF-012
/// </summary>
public sealed class Message : Entity
{
    public const int MaxContentLength = 2000;
    public const int MaxMessagesPerMinute = 60;

    /// <summary>
    /// Gets the conversation this message belongs to.
    /// </summary>
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// Gets the user ID of the message sender.
    /// </summary>
    public Guid SenderId { get; private set; }

    /// <summary>
    /// Gets the message content.
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Gets whether this message has been read by the recipient.
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// Gets the timestamp when this message was read.
    /// </summary>
    public DateTime? ReadAt { get; private set; }

    private Message() : base()
    {
        Content = string.Empty;
    }

    private Message(Guid conversationId, Guid senderId, string content) : base()
    {
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
        IsRead = false;
    }

    /// <summary>
    /// Creates a new message.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="senderId">The sender's user ID.</param>
    /// <param name="content">The message content (max 2000 chars).</param>
    /// <returns>A new Message instance.</returns>
    public static Message Create(Guid conversationId, Guid senderId, string content)
    {
        if (conversationId == Guid.Empty)
            throw new ArgumentException("Conversation ID cannot be empty.", nameof(conversationId));

        if (senderId == Guid.Empty)
            throw new ArgumentException("Sender ID cannot be empty.", nameof(senderId));

        ValidateContent(content);

        var message = new Message(conversationId, senderId, content.Trim());
        message.RaiseDomainEvent(new MessageSentEvent(
            message.Id, conversationId, senderId));

        return message;
    }

    /// <summary>
    /// Marks this message as read.
    /// No-op if already read.
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;

        RaiseDomainEvent(new MessageReadEvent(Id, ConversationId, SenderId));
    }

    private static void ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.", nameof(content));

        if (content.Trim().Length > MaxContentLength)
            throw new ArgumentException(
                $"Message content cannot exceed {MaxContentLength} characters.", nameof(content));
    }
}
