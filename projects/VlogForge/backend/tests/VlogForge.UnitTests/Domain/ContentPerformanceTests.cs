using FluentAssertions;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for ContentPerformance entity.
/// Story: ACF-004
/// </summary>
[Trait("Story", "ACF-004")]
public class ContentPerformanceTests
{
    private readonly Guid _validConnectionId = Guid.NewGuid();
    private const string ValidContentId = "yt_video_123456";
    private const string ValidTitle = "My Awesome Video";
    private const string ValidThumbnailUrl = "https://img.youtube.com/vi/123456/maxresdefault.jpg";
    private const string ValidContentUrl = "https://www.youtube.com/watch?v=123456";
    private readonly DateTime _validPublishedAt = DateTime.UtcNow.AddDays(-7);

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldSucceed()
    {
        // Arrange & Act
        var content = ContentPerformance.Create(
            _validConnectionId,
            PlatformType.YouTube,
            ValidContentId,
            ValidTitle,
            ValidThumbnailUrl,
            ValidContentUrl,
            _validPublishedAt,
            viewCount: 100000,
            likeCount: 5000,
            commentCount: 500,
            shareCount: 200);

        // Assert
        content.Should().NotBeNull();
        content.PlatformConnectionId.Should().Be(_validConnectionId);
        content.PlatformType.Should().Be(PlatformType.YouTube);
        content.ContentId.Should().Be(ValidContentId);
        content.Title.Should().Be(ValidTitle);
        content.ThumbnailUrl.Should().Be(ValidThumbnailUrl);
        content.ContentUrl.Should().Be(ValidContentUrl);
        content.PublishedAt.Should().Be(_validPublishedAt);
        content.ViewCount.Should().Be(100000);
        content.LikeCount.Should().Be(5000);
        content.CommentCount.Should().Be(500);
        content.ShareCount.Should().Be(200);
    }

    [Fact]
    public void CreateShouldCalculateEngagementRate()
    {
        // Arrange & Act
        var content = ContentPerformance.Create(
            _validConnectionId,
            PlatformType.YouTube,
            ValidContentId,
            ValidTitle,
            ValidThumbnailUrl,
            ValidContentUrl,
            _validPublishedAt,
            viewCount: 100000,
            likeCount: 5000,
            commentCount: 2000,
            shareCount: 1000);

        // Assert: (5000 + 2000 + 1000) / 100000 * 100 = 8%
        content.EngagementRate.Should().Be(8.0);
    }

    [Fact]
    public void CreateWithZeroViewsShouldHaveZeroEngagementRate()
    {
        // Arrange & Act
        var content = ContentPerformance.Create(
            _validConnectionId,
            PlatformType.YouTube,
            ValidContentId,
            ValidTitle,
            ValidThumbnailUrl,
            ValidContentUrl,
            _validPublishedAt,
            viewCount: 0,
            likeCount: 0,
            commentCount: 0,
            shareCount: 0);

        // Assert
        content.EngagementRate.Should().Be(0);
    }

    [Fact]
    public void CreateWithNullThumbnailShouldSucceed()
    {
        // Arrange & Act
        var content = ContentPerformance.Create(
            _validConnectionId,
            PlatformType.YouTube,
            ValidContentId,
            ValidTitle,
            thumbnailUrl: null,
            ValidContentUrl,
            _validPublishedAt,
            100000, 5000, 500, 200);

        // Assert
        content.ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public void CreateWithEmptyConnectionIdShouldThrow()
    {
        // Act
        var act = () => ContentPerformance.Create(
            Guid.Empty,
            PlatformType.YouTube,
            ValidContentId,
            ValidTitle,
            ValidThumbnailUrl,
            ValidContentUrl,
            _validPublishedAt,
            100000, 5000, 500, 200);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Platform connection ID cannot be empty.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateWithEmptyContentIdShouldThrow(string? contentId)
    {
        // Act
        var act = () => ContentPerformance.Create(
            _validConnectionId,
            PlatformType.YouTube,
            contentId!,
            ValidTitle,
            ValidThumbnailUrl,
            ValidContentUrl,
            _validPublishedAt,
            100000, 5000, 500, 200);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Content ID cannot be empty.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateWithEmptyTitleShouldThrow(string? title)
    {
        // Act
        var act = () => ContentPerformance.Create(
            _validConnectionId,
            PlatformType.YouTube,
            ValidContentId,
            title!,
            ValidThumbnailUrl,
            ValidContentUrl,
            _validPublishedAt,
            100000, 5000, 500, 200);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty.*");
    }

    [Fact]
    public void CreateWithNegativeViewCountShouldThrow()
    {
        // Act
        var act = () => ContentPerformance.Create(
            _validConnectionId,
            PlatformType.YouTube,
            ValidContentId,
            ValidTitle,
            ValidThumbnailUrl,
            ValidContentUrl,
            _validPublishedAt,
            viewCount: -1,
            likeCount: 5000,
            commentCount: 500,
            shareCount: 200);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("View count cannot be negative.*");
    }

    #endregion

    #region UpdateMetrics Tests

    [Fact]
    public void UpdateMetricsShouldUpdateAllValues()
    {
        // Arrange
        var content = CreateValidContent();

        // Act
        content.UpdateMetrics(
            viewCount: 200000,
            likeCount: 10000,
            commentCount: 1000,
            shareCount: 400);

        // Assert
        content.ViewCount.Should().Be(200000);
        content.LikeCount.Should().Be(10000);
        content.CommentCount.Should().Be(1000);
        content.ShareCount.Should().Be(400);
    }

    [Fact]
    public void UpdateMetricsShouldRecalculateEngagementRate()
    {
        // Arrange
        var content = CreateValidContent();

        // Act
        content.UpdateMetrics(
            viewCount: 200000,
            likeCount: 10000,
            commentCount: 4000,
            shareCount: 2000);

        // Assert: (10000 + 4000 + 2000) / 200000 * 100 = 8%
        content.EngagementRate.Should().Be(8.0);
    }

    [Fact]
    public void UpdateMetricsShouldUpdateLastUpdatedAt()
    {
        // Arrange
        var content = CreateValidContent();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        content.UpdateMetrics(200000, 10000, 1000, 400);

        // Assert
        content.LastUpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    #endregion

    #region UpdateMetadata Tests

    [Fact]
    public void UpdateMetadataShouldUpdateTitleAndThumbnail()
    {
        // Arrange
        var content = CreateValidContent();
        var newTitle = "Updated Title";
        var newThumbnail = "https://new-thumbnail.jpg";

        // Act
        content.UpdateMetadata(newTitle, newThumbnail);

        // Assert
        content.Title.Should().Be(newTitle);
        content.ThumbnailUrl.Should().Be(newThumbnail);
    }

    [Fact]
    public void UpdateMetadataWithEmptyTitleShouldThrow()
    {
        // Arrange
        var content = CreateValidContent();

        // Act
        var act = () => content.UpdateMetadata("", null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty.*");
    }

    #endregion

    #region Helper Methods

    private ContentPerformance CreateValidContent()
    {
        return ContentPerformance.Create(
            _validConnectionId,
            PlatformType.YouTube,
            ValidContentId,
            ValidTitle,
            ValidThumbnailUrl,
            ValidContentUrl,
            _validPublishedAt,
            viewCount: 100000,
            likeCount: 5000,
            commentCount: 500,
            shareCount: 200);
    }

    #endregion
}
