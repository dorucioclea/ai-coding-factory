namespace VlogForge.Api.Controllers.Conversations.Requests;

/// <summary>
/// Request to start a conversation.
/// Story: ACF-012
/// </summary>
public sealed class StartConversationRequest
{
    public Guid ParticipantId { get; set; }
}

/// <summary>
/// Request to send a message.
/// Story: ACF-012
/// </summary>
public sealed class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
}
