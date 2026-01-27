using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Actions that can be performed in an approval workflow.
/// Story: ACF-009
/// </summary>
public enum ApprovalAction
{
    /// <summary>Content was submitted for approval.</summary>
    Submitted = 0,

    /// <summary>Content was approved.</summary>
    Approved = 1,

    /// <summary>Changes were requested.</summary>
    ChangesRequested = 2,

    /// <summary>Content was resubmitted after changes.</summary>
    Resubmitted = 3
}

/// <summary>
/// Entity representing a record in the approval workflow history.
/// Tracks who performed what action and when, with optional feedback.
/// Story: ACF-009
/// </summary>
public sealed class ApprovalRecord : Entity
{
    public const int MaxFeedbackLength = 2000;

    /// <summary>
    /// Gets the content item ID this approval record belongs to.
    /// </summary>
    public Guid ContentItemId { get; private set; }

    /// <summary>
    /// Gets the team ID associated with this approval.
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the user ID who performed the action.
    /// </summary>
    public Guid ActorId { get; private set; }

    /// <summary>
    /// Gets the action that was performed.
    /// </summary>
    public ApprovalAction Action { get; private set; }

    /// <summary>
    /// Gets the optional feedback/comments for the action.
    /// </summary>
    public string? Feedback { get; private set; }

    /// <summary>
    /// Gets the status before this action was taken.
    /// </summary>
    public IdeaStatus PreviousStatus { get; private set; }

    /// <summary>
    /// Gets the status after this action was taken.
    /// </summary>
    public IdeaStatus NewStatus { get; private set; }

    private ApprovalRecord() : base()
    {
    }

    private ApprovalRecord(
        Guid contentItemId,
        Guid teamId,
        Guid actorId,
        ApprovalAction action,
        string? feedback,
        IdeaStatus previousStatus,
        IdeaStatus newStatus) : base()
    {
        ContentItemId = contentItemId;
        TeamId = teamId;
        ActorId = actorId;
        Action = action;
        Feedback = feedback;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }

    /// <summary>
    /// Creates a new approval record.
    /// </summary>
    /// <param name="contentItemId">The content item ID.</param>
    /// <param name="teamId">The team ID.</param>
    /// <param name="actorId">The user performing the action.</param>
    /// <param name="action">The approval action.</param>
    /// <param name="previousStatus">Status before the action.</param>
    /// <param name="newStatus">Status after the action.</param>
    /// <param name="feedback">Optional feedback or comments.</param>
    /// <returns>A new ApprovalRecord instance.</returns>
    public static ApprovalRecord Create(
        Guid contentItemId,
        Guid teamId,
        Guid actorId,
        ApprovalAction action,
        IdeaStatus previousStatus,
        IdeaStatus newStatus,
        string? feedback = null)
    {
        if (contentItemId == Guid.Empty)
            throw new ArgumentException("Content item ID cannot be empty.", nameof(contentItemId));

        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));

        if (actorId == Guid.Empty)
            throw new ArgumentException("Actor ID cannot be empty.", nameof(actorId));

        ValidateFeedback(feedback);

        return new ApprovalRecord(
            contentItemId,
            teamId,
            actorId,
            action,
            feedback?.Trim(),
            previousStatus,
            newStatus);
    }

    private static void ValidateFeedback(string? feedback)
    {
        if (feedback is not null && feedback.Length > MaxFeedbackLength)
            throw new ArgumentException($"Feedback cannot exceed {MaxFeedbackLength} characters.", nameof(feedback));
    }
}
