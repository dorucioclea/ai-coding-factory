using FluentAssertions;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for MetricsSnapshot entity.
/// Story: ACF-004
/// </summary>
[Trait("Story", "ACF-004")]
public class MetricsSnapshotTests
{
    private readonly Guid _validUserId = Guid.NewGuid();
    private readonly DateTime _validSnapshotDate = DateTime.UtcNow.Date;

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldSucceed()
    {
        // Arrange & Act
        var snapshot = MetricsSnapshot.Create(
            _validUserId,
            PlatformType.YouTube,
            _validSnapshotDate,
            followerCount: 10000,
            dailyViews: 5000,
            dailyLikes: 250,
            dailyComments: 50,
            engagementRate: 6.0);

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.UserId.Should().Be(_validUserId);
        snapshot.PlatformType.Should().Be(PlatformType.YouTube);
        snapshot.SnapshotDate.Should().Be(_validSnapshotDate);
        snapshot.FollowerCount.Should().Be(10000);
        snapshot.DailyViews.Should().Be(5000);
        snapshot.DailyLikes.Should().Be(250);
        snapshot.DailyComments.Should().Be(50);
        snapshot.EngagementRate.Should().Be(6.0);
    }

    [Fact]
    public void CreateShouldNormalizeDateToDateOnly()
    {
        // Arrange
        var dateTimeWithTime = new DateTime(2024, 3, 15, 14, 30, 45, DateTimeKind.Utc);

        // Act
        var snapshot = MetricsSnapshot.Create(
            _validUserId,
            PlatformType.YouTube,
            dateTimeWithTime,
            10000, 5000, 250, 50, 6.0);

        // Assert
        snapshot.SnapshotDate.Should().Be(new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void CreateWithEmptyUserIdShouldThrow()
    {
        // Act
        var act = () => MetricsSnapshot.Create(
            Guid.Empty,
            PlatformType.YouTube,
            _validSnapshotDate,
            10000, 5000, 250, 50, 6.0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty.*");
    }

    [Fact]
    public void CreateWithNegativeFollowerCountShouldThrow()
    {
        // Act
        var act = () => MetricsSnapshot.Create(
            _validUserId,
            PlatformType.YouTube,
            _validSnapshotDate,
            followerCount: -1,
            dailyViews: 5000,
            dailyLikes: 250,
            dailyComments: 50,
            engagementRate: 6.0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Follower count cannot be negative.*");
    }

    [Fact]
    public void CreateWithNegativeEngagementRateShouldThrow()
    {
        // Act
        var act = () => MetricsSnapshot.Create(
            _validUserId,
            PlatformType.YouTube,
            _validSnapshotDate,
            followerCount: 10000,
            dailyViews: 5000,
            dailyLikes: 250,
            dailyComments: 50,
            engagementRate: -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Engagement rate cannot be negative.*");
    }

    #endregion

    #region CreateFromMetrics Tests

    [Fact]
    public void CreateFromMetricsShouldUseMetricsValues()
    {
        // Arrange
        var connectionId = Guid.NewGuid();
        var metrics = PlatformMetrics.Create(
            connectionId,
            PlatformType.Instagram,
            followerCount: 50000,
            totalViews: 1000000,
            totalLikes: 50000,
            totalComments: 10000,
            totalShares: 5000);

        // Act
        var snapshot = MetricsSnapshot.Create(
            _validUserId,
            PlatformType.Instagram,
            _validSnapshotDate,
            metrics);

        // Assert
        snapshot.UserId.Should().Be(_validUserId);
        snapshot.PlatformType.Should().Be(PlatformType.Instagram);
        snapshot.FollowerCount.Should().Be(50000);
        snapshot.EngagementRate.Should().Be(metrics.EngagementRate);
    }

    [Fact]
    public void CreateFromMetricsWithNullShouldThrow()
    {
        // Act
        var act = () => MetricsSnapshot.Create(
            _validUserId,
            PlatformType.YouTube,
            _validSnapshotDate,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region CreateFromMetrics With Daily Values Tests

    [Fact]
    public void CreateFromMetricsWithDailyValuesShouldWork()
    {
        // Arrange
        var connectionId = Guid.NewGuid();
        var metrics = PlatformMetrics.Create(
            connectionId,
            PlatformType.TikTok,
            followerCount: 100000,
            totalViews: 5000000,
            totalLikes: 250000,
            totalComments: 50000,
            totalShares: 25000);

        // Act
        var snapshot = MetricsSnapshot.CreateFromMetrics(
            _validUserId,
            metrics,
            _validSnapshotDate,
            dailyViews: 50000,
            dailyLikes: 2500,
            dailyComments: 500);

        // Assert
        snapshot.FollowerCount.Should().Be(100000);
        snapshot.DailyViews.Should().Be(50000);
        snapshot.DailyLikes.Should().Be(2500);
        snapshot.DailyComments.Should().Be(500);
    }

    #endregion
}
