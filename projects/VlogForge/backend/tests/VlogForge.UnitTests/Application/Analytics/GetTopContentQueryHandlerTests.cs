using FluentAssertions;
using Moq;
using VlogForge.Application.Analytics.Queries.GetTopContent;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Analytics;

/// <summary>
/// Unit tests for GetTopContentQueryHandler.
/// Story: ACF-004 (AC4)
/// </summary>
[Trait("Story", "ACF-004")]
public class GetTopContentQueryHandlerTests
{
    private readonly Mock<IContentPerformanceRepository> _contentRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetTopContentQueryHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testConnectionId = Guid.NewGuid();

    public GetTopContentQueryHandlerTests()
    {
        _contentRepositoryMock = new Mock<IContentPerformanceRepository>();
        _cacheServiceMock = new Mock<ICacheService>();

        _handler = new GetTopContentQueryHandler(
            _contentRepositoryMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithCachedDataShouldReturnFromCache()
    {
        // Arrange
        var query = new GetTopContentQuery(_testUserId);
        var cachedResponse = CreateSampleTopContentResponse();

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.TopContentResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedResponse);
        _contentRepositoryMock.Verify(
            x => x.GetTopContentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleWithNoContentShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetTopContentQuery(_testUserId);

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.TopContentResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.TopContentResponse?)null);

        _contentRepositoryMock
            .Setup(x => x.GetTopContentAsync(_testUserId, 10, "views", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentPerformance>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Content.Should().BeEmpty();
        result.SortedBy.Should().Be("views");
    }

    [Fact]
    public async Task HandleShouldReturnContentSortedByViews()
    {
        // Arrange
        var query = new GetTopContentQuery(_testUserId, Limit: 5, SortBy: "views");
        var content = CreateSampleContent();

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.TopContentResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.TopContentResponse?)null);

        _contentRepositoryMock
            .Setup(x => x.GetTopContentAsync(_testUserId, 5, "views", It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Content.Should().HaveCount(2);
        result.SortedBy.Should().Be("views");
    }

    [Theory]
    [InlineData("engagement")]
    [InlineData("likes")]
    [InlineData("comments")]
    public async Task HandleShouldNormalizeSortBy(string sortBy)
    {
        // Arrange
        var query = new GetTopContentQuery(_testUserId, SortBy: sortBy.ToUpperInvariant());

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.TopContentResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.TopContentResponse?)null);

        _contentRepositoryMock
            .Setup(x => x.GetTopContentAsync(_testUserId, 10, sortBy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentPerformance>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.SortedBy.Should().Be(sortBy);
    }

    [Fact]
    public async Task HandleShouldCacheResult()
    {
        // Arrange
        var query = new GetTopContentQuery(_testUserId);

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.TopContentResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.TopContentResponse?)null);

        _contentRepositoryMock
            .Setup(x => x.GetTopContentAsync(_testUserId, 10, "views", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentPerformance>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<VlogForge.Application.Analytics.DTOs.TopContentResponse>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldMapContentToDto()
    {
        // Arrange
        var query = new GetTopContentQuery(_testUserId);
        var content = CreateSampleContent();

        _cacheServiceMock
            .Setup(x => x.GetAsync<VlogForge.Application.Analytics.DTOs.TopContentResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VlogForge.Application.Analytics.DTOs.TopContentResponse?)null);

        _contentRepositoryMock
            .Setup(x => x.GetTopContentAsync(_testUserId, 10, "views", It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var firstContent = result.Content[0];
        firstContent.ContentId.Should().Be("video_1");
        firstContent.Title.Should().Be("Top Video 1");
        firstContent.ViewCount.Should().Be(100000);
        firstContent.LikeCount.Should().Be(5000);
    }

    #region Helper Methods

    private static VlogForge.Application.Analytics.DTOs.TopContentResponse CreateSampleTopContentResponse()
    {
        return new VlogForge.Application.Analytics.DTOs.TopContentResponse(
            Content: [],
            SortedBy: "views");
    }

    private List<ContentPerformance> CreateSampleContent()
    {
        return
        [
            ContentPerformance.Create(
                _testConnectionId,
                PlatformType.YouTube,
                "video_1",
                "Top Video 1",
                "https://img.youtube.com/vi/1/thumb.jpg",
                "https://youtube.com/watch?v=1",
                DateTime.UtcNow.AddDays(-7),
                viewCount: 100000,
                likeCount: 5000,
                commentCount: 500,
                shareCount: 200),
            ContentPerformance.Create(
                _testConnectionId,
                PlatformType.YouTube,
                "video_2",
                "Top Video 2",
                "https://img.youtube.com/vi/2/thumb.jpg",
                "https://youtube.com/watch?v=2",
                DateTime.UtcNow.AddDays(-14),
                viewCount: 80000,
                likeCount: 4000,
                commentCount: 400,
                shareCount: 150)
        ];
    }

    #endregion
}
