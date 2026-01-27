using FluentAssertions;
using Moq;
using VlogForge.Application.Analytics.Queries.GetTrends;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Analytics;

/// <summary>
/// Unit tests for GetAnalyticsTrendsQueryHandler.
/// Story: ACF-004 (AC2)
/// </summary>
[Trait("Story", "ACF-004")]
public class GetAnalyticsTrendsQueryHandlerTests
{
    private readonly Mock<IMetricsSnapshotRepository> _snapshotRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetAnalyticsTrendsQueryHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public GetAnalyticsTrendsQueryHandlerTests()
    {
        _snapshotRepositoryMock = new Mock<IMetricsSnapshotRepository>();
        _cacheServiceMock = new Mock<ICacheService>();

        _handler = new GetAnalyticsTrendsQueryHandler(
            _snapshotRepositoryMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithCachedDataShouldReturnFromCache()
    {
        // Arrange
        var query = new GetAnalyticsTrendsQuery(_testUserId, "7d");
        var cachedResponse = CreateSampleTrendsResponse();

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedResponse);
        _snapshotRepositoryMock.Verify(
            x => x.GetSnapshotsAsync(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<PlatformType?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleWithNoSnapshotsShouldReturnEmptyTrends()
    {
        // Arrange
        var query = new GetAnalyticsTrendsQuery(_testUserId, "7d");

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse?)null);

        _snapshotRepositoryMock
            .Setup(x => x.GetSnapshotsAsync(
                _testUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<PlatformType?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetricsSnapshot>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Period.Should().Be("7d");
        result.FollowerTrend.Should().HaveCount(8); // 7 days + 1
        result.ViewsTrend.Should().HaveCount(8);
        result.EngagementTrend.Should().HaveCount(8);
    }

    [Theory]
    [InlineData("7d", 8)]
    [InlineData("30d", 31)]
    [InlineData("90d", 91)]
    public async Task HandleShouldReturnCorrectNumberOfDataPoints(string period, int expectedCount)
    {
        // Arrange
        var query = new GetAnalyticsTrendsQuery(_testUserId, period);

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse?)null);

        _snapshotRepositoryMock
            .Setup(x => x.GetSnapshotsAsync(
                _testUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<PlatformType?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetricsSnapshot>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Period.Should().Be(period);
        result.FollowerTrend.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task HandleShouldCacheResult()
    {
        // Arrange
        var query = new GetAnalyticsTrendsQuery(_testUserId, "7d");

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse?)null);

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
                It.IsAny<VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldAggregateSnapshotsByDate()
    {
        // Arrange
        var query = new GetAnalyticsTrendsQuery(_testUserId, "7d");
        var snapshots = CreateSampleSnapshots();

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse?)null);

        _snapshotRepositoryMock
            .Setup(x => x.GetSnapshotsAsync(
                _testUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<PlatformType?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshots);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FollowerTrend.Should().NotBeEmpty();
        result.ViewsTrend.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("1d")]
    [InlineData("365d")]
    public async Task HandleWithInvalidPeriodShouldDefaultTo7Days(string period)
    {
        // Arrange
        var query = new GetAnalyticsTrendsQuery(_testUserId, period);

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse?)null);

        _snapshotRepositoryMock
            .Setup(x => x.GetSnapshotsAsync(
                _testUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<PlatformType?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetricsSnapshot>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Handler defaults invalid periods to 7 days (8 data points)
        result.FollowerTrend.Should().HaveCount(8);
    }

    #region Helper Methods

    private static VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse CreateSampleTrendsResponse()
    {
        return new VlogForge.Application.Analytics.DTOs.AnalyticsTrendsResponse(
            Period: "7d",
            FollowerTrend: [],
            ViewsTrend: [],
            EngagementTrend: []);
    }

    private List<MetricsSnapshot> CreateSampleSnapshots()
    {
        var today = DateTime.UtcNow.Date;
        var snapshots = new List<MetricsSnapshot>();

        // Create snapshots for the past 7 days with two platforms
        for (var i = 0; i < 7; i++)
        {
            var date = today.AddDays(-i);

            snapshots.Add(MetricsSnapshot.Create(
                _testUserId,
                PlatformType.YouTube,
                date,
                followerCount: 10000 + (i * 100),
                dailyViews: 5000 + (i * 50),
                dailyLikes: 500 + (i * 10),
                dailyComments: 100 + (i * 2),
                engagementRate: 5.0 + (i * 0.1)));

            snapshots.Add(MetricsSnapshot.Create(
                _testUserId,
                PlatformType.Instagram,
                date,
                followerCount: 20000 + (i * 200),
                dailyViews: 10000 + (i * 100),
                dailyLikes: 1000 + (i * 20),
                dailyComments: 200 + (i * 5),
                engagementRate: 6.0 + (i * 0.2)));
        }

        return snapshots;
    }

    #endregion
}
