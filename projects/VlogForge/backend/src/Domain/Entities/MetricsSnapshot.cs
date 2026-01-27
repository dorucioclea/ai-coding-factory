using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Entity representing a daily snapshot of metrics for historical trend analysis.
/// One snapshot per user per platform per day.
/// Story: ACF-004
/// </summary>
public sealed class MetricsSnapshot : Entity
{
    /// <summary>
    /// Number of days used for estimating daily values from totals.
    /// </summary>
    private const int EstimationPeriodDays = 30;

    /// <summary>
    /// Gets the user ID this snapshot belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the platform type.
    /// </summary>
    public PlatformType PlatformType { get; private set; }

    /// <summary>
    /// Gets the date of this snapshot (date only, no time component).
    /// </summary>
    public DateTime SnapshotDate { get; private set; }

    /// <summary>
    /// Gets the follower count at time of snapshot.
    /// </summary>
    public long FollowerCount { get; private set; }

    /// <summary>
    /// Gets the views gained during this day.
    /// </summary>
    public long DailyViews { get; private set; }

    /// <summary>
    /// Gets the likes gained during this day.
    /// </summary>
    public long DailyLikes { get; private set; }

    /// <summary>
    /// Gets the comments gained during this day.
    /// </summary>
    public long DailyComments { get; private set; }

    /// <summary>
    /// Gets the engagement rate for this day.
    /// </summary>
    public double EngagementRate { get; private set; }

    private MetricsSnapshot() : base()
    {
    }

    private MetricsSnapshot(
        Guid userId,
        PlatformType platformType,
        DateTime snapshotDate,
        long followerCount,
        long dailyViews,
        long dailyLikes,
        long dailyComments,
        double engagementRate) : base()
    {
        UserId = userId;
        PlatformType = platformType;
        SnapshotDate = snapshotDate.Date; // Ensure date only
        FollowerCount = followerCount;
        DailyViews = dailyViews;
        DailyLikes = dailyLikes;
        DailyComments = dailyComments;
        EngagementRate = engagementRate;
    }

    /// <summary>
    /// Creates a new metrics snapshot.
    /// </summary>
    public static MetricsSnapshot Create(
        Guid userId,
        PlatformType platformType,
        DateTime snapshotDate,
        long followerCount,
        long dailyViews,
        long dailyLikes,
        long dailyComments,
        double engagementRate)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (followerCount < 0)
            throw new ArgumentException("Follower count cannot be negative.", nameof(followerCount));

        if (dailyViews < 0)
            throw new ArgumentException("Daily views cannot be negative.", nameof(dailyViews));

        if (dailyLikes < 0)
            throw new ArgumentException("Daily likes cannot be negative.", nameof(dailyLikes));

        if (dailyComments < 0)
            throw new ArgumentException("Daily comments cannot be negative.", nameof(dailyComments));

        if (engagementRate < 0)
            throw new ArgumentException("Engagement rate cannot be negative.", nameof(engagementRate));

        return new MetricsSnapshot(
            userId,
            platformType,
            snapshotDate,
            followerCount,
            dailyViews,
            dailyLikes,
            dailyComments,
            engagementRate);
    }

    /// <summary>
    /// Creates a snapshot from current platform metrics with explicit daily values.
    /// </summary>
    public static MetricsSnapshot CreateFromMetrics(
        Guid userId,
        PlatformMetrics metrics,
        DateTime snapshotDate,
        long dailyViews,
        long dailyLikes,
        long dailyComments)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return Create(
            userId,
            metrics.PlatformType,
            snapshotDate,
            metrics.FollowerCount,
            dailyViews,
            dailyLikes,
            dailyComments,
            metrics.EngagementRate);
    }

    /// <summary>
    /// Creates a snapshot from current platform metrics.
    /// Uses the metrics' current totals as daily values (for initial snapshot).
    /// </summary>
    public static MetricsSnapshot Create(
        Guid userId,
        PlatformType platformType,
        DateTime snapshotDate,
        PlatformMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        // For simplicity, use estimated daily values based on totals
        // In production, this would track deltas from previous snapshot
        var estimatedDailyViews = metrics.TotalViews / EstimationPeriodDays;
        var estimatedDailyLikes = metrics.TotalLikes / EstimationPeriodDays;
        var estimatedDailyComments = metrics.TotalComments / EstimationPeriodDays;

        return Create(
            userId,
            platformType,
            snapshotDate,
            metrics.FollowerCount,
            estimatedDailyViews,
            estimatedDailyLikes,
            estimatedDailyComments,
            metrics.EngagementRate);
    }
}
