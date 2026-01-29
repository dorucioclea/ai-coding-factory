using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Represents an activity entry in a shared project's activity feed.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectActivity : Entity
{
    public const int MaxMessageLength = 500;

    /// <summary>
    /// Gets the shared project ID.
    /// </summary>
    public Guid SharedProjectId { get; private set; }

    /// <summary>
    /// Gets the user ID who performed the action.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the activity type.
    /// </summary>
    public SharedProjectActivityType ActivityType { get; private set; }

    /// <summary>
    /// Gets the human-readable activity message.
    /// </summary>
    public string Message { get; private set; }

    private SharedProjectActivity() : base()
    {
        Message = string.Empty;
    }

    private SharedProjectActivity(Guid sharedProjectId, Guid userId,
        SharedProjectActivityType activityType, string message) : base()
    {
        SharedProjectId = sharedProjectId;
        UserId = userId;
        ActivityType = activityType;
        Message = message;
    }

    /// <summary>
    /// Creates a new activity entry.
    /// </summary>
    public static SharedProjectActivity Create(Guid sharedProjectId, Guid userId,
        SharedProjectActivityType activityType, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Activity message cannot be empty.", nameof(message));

        var trimmed = message.Trim();
        if (trimmed.Length > MaxMessageLength)
            trimmed = trimmed[..MaxMessageLength];

        return new SharedProjectActivity(sharedProjectId, userId, activityType, trimmed);
    }
}
