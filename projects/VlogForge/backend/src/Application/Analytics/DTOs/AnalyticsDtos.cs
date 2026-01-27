namespace VlogForge.Application.Analytics.DTOs;

/// <summary>
/// Response for analytics dashboard overview.
/// Story: ACF-004
/// </summary>
public sealed record AnalyticsOverviewResponse(
    long TotalFollowers,
    long TotalViews,
    double AverageEngagementRate,
    IReadOnlyList<PlatformMetricsSummary> PlatformBreakdown,
    GrowthIndicators Growth);

/// <summary>
/// Summary of metrics for a single platform.
/// </summary>
public sealed record PlatformMetricsSummary(
    string PlatformType,
    long FollowerCount,
    long ViewCount,
    double EngagementRate,
    DateTime? LastSyncAt);

/// <summary>
/// Growth indicators comparing current period to previous period.
/// </summary>
public sealed record GrowthIndicators(
    double FollowerGrowthPercent,
    double ViewsGrowthPercent,
    double EngagementGrowthPercent,
    int ComparisonDays);

/// <summary>
/// Response for analytics trends data.
/// Story: ACF-004
/// </summary>
public sealed record AnalyticsTrendsResponse(
    string Period,
    IReadOnlyList<TrendDataPoint> FollowerTrend,
    IReadOnlyList<TrendDataPoint> ViewsTrend,
    IReadOnlyList<TrendDataPoint> EngagementTrend);

/// <summary>
/// A single data point in a trend chart.
/// </summary>
public sealed record TrendDataPoint(
    DateTime Date,
    long Value,
    string? PlatformType = null);

/// <summary>
/// Response for top content list.
/// Story: ACF-004
/// </summary>
public sealed record TopContentResponse(
    IReadOnlyList<ContentPerformanceDto> Content,
    string SortedBy);

/// <summary>
/// DTO for content performance.
/// </summary>
public sealed record ContentPerformanceDto(
    string ContentId,
    string PlatformType,
    string Title,
    string? ThumbnailUrl,
    string ContentUrl,
    DateTime PublishedAt,
    long ViewCount,
    long LikeCount,
    long CommentCount,
    double EngagementRate);
