using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Enum representing supported social media platforms.
/// Story: ACF-002
/// </summary>
public enum PlatformType
{
    YouTube,
    TikTok,
    Instagram,
    Twitter,
    Twitch,
    LinkedIn,
    Facebook,
    Website
}

/// <summary>
/// Enum representing the connection status of an OAuth-linked platform.
/// Story: ACF-003
/// </summary>
public enum ConnectionStatus
{
    Connected,
    Disconnected,
    Error,
    TokenExpired
}

/// <summary>
/// Entity representing a connected social media platform on a creator profile.
/// Story: ACF-002
/// </summary>
public sealed class ConnectedPlatform : Entity
{
    /// <summary>
    /// Gets the profile ID this platform belongs to.
    /// </summary>
    public Guid ProfileId { get; private set; }

    /// <summary>
    /// Gets the platform type.
    /// </summary>
    public PlatformType PlatformType { get; private set; }

    /// <summary>
    /// Gets the platform-specific username or handle.
    /// </summary>
    public string Handle { get; private set; }

    /// <summary>
    /// Gets the full URL to the profile on this platform.
    /// </summary>
    public string ProfileUrl { get; private set; }

    /// <summary>
    /// Gets whether this platform connection has been verified.
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Gets the follower count (if available, for display purposes).
    /// </summary>
    public int? FollowerCount { get; private set; }

    private ConnectedPlatform() : base()
    {
        Handle = string.Empty;
        ProfileUrl = string.Empty;
    }

    private ConnectedPlatform(Guid profileId, PlatformType platformType, string handle, string profileUrl)
        : base()
    {
        ProfileId = profileId;
        PlatformType = platformType;
        Handle = handle;
        ProfileUrl = profileUrl;
        IsVerified = false;
    }

    /// <summary>
    /// Creates a new connected platform.
    /// </summary>
    public static ConnectedPlatform Create(Guid profileId, PlatformType platformType, string handle, string profileUrl)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));

        if (string.IsNullOrWhiteSpace(handle))
            throw new ArgumentException("Handle cannot be empty.", nameof(handle));

        if (string.IsNullOrWhiteSpace(profileUrl))
            throw new ArgumentException("Profile URL cannot be empty.", nameof(profileUrl));

        if (!Uri.TryCreate(profileUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Profile URL must be a valid absolute URL.", nameof(profileUrl));

        return new ConnectedPlatform(profileId, platformType, handle.Trim(), profileUrl.Trim());
    }

    /// <summary>
    /// Marks this platform connection as verified.
    /// </summary>
    public void Verify()
    {
        IsVerified = true;
    }

    /// <summary>
    /// Updates the follower count.
    /// </summary>
    public void UpdateFollowerCount(int? count)
    {
        if (count.HasValue && count.Value < 0)
            throw new ArgumentException("Follower count cannot be negative.", nameof(count));

        FollowerCount = count;
    }

    /// <summary>
    /// Updates the handle and profile URL.
    /// </summary>
    public void UpdateDetails(string handle, string profileUrl)
    {
        if (string.IsNullOrWhiteSpace(handle))
            throw new ArgumentException("Handle cannot be empty.", nameof(handle));

        if (string.IsNullOrWhiteSpace(profileUrl))
            throw new ArgumentException("Profile URL cannot be empty.", nameof(profileUrl));

        if (!Uri.TryCreate(profileUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Profile URL must be a valid absolute URL.", nameof(profileUrl));

        Handle = handle.Trim();
        ProfileUrl = profileUrl.Trim();
        IsVerified = false; // Re-verification needed after update
    }
}
