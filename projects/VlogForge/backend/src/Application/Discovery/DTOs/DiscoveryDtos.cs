using VlogForge.Application.Profiles.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Discovery.DTOs;

/// <summary>
/// DTO for a creator in discovery results.
/// Story: ACF-010
/// </summary>
public sealed class DiscoveryCreatorDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string? ProfilePictureUrl { get; init; }
    public bool OpenToCollaborations { get; init; }
    public IReadOnlyList<string> NicheTags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<DiscoveryPlatformDto> Platforms { get; init; } = Array.Empty<DiscoveryPlatformDto>();
    public int TotalFollowers { get; init; }

    public static DiscoveryCreatorDto FromEntity(CreatorProfile profile)
    {
        var platforms = profile.ConnectedPlatforms
            .Select(DiscoveryPlatformDto.FromEntity)
            .ToList();

        return new DiscoveryCreatorDto
        {
            Id = profile.Id,
            Username = profile.Username,
            DisplayName = profile.DisplayName,
            Bio = profile.Bio.Value,
            ProfilePictureUrl = profile.ProfilePictureUrl,
            OpenToCollaborations = profile.OpenToCollaborations,
            NicheTags = profile.NicheTags.Select(t => t.Value).ToList(),
            Platforms = platforms,
            TotalFollowers = platforms.Sum(p => p.FollowerCount ?? 0)
        };
    }
}

/// <summary>
/// Platform info for discovery results.
/// Story: ACF-010
/// </summary>
public sealed class DiscoveryPlatformDto
{
    public string PlatformType { get; init; } = string.Empty;
    public string Handle { get; init; } = string.Empty;
    public int? FollowerCount { get; init; }

    public static DiscoveryPlatformDto FromEntity(ConnectedPlatform platform)
    {
        return new DiscoveryPlatformDto
        {
            PlatformType = platform.PlatformType.ToString(),
            Handle = platform.Handle,
            FollowerCount = platform.FollowerCount
        };
    }
}

/// <summary>
/// Response for discovery query with cursor-based pagination.
/// Story: ACF-010
/// </summary>
public sealed class DiscoveryResponse
{
    public IReadOnlyList<DiscoveryCreatorDto> Items { get; init; } = new List<DiscoveryCreatorDto>();
    public int TotalCount { get; init; }
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Audience size range options for filtering.
/// Story: ACF-010
/// </summary>
public enum AudienceSizeRange
{
    /// <summary>1K - 10K followers</summary>
    Small,
    /// <summary>10K - 100K followers</summary>
    Medium,
    /// <summary>100K+ followers</summary>
    Large
}
