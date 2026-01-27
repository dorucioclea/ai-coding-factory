using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Entity representing performance metrics for individual content (video, post, reel).
/// Tracks top-performing content across platforms.
/// Story: ACF-004
/// </summary>
public sealed class ContentPerformance : Entity
{
    /// <summary>
    /// Gets the platform connection ID this content belongs to.
    /// </summary>
    public Guid PlatformConnectionId { get; private set; }

    /// <summary>
    /// Gets the platform type.
    /// </summary>
    public PlatformType PlatformType { get; private set; }

    /// <summary>
    /// Gets the platform-specific content identifier.
    /// </summary>
    public string ContentId { get; private set; }

    /// <summary>
    /// Gets the content title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the thumbnail URL for the content.
    /// </summary>
    public string? ThumbnailUrl { get; private set; }

    /// <summary>
    /// Gets the URL to view the content on the platform.
    /// </summary>
    public string ContentUrl { get; private set; }

    /// <summary>
    /// Gets when the content was published.
    /// </summary>
    public DateTime PublishedAt { get; private set; }

    /// <summary>
    /// Gets the view count.
    /// </summary>
    public long ViewCount { get; private set; }

    /// <summary>
    /// Gets the like count.
    /// </summary>
    public long LikeCount { get; private set; }

    /// <summary>
    /// Gets the comment count.
    /// </summary>
    public long CommentCount { get; private set; }

    /// <summary>
    /// Gets the share count.
    /// </summary>
    public long ShareCount { get; private set; }

    /// <summary>
    /// Gets the engagement rate: (likes + comments + shares) / views * 100.
    /// </summary>
    public double EngagementRate { get; private set; }

    /// <summary>
    /// Gets when these metrics were last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; private set; }

    private ContentPerformance() : base()
    {
        ContentId = string.Empty;
        Title = string.Empty;
        ContentUrl = string.Empty;
    }

    private ContentPerformance(
        Guid platformConnectionId,
        PlatformType platformType,
        string contentId,
        string title,
        string? thumbnailUrl,
        string contentUrl,
        DateTime publishedAt,
        long viewCount,
        long likeCount,
        long commentCount,
        long shareCount) : base()
    {
        PlatformConnectionId = platformConnectionId;
        PlatformType = platformType;
        ContentId = contentId;
        Title = title;
        ThumbnailUrl = thumbnailUrl;
        ContentUrl = contentUrl;
        PublishedAt = publishedAt;
        ViewCount = viewCount;
        LikeCount = likeCount;
        CommentCount = commentCount;
        ShareCount = shareCount;
        EngagementRate = CalculateEngagementRate(viewCount, likeCount, commentCount, shareCount);
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new content performance record.
    /// </summary>
    public static ContentPerformance Create(
        Guid platformConnectionId,
        PlatformType platformType,
        string contentId,
        string title,
        string? thumbnailUrl,
        string contentUrl,
        DateTime publishedAt,
        long viewCount,
        long likeCount,
        long commentCount,
        long shareCount)
    {
        if (platformConnectionId == Guid.Empty)
            throw new ArgumentException("Platform connection ID cannot be empty.", nameof(platformConnectionId));

        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentException("Content ID cannot be empty.", nameof(contentId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(contentUrl))
            throw new ArgumentException("Content URL cannot be empty.", nameof(contentUrl));

        if (viewCount < 0)
            throw new ArgumentException("View count cannot be negative.", nameof(viewCount));

        if (likeCount < 0)
            throw new ArgumentException("Like count cannot be negative.", nameof(likeCount));

        if (commentCount < 0)
            throw new ArgumentException("Comment count cannot be negative.", nameof(commentCount));

        if (shareCount < 0)
            throw new ArgumentException("Share count cannot be negative.", nameof(shareCount));

        return new ContentPerformance(
            platformConnectionId,
            platformType,
            contentId.Trim(),
            title.Trim(),
            thumbnailUrl?.Trim(),
            contentUrl.Trim(),
            publishedAt,
            viewCount,
            likeCount,
            commentCount,
            shareCount);
    }

    /// <summary>
    /// Updates the performance metrics for this content.
    /// </summary>
    public void UpdateMetrics(
        long viewCount,
        long likeCount,
        long commentCount,
        long shareCount)
    {
        if (viewCount < 0)
            throw new ArgumentException("View count cannot be negative.", nameof(viewCount));

        if (likeCount < 0)
            throw new ArgumentException("Like count cannot be negative.", nameof(likeCount));

        if (commentCount < 0)
            throw new ArgumentException("Comment count cannot be negative.", nameof(commentCount));

        if (shareCount < 0)
            throw new ArgumentException("Share count cannot be negative.", nameof(shareCount));

        ViewCount = viewCount;
        LikeCount = likeCount;
        CommentCount = commentCount;
        ShareCount = shareCount;
        EngagementRate = CalculateEngagementRate(viewCount, likeCount, commentCount, shareCount);
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the content metadata.
    /// </summary>
    public void UpdateMetadata(string title, string? thumbnailUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Title = title.Trim();
        ThumbnailUrl = thumbnailUrl?.Trim();
        LastUpdatedAt = DateTime.UtcNow;
    }

    private static double CalculateEngagementRate(long views, long likes, long comments, long shares)
    {
        if (views == 0)
            return 0;

        return (double)(likes + comments + shares) / views * 100;
    }
}
