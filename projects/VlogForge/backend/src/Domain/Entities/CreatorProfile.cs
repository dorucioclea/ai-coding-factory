using VlogForge.Domain.Common;
using VlogForge.Domain.Events;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Entity representing a creator's public profile.
/// Story: ACF-002
/// </summary>
public sealed class CreatorProfile : AggregateRoot
{
    public const int MaxNicheTags = 5;
    public const int MaxUsernameLength = 30;
    public const int MinUsernameLength = 3;

    private readonly List<NicheTag> _nicheTags = new();
    private readonly List<ConnectedPlatform> _connectedPlatforms = new();

    /// <summary>
    /// Gets the user ID this profile belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the unique username for URL routing (e.g., /profiles/johndoe).
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// Gets the display name shown on the profile.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the creator's bio/description.
    /// </summary>
    public Bio Bio { get; private set; }

    /// <summary>
    /// Gets the URL to the profile picture (null if no picture uploaded).
    /// </summary>
    public string? ProfilePictureUrl { get; private set; }

    /// <summary>
    /// Gets whether the creator is open to collaborations.
    /// </summary>
    public bool OpenToCollaborations { get; private set; }

    /// <summary>
    /// Gets the collaboration preferences description.
    /// </summary>
    public string? CollaborationPreferences { get; private set; }

    /// <summary>
    /// Gets the niche tags for this profile (max 5).
    /// </summary>
    public IReadOnlyCollection<NicheTag> NicheTags => _nicheTags.AsReadOnly();

    /// <summary>
    /// Gets the connected social media platforms.
    /// </summary>
    public IReadOnlyCollection<ConnectedPlatform> ConnectedPlatforms => _connectedPlatforms.AsReadOnly();

    private CreatorProfile() : base()
    {
        Username = string.Empty;
        DisplayName = string.Empty;
        Bio = Bio.Empty;
    }

    private CreatorProfile(Guid userId, string username, string displayName) : base()
    {
        UserId = userId;
        Username = username;
        DisplayName = displayName;
        Bio = Bio.Empty;
        OpenToCollaborations = false;
    }

    /// <summary>
    /// Creates a new creator profile.
    /// </summary>
    public static CreatorProfile Create(Guid userId, string username, string displayName)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        ValidateUsername(username);
        ValidateDisplayName(displayName);

        var profile = new CreatorProfile(userId, NormalizeUsername(username), displayName.Trim());
        profile.RaiseDomainEvent(new CreatorProfileCreatedEvent(profile.Id, profile.UserId, profile.Username));

        return profile;
    }

    /// <summary>
    /// Updates the profile's basic information.
    /// </summary>
    public void UpdateBasicInfo(string displayName, Bio bio)
    {
        ValidateDisplayName(displayName);

        DisplayName = displayName.Trim();
        Bio = bio;
        IncrementVersion();

        RaiseDomainEvent(new CreatorProfileUpdatedEvent(Id, UserId));
    }

    /// <summary>
    /// Updates the profile picture URL.
    /// </summary>
    public void UpdateProfilePicture(string? url)
    {
        if (url != null && !Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Profile picture URL must be a valid absolute URL.", nameof(url));
        }

        ProfilePictureUrl = url;
        IncrementVersion();
    }

    /// <summary>
    /// Sets the collaboration availability and preferences.
    /// </summary>
    public void SetCollaborationSettings(bool openToCollaborations, string? preferences = null)
    {
        if (preferences != null && preferences.Length > 500)
        {
            throw new ArgumentException("Collaboration preferences cannot exceed 500 characters.", nameof(preferences));
        }

        var wasOpen = OpenToCollaborations;
        OpenToCollaborations = openToCollaborations;
        CollaborationPreferences = preferences?.Trim();
        IncrementVersion();

        if (wasOpen != openToCollaborations)
        {
            RaiseDomainEvent(new CollaborationAvailabilityChangedEvent(Id, UserId, openToCollaborations));
        }
    }

    /// <summary>
    /// Sets the niche tags for this profile.
    /// </summary>
    public void SetNicheTags(IEnumerable<NicheTag> tags)
    {
        var tagList = tags.Distinct().Take(MaxNicheTags).ToList();

        _nicheTags.Clear();
        _nicheTags.AddRange(tagList);
        IncrementVersion();
    }

    /// <summary>
    /// Adds a niche tag to the profile.
    /// </summary>
    public bool AddNicheTag(NicheTag tag)
    {
        if (_nicheTags.Count >= MaxNicheTags)
            return false;

        if (_nicheTags.Contains(tag))
            return false;

        _nicheTags.Add(tag);
        IncrementVersion();
        return true;
    }

    /// <summary>
    /// Removes a niche tag from the profile.
    /// </summary>
    public bool RemoveNicheTag(NicheTag tag)
    {
        var removed = _nicheTags.Remove(tag);
        if (removed)
        {
            IncrementVersion();
        }
        return removed;
    }

    /// <summary>
    /// Adds a connected platform to the profile.
    /// </summary>
    public void AddConnectedPlatform(ConnectedPlatform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        // Remove existing platform of same type
        _connectedPlatforms.RemoveAll(p => p.PlatformType == platform.PlatformType);
        _connectedPlatforms.Add(platform);
        IncrementVersion();
    }

    /// <summary>
    /// Removes a connected platform from the profile.
    /// </summary>
    public bool RemoveConnectedPlatform(PlatformType platformType)
    {
        var removed = _connectedPlatforms.RemoveAll(p => p.PlatformType == platformType) > 0;
        if (removed)
        {
            IncrementVersion();
        }
        return removed;
    }

    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        var normalized = username.Trim().ToLowerInvariant();

        if (normalized.Length < MinUsernameLength)
            throw new ArgumentException($"Username must be at least {MinUsernameLength} characters.", nameof(username));

        if (normalized.Length > MaxUsernameLength)
            throw new ArgumentException($"Username cannot exceed {MaxUsernameLength} characters.", nameof(username));

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[a-z0-9_]+$"))
            throw new ArgumentException("Username can only contain letters, numbers, and underscores.", nameof(username));
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        if (displayName.Trim().Length < 2)
            throw new ArgumentException("Display name must be at least 2 characters.", nameof(displayName));

        if (displayName.Trim().Length > 100)
            throw new ArgumentException("Display name cannot exceed 100 characters.", nameof(displayName));
    }

    private static string NormalizeUsername(string username) => username.Trim().ToLowerInvariant();
}
