namespace VlogForge.Api.Controllers.Collaborations.Requests;

/// <summary>
/// Request to send a collaboration request.
/// Story: ACF-011
/// </summary>
public sealed class SendCollaborationRequestRequest
{
    /// <summary>
    /// The recipient's user ID.
    /// </summary>
    public Guid RecipientId { get; init; }

    /// <summary>
    /// The proposal message (max 1000 characters).
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Request to decline a collaboration request.
/// Story: ACF-011
/// </summary>
public sealed class DeclineCollaborationRequestRequest
{
    /// <summary>
    /// Optional reason for declining.
    /// </summary>
    public string? Reason { get; init; }
}
