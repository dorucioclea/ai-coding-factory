using VlogForge.Domain.Common;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when a new conversation is created.
/// Story: ACF-012
/// </summary>
public sealed class ConversationCreatedEvent : DomainEvent
{
    public Guid ConversationId { get; }
    public Guid Participant1Id { get; }
    public Guid Participant2Id { get; }

    public ConversationCreatedEvent(Guid conversationId, Guid participant1Id, Guid participant2Id)
    {
        ConversationId = conversationId;
        Participant1Id = participant1Id;
        Participant2Id = participant2Id;
    }
}

/// <summary>
/// Event raised when a message is sent.
/// Story: ACF-012
/// </summary>
public sealed class MessageSentEvent : DomainEvent
{
    public Guid MessageId { get; }
    public Guid ConversationId { get; }
    public Guid SenderId { get; }
    public string Content { get; }

    public MessageSentEvent(Guid messageId, Guid conversationId, Guid senderId, string content)
    {
        MessageId = messageId;
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
    }
}

/// <summary>
/// Event raised when a message is read.
/// Story: ACF-012
/// </summary>
public sealed class MessageReadEvent : DomainEvent
{
    public Guid MessageId { get; }
    public Guid ConversationId { get; }
    public Guid ReadByUserId { get; }

    public MessageReadEvent(Guid messageId, Guid conversationId, Guid readByUserId)
    {
        MessageId = messageId;
        ConversationId = conversationId;
        ReadByUserId = readByUserId;
    }
}
