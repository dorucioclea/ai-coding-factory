using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Conversation aggregate root.
/// Represents a messaging thread between two creators.
/// Story: ACF-012
/// </summary>
public sealed class Conversation : AggregateRoot
{
    public const int MaxPreviewLength = 100;

    /// <summary>
    /// Gets the user ID of the first participant.
    /// </summary>
    public Guid Participant1Id { get; private set; }

    /// <summary>
    /// Gets the user ID of the second participant.
    /// </summary>
    public Guid Participant2Id { get; private set; }

    /// <summary>
    /// Gets the timestamp of the last message in this conversation.
    /// </summary>
    public DateTime? LastMessageAt { get; private set; }

    /// <summary>
    /// Gets a preview of the last message content.
    /// </summary>
    public string? LastMessagePreview { get; private set; }

    private Conversation() : base()
    {
    }

    private Conversation(Guid participant1Id, Guid participant2Id) : base()
    {
        Participant1Id = participant1Id;
        Participant2Id = participant2Id;
    }

    /// <summary>
    /// Creates a new conversation between two participants.
    /// </summary>
    /// <param name="participant1Id">The first participant's user ID.</param>
    /// <param name="participant2Id">The second participant's user ID.</param>
    /// <returns>A new Conversation instance.</returns>
    public static Conversation Create(Guid participant1Id, Guid participant2Id)
    {
        if (participant1Id == Guid.Empty)
            throw new ArgumentException("Participant 1 ID cannot be empty.", nameof(participant1Id));

        if (participant2Id == Guid.Empty)
            throw new ArgumentException("Participant 2 ID cannot be empty.", nameof(participant2Id));

        if (participant1Id == participant2Id)
            throw new ArgumentException("Cannot create a conversation with yourself.", nameof(participant2Id));

        // Normalize participant order so (A,B) and (B,A) produce the same unique row
        var (p1, p2) = participant1Id.CompareTo(participant2Id) < 0
            ? (participant1Id, participant2Id)
            : (participant2Id, participant1Id);

        var conversation = new Conversation(p1, p2);
        conversation.RaiseDomainEvent(new ConversationCreatedEvent(
            conversation.Id, participant1Id, participant2Id));

        return conversation;
    }

    /// <summary>
    /// Updates the last message preview and timestamp.
    /// </summary>
    /// <param name="preview">The message content preview.</param>
    public void UpdateLastMessage(string preview)
    {
        if (string.IsNullOrWhiteSpace(preview))
            throw new ArgumentException("Preview cannot be empty.", nameof(preview));

        LastMessagePreview = preview.Length > MaxPreviewLength
            ? preview[..MaxPreviewLength] + "..."
            : preview;
        LastMessageAt = DateTime.UtcNow;
        IncrementVersion();
    }

    /// <summary>
    /// Checks whether a user is a participant in this conversation.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user is a participant.</returns>
    public bool IsParticipant(Guid userId)
    {
        return userId == Participant1Id || userId == Participant2Id;
    }

    /// <summary>
    /// Gets the other participant's ID given one participant.
    /// </summary>
    /// <param name="userId">The known participant's user ID.</param>
    /// <returns>The other participant's user ID.</returns>
    public Guid GetOtherParticipantId(Guid userId)
    {
        if (userId == Participant1Id)
            return Participant2Id;
        if (userId == Participant2Id)
            return Participant1Id;

        throw new InvalidOperationException("User is not a participant in this conversation.");
    }
}
