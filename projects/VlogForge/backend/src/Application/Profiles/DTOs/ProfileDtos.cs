using VlogForge.Domain.Entities;

namespace VlogForge.Application.Profiles.DTOs;

/// <summary>
/// Response containing full creator profile information.
/// Story: ACF-002
/// </summary>
public sealed class CreatorProfileResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string? ProfilePictureUrl { get; init; }
    public bool OpenToCollaborations { get; init; }
    public string? CollaborationPreferences { get; init; }
    public IReadOnlyList<string> NicheTags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ConnectedPlatformDto> ConnectedPlatforms { get; init; } = Array.Empty<ConnectedPlatformDto>();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static CreatorProfileResponse FromEntity(CreatorProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        Username = profile.Username,
        DisplayName = profile.DisplayName,
        Bio = profile.Bio.Value,
        ProfilePictureUrl = profile.ProfilePictureUrl,
        OpenToCollaborations = profile.OpenToCollaborations,
        CollaborationPreferences = profile.CollaborationPreferences,
        NicheTags = profile.NicheTags.Select(t => t.Value).ToList(),
        ConnectedPlatforms = profile.ConnectedPlatforms.Select(ConnectedPlatformDto.FromEntity).ToList(),
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}

/// <summary>
/// DTO for connected platform information.
/// Story: ACF-002
/// </summary>
public sealed class ConnectedPlatformDto
{
    public Guid Id { get; init; }
    public string PlatformType { get; init; } = string.Empty;
    public string Handle { get; init; } = string.Empty;
    public string ProfileUrl { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public int? FollowerCount { get; init; }

    public static ConnectedPlatformDto FromEntity(ConnectedPlatform platform) => new()
    {
        Id = platform.Id,
        PlatformType = platform.PlatformType.ToString(),
        Handle = platform.Handle,
        ProfileUrl = platform.ProfileUrl,
        IsVerified = platform.IsVerified,
        FollowerCount = platform.FollowerCount
    };
}

/// <summary>
/// Public profile view (limited information for non-authenticated users).
/// Story: ACF-002
/// </summary>
public sealed class PublicProfileResponse
{
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string? ProfilePictureUrl { get; init; }
    public bool OpenToCollaborations { get; init; }
    public IReadOnlyList<string> NicheTags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<PublicConnectedPlatformDto> ConnectedPlatforms { get; init; } = Array.Empty<PublicConnectedPlatformDto>();

    public static PublicProfileResponse FromEntity(CreatorProfile profile) => new()
    {
        Username = profile.Username,
        DisplayName = profile.DisplayName,
        Bio = profile.Bio.Value,
        ProfilePictureUrl = profile.ProfilePictureUrl,
        OpenToCollaborations = profile.OpenToCollaborations,
        NicheTags = profile.NicheTags.Select(t => t.Value).ToList(),
        ConnectedPlatforms = profile.ConnectedPlatforms.Select(PublicConnectedPlatformDto.FromEntity).ToList()
    };
}

/// <summary>
/// Public connected platform view.
/// Story: ACF-002
/// </summary>
public sealed class PublicConnectedPlatformDto
{
    public string PlatformType { get; init; } = string.Empty;
    public string Handle { get; init; } = string.Empty;
    public string ProfileUrl { get; init; } = string.Empty;
    public bool IsVerified { get; init; }

    public static PublicConnectedPlatformDto FromEntity(ConnectedPlatform platform) => new()
    {
        PlatformType = platform.PlatformType.ToString(),
        Handle = platform.Handle,
        ProfileUrl = platform.ProfileUrl,
        IsVerified = platform.IsVerified
    };
}
