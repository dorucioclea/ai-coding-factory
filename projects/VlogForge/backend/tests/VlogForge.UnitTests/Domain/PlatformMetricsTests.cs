using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for PlatformMetrics entity.
/// Story: ACF-004
/// </summary>
[Trait("Story", "ACF-004")]
public class PlatformMetricsTests
{
    private readonly Guid _validConnectionId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldSucceed()
    {
        // Arrange & Act
        var metrics = PlatformMetrics.Create(
            _validConnectionId,
            PlatformType.YouTube,
            followerCount: 10000,
            totalViews: 500000,
            totalLikes: 25000,
            totalComments: 5000,
            totalShares: 1000);

        // Assert
        metrics.Should().NotBeNull();
        metrics.PlatformConnectionId.Should().Be(_validConnectionId);
        metrics.PlatformType.Should().Be(PlatformType.YouTube);
        metrics.FollowerCount.Should().Be(10000);
        metrics.TotalViews.Should().Be(500000);
        metrics.TotalLikes.Should().Be(25000);
        metrics.TotalComments.Should().Be(5000);
        metrics.TotalShares.Should().Be(1000);
    }

    [Fact]
    public void CreateShouldCalculateEngagementRate()
    {
        // Arrange & Act
        var metrics = PlatformMetrics.Create(
            _validConnectionId,
            PlatformType.YouTube,
            followerCount: 10000,
            totalViews: 100000,
            totalLikes: 5000,
            totalComments: 2000,
            totalShares: 1000);

        // Assert: (5000 + 2000 + 1000) / 100000 * 100 = 8%
        metrics.EngagementRate.Should().Be(8.0);
    }

    [Fact]
    public void CreateWithZeroViewsShouldHaveZeroEngagementRate()
    {
        // Arrange & Act
        var metrics = PlatformMetrics.Create(
            _validConnectionId,
            PlatformType.YouTube,
            followerCount: 10000,
            totalViews: 0,
            totalLikes: 0,
            totalComments: 0,
            totalShares: 0);

        // Assert
        metrics.EngagementRate.Should().Be(0);
    }

    [Fact]
    public void CreateWithEmptyConnectionIdShouldThrow()
    {
        // Act
        var act = () => PlatformMetrics.Create(
            Guid.Empty,
            PlatformType.YouTube,
            10000, 500000, 25000, 5000, 1000);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Platform connection ID cannot be empty.*");
    }

    [Fact]
    public void CreateWithNegativeFollowerCountShouldThrow()
    {
        // Act
        var act = () => PlatformMetrics.Create(
            _validConnectionId,
            PlatformType.YouTube,
            followerCount: -1,
            totalViews: 500000,
            totalLikes: 25000,
            totalComments: 5000,
            totalShares: 1000);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Follower count cannot be negative.*");
    }

    [Fact]
    public void CreateWithNegativeViewsShouldThrow()
    {
        // Act
        var act = () => PlatformMetrics.Create(
            _validConnectionId,
            PlatformType.YouTube,
            followerCount: 10000,
            totalViews: -1,
            totalLikes: 25000,
            totalComments: 5000,
            totalShares: 1000);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Total views cannot be negative.*");
    }

    #endregion

    #region UpdateMetrics Tests

    [Fact]
    public void UpdateMetricsShouldUpdateAllValues()
    {
        // Arrange
        var metrics = CreateValidMetrics();

        // Act
        metrics.UpdateMetrics(
            followerCount: 20000,
            totalViews: 1000000,
            totalLikes: 50000,
            totalComments: 10000,
            totalShares: 2000);

        // Assert
        metrics.FollowerCount.Should().Be(20000);
        metrics.TotalViews.Should().Be(1000000);
        metrics.TotalLikes.Should().Be(50000);
        metrics.TotalComments.Should().Be(10000);
        metrics.TotalShares.Should().Be(2000);
    }

    [Fact]
    public void UpdateMetricsShouldRecalculateEngagementRate()
    {
        // Arrange
        var metrics = CreateValidMetrics();

        // Act
        metrics.UpdateMetrics(
            followerCount: 10000,
            totalViews: 200000,
            totalLikes: 10000,
            totalComments: 4000,
            totalShares: 2000);

        // Assert: (10000 + 4000 + 2000) / 200000 * 100 = 8%
        metrics.EngagementRate.Should().Be(8.0);
    }

    [Fact]
    public void UpdateMetricsShouldUpdateTimestamp()
    {
        // Arrange
        var metrics = CreateValidMetrics();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        metrics.UpdateMetrics(20000, 1000000, 50000, 10000, 2000);

        // Assert
        metrics.MetricsUpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void UpdateMetricsShouldRaiseMetricsSyncedEvent()
    {
        // Arrange
        var metrics = CreateValidMetrics();
        metrics.ClearDomainEvents();

        // Act
        metrics.UpdateMetrics(20000, 1000000, 50000, 10000, 2000);

        // Assert
        metrics.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MetricsSyncedEvent>();
    }

    #endregion

    #region Helper Methods

    private PlatformMetrics CreateValidMetrics()
    {
        return PlatformMetrics.Create(
            _validConnectionId,
            PlatformType.YouTube,
            followerCount: 10000,
            totalViews: 500000,
            totalLikes: 25000,
            totalComments: 5000,
            totalShares: 1000);
    }

    #endregion
}
