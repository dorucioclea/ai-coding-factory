using MediatR;
using VlogForge.Application.Analytics.DTOs;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Analytics.Queries.GetTrends;

/// <summary>
/// Handler for GetAnalyticsTrendsQuery.
/// Story: ACF-004
/// </summary>
public sealed class GetAnalyticsTrendsQueryHandler
    : IRequestHandler<GetAnalyticsTrendsQuery, AnalyticsTrendsResponse>
{
    private const string CacheVersion = "v1";
    private const string CacheKeyPrefix = $"analytics:trends:{CacheVersion}:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private readonly IMetricsSnapshotRepository _snapshotRepository;
    private readonly ICacheService _cacheService;

    public GetAnalyticsTrendsQueryHandler(
        IMetricsSnapshotRepository snapshotRepository,
        ICacheService cacheService)
    {
        _snapshotRepository = snapshotRepository;
        _cacheService = cacheService;
    }

    public async Task<AnalyticsTrendsResponse> Handle(
        GetAnalyticsTrendsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{request.UserId}:{request.Period}";

        // Try to get from cache first
        var cached = await _cacheService.GetAsync<AnalyticsTrendsResponse>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var days = ParsePeriodToDays(request.Period);
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-days);

        // Get all snapshots for the period
        var snapshots = await _snapshotRepository.GetSnapshotsAsync(
            request.UserId,
            startDate,
            endDate,
            cancellationToken: cancellationToken);

        // Aggregate by date (combine all platforms)
        var followerTrend = BuildFollowerTrend(snapshots, startDate, days);
        var viewsTrend = BuildViewsTrend(snapshots, startDate, days);
        var engagementTrend = BuildEngagementTrend(snapshots, startDate, days);

        var response = new AnalyticsTrendsResponse(
            request.Period,
            followerTrend,
            viewsTrend,
            engagementTrend);

        // Cache the response
        await _cacheService.SetAsync(cacheKey, response, CacheTtl, cancellationToken);

        return response;
    }

    private static int ParsePeriodToDays(string period)
    {
        return period.ToLowerInvariant() switch
        {
            "7d" => 7,
            "30d" => 30,
            "90d" => 90,
            _ => 7 // Default to 7 days
        };
    }

    private static List<TrendDataPoint> BuildFollowerTrend(
        IReadOnlyList<MetricsSnapshot> snapshots,
        DateTime startDate,
        int days)
    {
        var result = new List<TrendDataPoint>();

        for (var i = 0; i <= days; i++)
        {
            var date = startDate.AddDays(i);
            var daySnapshots = snapshots.Where(s => s.SnapshotDate == date).ToList();

            // Sum followers across all platforms for this day
            var totalFollowers = daySnapshots.Sum(s => s.FollowerCount);

            result.Add(new TrendDataPoint(date, totalFollowers));
        }

        return result;
    }

    private static List<TrendDataPoint> BuildViewsTrend(
        IReadOnlyList<MetricsSnapshot> snapshots,
        DateTime startDate,
        int days)
    {
        var result = new List<TrendDataPoint>();

        for (var i = 0; i <= days; i++)
        {
            var date = startDate.AddDays(i);
            var daySnapshots = snapshots.Where(s => s.SnapshotDate == date).ToList();

            // Sum daily views across all platforms
            var totalViews = daySnapshots.Sum(s => s.DailyViews);

            result.Add(new TrendDataPoint(date, totalViews));
        }

        return result;
    }

    private static List<TrendDataPoint> BuildEngagementTrend(
        IReadOnlyList<MetricsSnapshot> snapshots,
        DateTime startDate,
        int days)
    {
        var result = new List<TrendDataPoint>();

        for (var i = 0; i <= days; i++)
        {
            var date = startDate.AddDays(i);
            var daySnapshots = snapshots.Where(s => s.SnapshotDate == date).ToList();

            // Average engagement rate across platforms
            var avgEngagement = daySnapshots.Count > 0
                ? daySnapshots.Average(s => s.EngagementRate)
                : 0;

            // Convert to long for consistent TrendDataPoint (multiply by 100 for precision)
            result.Add(new TrendDataPoint(date, (long)(avgEngagement * 100)));
        }

        return result;
    }
}
