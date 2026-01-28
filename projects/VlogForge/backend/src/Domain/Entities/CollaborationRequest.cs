using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Status of a collaboration request.
/// Story: ACF-011
/// </summary>
public enum CollaborationRequestStatus
{
    /// <summary>Request is awaiting response.</summary>
    Pending = 0,

    /// <summary>Request was accepted by recipient.</summary>
    Accepted = 1,

    /// <summary>Request was declined by recipient.</summary>
    Declined = 2,

    /// <summary>Request was withdrawn by sender.</summary>
    Withdrawn = 3,

    /// <summary>Request expired without response (14 days).</summary>
    Expired = 4
}

/// <summary>
/// Collaboration request aggregate root.
/// Represents a proposal from one creator to another for a joint project.
/// Story: ACF-011
/// </summary>
public sealed class CollaborationRequest : AggregateRoot
{
    public const int MaxMessageLength = 1000;
    public const int MaxDeclineReasonLength = 500;
    public const int ExpirationDays = 14;
    public const int MaxRequestsPerDayPerUser = 5;

    /// <summary>
    /// Gets the user ID of the sender.
    /// </summary>
    public Guid SenderId { get; private set; }

    /// <summary>
    /// Gets the user ID of the recipient.
    /// </summary>
    public Guid RecipientId { get; private set; }

    /// <summary>
    /// Gets the proposal message.
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Gets the current status of the request.
    /// </summary>
    public CollaborationRequestStatus Status { get; private set; }

    /// <summary>
    /// Gets the date when the request expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the date when the request was responded to (accepted/declined).
    /// </summary>
    public DateTime? RespondedAt { get; private set; }

    /// <summary>
    /// Gets the optional decline reason.
    /// </summary>
    public string? DeclineReason { get; private set; }

    private CollaborationRequest() : base()
    {
        Message = string.Empty;
    }

    private CollaborationRequest(Guid senderId, Guid recipientId, string message) : base()
    {
        SenderId = senderId;
        RecipientId = recipientId;
        Message = message;
        Status = CollaborationRequestStatus.Pending;
        ExpiresAt = DateTime.UtcNow.AddDays(ExpirationDays);
    }

    /// <summary>
    /// Creates a new collaboration request.
    /// </summary>
    /// <param name="senderId">The sender's user ID.</param>
    /// <param name="recipientId">The recipient's user ID.</param>
    /// <param name="message">The proposal message (max 1000 chars).</param>
    /// <returns>A new CollaborationRequest instance.</returns>
    public static CollaborationRequest Create(Guid senderId, Guid recipientId, string message)
    {
        if (senderId == Guid.Empty)
            throw new ArgumentException("Sender ID cannot be empty.", nameof(senderId));

        if (recipientId == Guid.Empty)
            throw new ArgumentException("Recipient ID cannot be empty.", nameof(recipientId));

        if (senderId == recipientId)
            throw new ArgumentException("Cannot send a collaboration request to yourself.", nameof(recipientId));

        ValidateMessage(message);

        var request = new CollaborationRequest(senderId, recipientId, message.Trim());
        request.RaiseDomainEvent(new CollaborationRequestCreatedEvent(
            request.Id, senderId, recipientId));

        return request;
    }

    /// <summary>
    /// Accepts the collaboration request.
    /// </summary>
    /// <param name="userId">The user ID accepting (must be recipient).</param>
    public void Accept(Guid userId)
    {
        EnsureRecipient(userId);
        EnsurePending();
        EnsureNotExpired();

        Status = CollaborationRequestStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
        IncrementVersion();

        RaiseDomainEvent(new CollaborationRequestAcceptedEvent(Id, SenderId, RecipientId));
    }

    /// <summary>
    /// Declines the collaboration request.
    /// </summary>
    /// <param name="userId">The user ID declining (must be recipient).</param>
    /// <param name="reason">Optional decline reason.</param>
    public void Decline(Guid userId, string? reason = null)
    {
        EnsureRecipient(userId);
        EnsurePending();
        EnsureNotExpired();

        if (reason is not null)
        {
            reason = reason.Trim();
            if (reason.Length > MaxDeclineReasonLength)
                throw new ArgumentException(
                    $"Decline reason cannot exceed {MaxDeclineReasonLength} characters.", nameof(reason));
        }

        Status = CollaborationRequestStatus.Declined;
        DeclineReason = reason;
        RespondedAt = DateTime.UtcNow;
        IncrementVersion();

        RaiseDomainEvent(new CollaborationRequestDeclinedEvent(Id, SenderId, RecipientId));
    }

    /// <summary>
    /// Withdraws the collaboration request (sender cancels).
    /// </summary>
    /// <param name="userId">The user ID withdrawing (must be sender).</param>
    public void Withdraw(Guid userId)
    {
        if (userId != SenderId)
            throw new UnauthorizedAccessException("Only the sender can withdraw a collaboration request.");

        EnsurePending();

        Status = CollaborationRequestStatus.Withdrawn;
        RespondedAt = DateTime.UtcNow;
        IncrementVersion();

        RaiseDomainEvent(new CollaborationRequestWithdrawnEvent(Id, SenderId, RecipientId));
    }

    /// <summary>
    /// Marks the request as expired.
    /// </summary>
    public void MarkExpired()
    {
        if (Status != CollaborationRequestStatus.Pending)
            return;

        Status = CollaborationRequestStatus.Expired;
        IncrementVersion();

        RaiseDomainEvent(new CollaborationRequestExpiredEvent(Id, SenderId, RecipientId));
    }

    /// <summary>
    /// Gets whether this request has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Gets whether this request is still pending and not expired.
    /// </summary>
    public bool IsActive => Status == CollaborationRequestStatus.Pending && !IsExpired;

    private void EnsureRecipient(Guid userId)
    {
        if (userId != RecipientId)
            throw new UnauthorizedAccessException("Only the recipient can respond to a collaboration request.");
    }

    private void EnsurePending()
    {
        if (Status != CollaborationRequestStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot perform this action on a request with status '{Status}'.");
    }

    private void EnsureNotExpired()
    {
        if (IsExpired)
            throw new InvalidOperationException("This collaboration request has expired.");
    }

    private static void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        if (message.Trim().Length > MaxMessageLength)
            throw new ArgumentException(
                $"Message cannot exceed {MaxMessageLength} characters.", nameof(message));
    }
}
