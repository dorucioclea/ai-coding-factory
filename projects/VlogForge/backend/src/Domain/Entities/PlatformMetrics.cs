using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Entity representing current aggregated metrics for a connected platform.
/// Updated periodically via background sync job.
/// Story: ACF-004
/// </summary>
public sealed class PlatformMetrics : Entity
{
    /// <summary>
    /// Gets the platform connection ID this metrics belongs to.
    /// </summary>
    public Guid PlatformConnectionId { get; private set; }

    /// <summary>
    /// Gets the platform type for quick filtering.
    /// </summary>
    public PlatformType PlatformType { get; private set; }

    /// <summary>
    /// Gets the current follower count.
    /// </summary>
    public long FollowerCount { get; private set; }

    /// <summary>
    /// Gets the total view count across all content.
    /// </summary>
    public long TotalViews { get; private set; }

    /// <summary>
    /// Gets the total like count across all content.
    /// </summary>
    public long TotalLikes { get; private set; }

    /// <summary>
    /// Gets the total comment count across all content.
    /// </summary>
    public long TotalComments { get; private set; }

    /// <summary>
    /// Gets the total share count across all content.
    /// </summary>
    public long TotalShares { get; private set; }

    /// <summary>
    /// Gets the engagement rate: (likes + comments + shares) / views * 100.
    /// </summary>
    public double EngagementRate { get; private set; }

    /// <summary>
    /// Gets when these metrics were last updated from the platform API.
    /// </summary>
    public DateTime MetricsUpdatedAt { get; private set; }

    private PlatformMetrics() : base()
    {
    }

    private PlatformMetrics(
        Guid platformConnectionId,
        PlatformType platformType,
        long followerCount,
        long totalViews,
        long totalLikes,
        long totalComments,
        long totalShares) : base()
    {
        PlatformConnectionId = platformConnectionId;
        PlatformType = platformType;
        FollowerCount = followerCount;
        TotalViews = totalViews;
        TotalLikes = totalLikes;
        TotalComments = totalComments;
        TotalShares = totalShares;
        EngagementRate = CalculateEngagementRate(totalViews, totalLikes, totalComments, totalShares);
        MetricsUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new platform metrics record.
    /// </summary>
    public static PlatformMetrics Create(
        Guid platformConnectionId,
        PlatformType platformType,
        long followerCount,
        long totalViews,
        long totalLikes,
        long totalComments,
        long totalShares)
    {
        if (platformConnectionId == Guid.Empty)
            throw new ArgumentException("Platform connection ID cannot be empty.", nameof(platformConnectionId));

        if (followerCount < 0)
            throw new ArgumentException("Follower count cannot be negative.", nameof(followerCount));

        if (totalViews < 0)
            throw new ArgumentException("Total views cannot be negative.", nameof(totalViews));

        if (totalLikes < 0)
            throw new ArgumentException("Total likes cannot be negative.", nameof(totalLikes));

        if (totalComments < 0)
            throw new ArgumentException("Total comments cannot be negative.", nameof(totalComments));

        if (totalShares < 0)
            throw new ArgumentException("Total shares cannot be negative.", nameof(totalShares));

        return new PlatformMetrics(
            platformConnectionId,
            platformType,
            followerCount,
            totalViews,
            totalLikes,
            totalComments,
            totalShares);
    }

    /// <summary>
    /// Updates the metrics with new data from the platform API.
    /// </summary>
    public void UpdateMetrics(
        long followerCount,
        long totalViews,
        long totalLikes,
        long totalComments,
        long totalShares)
    {
        if (followerCount < 0)
            throw new ArgumentException("Follower count cannot be negative.", nameof(followerCount));

        if (totalViews < 0)
            throw new ArgumentException("Total views cannot be negative.", nameof(totalViews));

        if (totalLikes < 0)
            throw new ArgumentException("Total likes cannot be negative.", nameof(totalLikes));

        if (totalComments < 0)
            throw new ArgumentException("Total comments cannot be negative.", nameof(totalComments));

        if (totalShares < 0)
            throw new ArgumentException("Total shares cannot be negative.", nameof(totalShares));

        FollowerCount = followerCount;
        TotalViews = totalViews;
        TotalLikes = totalLikes;
        TotalComments = totalComments;
        TotalShares = totalShares;
        EngagementRate = CalculateEngagementRate(totalViews, totalLikes, totalComments, totalShares);
        MetricsUpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new MetricsSyncedEvent(Id, PlatformConnectionId, PlatformType, MetricsUpdatedAt));
    }

    private static double CalculateEngagementRate(long views, long likes, long comments, long shares)
    {
        if (views == 0)
            return 0;

        // Explicitly convert each value to double to prevent integer overflow
        var totalEngagements = (double)likes + (double)comments + (double)shares;
        return totalEngagements / views * 100;
    }
}
