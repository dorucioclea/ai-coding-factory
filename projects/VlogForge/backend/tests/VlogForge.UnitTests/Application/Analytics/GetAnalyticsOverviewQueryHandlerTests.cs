using FluentAssertions;
using Moq;
using VlogForge.Application.Analytics.Queries.GetOverview;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Analytics;

/// <summary>
/// Unit tests for GetAnalyticsOverviewQueryHandler.
/// Story: ACF-004 (AC1, AC3, AC5)
/// </summary>
[Trait("Story", "ACF-004")]
public class GetAnalyticsOverviewQueryHandlerTests
{
    private readonly Mock<IPlatformMetricsRepository> _metricsRepositoryMock;
    private readonly Mock<IMetricsSnapshotRepository> _snapshotRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetAnalyticsOverviewQueryHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public GetAnalyticsOverviewQueryHandlerTests()
    {
        _metricsRepositoryMock = new Mock<IPlatformMetricsRepository>();
        _snapshotRepositoryMock = new Mock<IMetricsSnapshotRepository>();
        _cacheServiceMock = new Mock<ICacheService>();

        _handler = new GetAnalyticsOverviewQueryHandler(
            _metricsRepositoryMock.Object,
            _snapshotRepositoryMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithCachedDataShouldReturnFromCache()
    {
        // Arrange
        var query = new GetAnalyticsOverviewQuery(_testUserId);
        var cachedResponse = CreateSampleOverviewResponse();

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedResponse);
        _metricsRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleWithNoMetricsShouldReturnZeroTotals()
    {
        // Arrange
        var query = new GetAnalyticsOverviewQuery(_testUserId);

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse?)null);

        _metricsRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformMetrics>());

        _snapshotRepositoryMock
            .Setup(x => x.GetSnapshotsAsync(
                _testUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<PlatformType?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetricsSnapshot>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalFollowers.Should().Be(0);
        result.TotalViews.Should().Be(0);
        result.AverageEngagementRate.Should().Be(0);
        result.PlatformBreakdown.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleShouldAggregateTotalsAcrossPlatforms()
    {
        // Arrange
        var query = new GetAnalyticsOverviewQuery(_testUserId);
        var metrics = CreateMultiplePlatformMetrics();

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse?)null);

        _metricsRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metrics);

        _snapshotRepositoryMock
            .Setup(x => x.GetSnapshotsAsync(
                _testUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<PlatformType?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetricsSnapshot>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalFollowers.Should().Be(60000); // 10000 + 50000
        result.TotalViews.Should().Be(1500000); // 500000 + 1000000
        result.PlatformBreakdown.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleShouldCacheResult()
    {
        // Arrange
        var query = new GetAnalyticsOverviewQuery(_testUserId);

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse?)null);

        _metricsRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformMetrics>());

        _snapshotRepositoryMock
            .Setup(x => x.GetSnapshotsAsync(
                _testUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<PlatformType?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetricsSnapshot>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #region Helper Methods

    private static VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse CreateSampleOverviewResponse()
    {
        return new VlogForge.Application.Analytics.DTOs.AnalyticsOverviewResponse(
            TotalFollowers: 50000,
            TotalViews: 1000000,
            AverageEngagementRate: 5.5,
            PlatformBreakdown: [],
            Growth: new VlogForge.Application.Analytics.DTOs.GrowthIndicators(
                FollowerGrowthPercent: 2.5,
                ViewsGrowthPercent: 5.0,
                EngagementGrowthPercent: 1.0,
                ComparisonDays: 7));
    }

    private static List<PlatformMetrics> CreateMultiplePlatformMetrics()
    {
        var connectionId1 = Guid.NewGuid();
        var connectionId2 = Guid.NewGuid();

        return
        [
            PlatformMetrics.Create(
                connectionId1,
                PlatformType.YouTube,
                followerCount: 10000,
                totalViews: 500000,
                totalLikes: 25000,
                totalComments: 5000,
                totalShares: 1000),
            PlatformMetrics.Create(
                connectionId2,
                PlatformType.Instagram,
                followerCount: 50000,
                totalViews: 1000000,
                totalLikes: 100000,
                totalComments: 20000,
                totalShares: 5000)
        ];
    }

    #endregion
}
