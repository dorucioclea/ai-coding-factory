using System.ComponentModel.DataAnnotations;

namespace VlogForge.Api.Controllers.Profiles.Requests;

/// <summary>
/// Request to create a creator profile.
/// Story: ACF-002
/// </summary>
public record CreateProfileRequest
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Required]
    [MinLength(3)]
    [MaxLength(30)]
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the bio (optional).
    /// </summary>
    [MaxLength(500)]
    public string? Bio { get; init; }
}

/// <summary>
/// Request to update a creator profile.
/// Story: ACF-002
/// </summary>
public record UpdateProfileRequest
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the bio.
    /// </summary>
    [MaxLength(500)]
    public string? Bio { get; init; }

    /// <summary>
    /// Gets or sets the niche tags.
    /// </summary>
    public IReadOnlyList<string>? NicheTags { get; init; }

    /// <summary>
    /// Gets or sets whether the creator is open to collaborations.
    /// </summary>
    public bool? OpenToCollaborations { get; init; }

    /// <summary>
    /// Gets or sets the collaboration preferences.
    /// </summary>
    [MaxLength(500)]
    public string? CollaborationPreferences { get; init; }
}

/// <summary>
/// Request to set collaboration settings.
/// Story: ACF-002
/// </summary>
public record SetCollaborationSettingsRequest
{
    /// <summary>
    /// Gets or sets whether the creator is open to collaborations.
    /// </summary>
    [Required]
    public bool OpenToCollaborations { get; init; }

    /// <summary>
    /// Gets or sets the collaboration preferences.
    /// </summary>
    [MaxLength(500)]
    public string? CollaborationPreferences { get; init; }
}
