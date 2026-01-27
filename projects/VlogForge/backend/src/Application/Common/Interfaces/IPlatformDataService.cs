using VlogForge.Domain.Entities;

namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Interface for fetching data from social media platforms.
/// Story: ACF-004
/// </summary>
public interface IPlatformDataService
{
    /// <summary>
    /// Gets the platform this service supports.
    /// </summary>
    PlatformType SupportedPlatform { get; }

    /// <summary>
    /// Fetches current metrics for the authenticated user.
    /// </summary>
    Task<PlatformMetricsData> FetchMetricsAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches top performing content for the authenticated user.
    /// </summary>
    Task<IReadOnlyList<ContentData>> FetchTopContentAsync(
        string accessToken,
        int limit = 10,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer object for platform metrics.
/// </summary>
public record PlatformMetricsData(
    long FollowerCount,
    long TotalViews,
    long TotalLikes,
    long TotalComments,
    long TotalShares);

/// <summary>
/// Data transfer object for content data.
/// </summary>
public record ContentData(
    string ContentId,
    string Title,
    string? ThumbnailUrl,
    string ContentUrl,
    DateTime PublishedAt,
    long ViewCount,
    long LikeCount,
    long CommentCount,
    long ShareCount);
