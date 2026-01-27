using MediatR;
using VlogForge.Application.Analytics.DTOs;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Analytics.Queries.GetOverview;

/// <summary>
/// Handler for GetAnalyticsOverviewQuery.
/// Story: ACF-004
/// </summary>
public sealed class GetAnalyticsOverviewQueryHandler
    : IRequestHandler<GetAnalyticsOverviewQuery, AnalyticsOverviewResponse>
{
    private const string CacheKeyPrefix = "analytics:overview:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);
    private const int ComparisonDays = 7;

    private readonly IPlatformMetricsRepository _metricsRepository;
    private readonly IMetricsSnapshotRepository _snapshotRepository;
    private readonly ICacheService _cacheService;

    public GetAnalyticsOverviewQueryHandler(
        IPlatformMetricsRepository metricsRepository,
        IMetricsSnapshotRepository snapshotRepository,
        ICacheService cacheService)
    {
        _metricsRepository = metricsRepository;
        _snapshotRepository = snapshotRepository;
        _cacheService = cacheService;
    }

    public async Task<AnalyticsOverviewResponse> Handle(
        GetAnalyticsOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{request.UserId}";

        // Try to get from cache first
        var cached = await _cacheService.GetAsync<AnalyticsOverviewResponse>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Get current metrics for all connected platforms
        var currentMetrics = await _metricsRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        // Calculate totals
        var totalFollowers = currentMetrics.Sum(m => m.FollowerCount);
        var totalViews = currentMetrics.Sum(m => m.TotalViews);
        var averageEngagementRate = currentMetrics.Count > 0
            ? currentMetrics.Average(m => m.EngagementRate)
            : 0;

        // Build platform breakdown
        var platformBreakdown = currentMetrics
            .Select(m => new PlatformMetricsSummary(
                m.PlatformType.ToString(),
                m.FollowerCount,
                m.TotalViews,
                m.EngagementRate,
                m.MetricsUpdatedAt))
            .ToList();

        // Calculate growth indicators
        var growth = await CalculateGrowthAsync(request.UserId, currentMetrics, cancellationToken);

        var response = new AnalyticsOverviewResponse(
            totalFollowers,
            totalViews,
            Math.Round(averageEngagementRate, 2),
            platformBreakdown,
            growth);

        // Cache the response
        await _cacheService.SetAsync(cacheKey, response, CacheTtl, cancellationToken);

        return response;
    }

    private async Task<GrowthIndicators> CalculateGrowthAsync(
        Guid userId,
        IReadOnlyList<Domain.Entities.PlatformMetrics> currentMetrics,
        CancellationToken cancellationToken)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-ComparisonDays);
        var previousStartDate = startDate.AddDays(-ComparisonDays);

        // Get snapshots for current and previous periods
        var allSnapshots = await _snapshotRepository.GetSnapshotsAsync(
            userId,
            previousStartDate,
            endDate,
            cancellationToken: cancellationToken);

        var currentPeriodSnapshots = allSnapshots
            .Where(s => s.SnapshotDate >= startDate && s.SnapshotDate <= endDate)
            .ToList();

        var previousPeriodSnapshots = allSnapshots
            .Where(s => s.SnapshotDate >= previousStartDate && s.SnapshotDate < startDate)
            .ToList();

        // Current period totals
        var currentFollowers = currentMetrics.Sum(m => m.FollowerCount);
        var currentViews = currentPeriodSnapshots.Sum(s => s.DailyViews);
        var currentEngagement = currentMetrics.Count > 0
            ? currentMetrics.Average(m => m.EngagementRate)
            : 0;

        // Previous period totals (use first day of period as baseline)
        var previousFollowers = previousPeriodSnapshots
            .GroupBy(s => s.PlatformType)
            .Sum(g => g.OrderBy(s => s.SnapshotDate).First().FollowerCount);
        var previousViews = previousPeriodSnapshots.Sum(s => s.DailyViews);
        var previousEngagement = previousPeriodSnapshots.Count > 0
            ? previousPeriodSnapshots.Average(s => s.EngagementRate)
            : 0;

        return new GrowthIndicators(
            CalculatePercentChange(previousFollowers, currentFollowers),
            CalculatePercentChange(previousViews, currentViews),
            CalculatePercentChange(previousEngagement, currentEngagement),
            ComparisonDays);
    }

    private static double CalculatePercentChange(double previous, double current)
    {
        if (previous == 0)
        {
            return current > 0 ? 100 : 0;
        }

        return Math.Round((current - previous) / previous * 100, 2);
    }

    private static double CalculatePercentChange(long previous, long current)
    {
        return CalculatePercentChange((double)previous, current);
    }
}
