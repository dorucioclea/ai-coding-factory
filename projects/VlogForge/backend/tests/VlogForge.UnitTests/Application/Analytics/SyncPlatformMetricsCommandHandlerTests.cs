using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Analytics.Commands.SyncMetrics;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Analytics;

/// <summary>
/// Unit tests for SyncPlatformMetricsCommandHandler.
/// Story: ACF-004
/// </summary>
[Trait("Story", "ACF-004")]
public class SyncPlatformMetricsCommandHandlerTests
{
    private readonly Mock<IPlatformConnectionRepository> _connectionRepositoryMock;
    private readonly Mock<IPlatformMetricsRepository> _metricsRepositoryMock;
    private readonly Mock<IContentPerformanceRepository> _contentRepositoryMock;
    private readonly Mock<IPlatformDataService> _platformServiceMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly Mock<ILogger<SyncPlatformMetricsCommandHandler>> _loggerMock;
    private readonly SyncPlatformMetricsCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testConnectionId = Guid.NewGuid();

    public SyncPlatformMetricsCommandHandlerTests()
    {
        _connectionRepositoryMock = new Mock<IPlatformConnectionRepository>();
        _metricsRepositoryMock = new Mock<IPlatformMetricsRepository>();
        _contentRepositoryMock = new Mock<IContentPerformanceRepository>();
        _platformServiceMock = new Mock<IPlatformDataService>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _loggerMock = new Mock<ILogger<SyncPlatformMetricsCommandHandler>>();

        _platformServiceMock.Setup(x => x.SupportedPlatform).Returns(PlatformType.YouTube);

        _handler = new SyncPlatformMetricsCommandHandler(
            _connectionRepositoryMock.Object,
            _metricsRepositoryMock.Object,
            _contentRepositoryMock.Object,
            new[] { _platformServiceMock.Object },
            _encryptionServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithNoConnectionsShouldReturnZeroResults()
    {
        // Arrange
        var command = new SyncPlatformMetricsCommand(_testUserId);

        _connectionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PlatformsSynced.Should().Be(0);
        result.ContentItemsSynced.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleWithUnauthorizedConnectionShouldThrow()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var connection = CreateTestConnection(otherUserId);
        var command = new SyncPlatformMetricsCommand(_testUserId, _testConnectionId);

        _connectionRepositoryMock
            .Setup(x => x.GetByIdAsync(_testConnectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleWithSpecificConnectionShouldOnlySyncThatConnection()
    {
        // Arrange
        var connection = CreateTestConnection(_testUserId);
        var command = new SyncPlatformMetricsCommand(_testUserId, _testConnectionId);
        var metricsData = CreateSampleMetricsData();

        _connectionRepositoryMock
            .Setup(x => x.GetByIdAsync(_testConnectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        _encryptionServiceMock
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns("decrypted-token");

        _platformServiceMock
            .Setup(x => x.FetchMetricsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsData);

        _platformServiceMock
            .Setup(x => x.FetchTopContentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentData>());

        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlatformMetrics?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PlatformsSynced.Should().Be(1);
        _connectionRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleWithNoAccessTokenShouldAddError()
    {
        // Arrange
        // Create a valid connection, but mock decrypt to return empty
        var connection = CreateTestConnection(_testUserId);
        var command = new SyncPlatformMetricsCommand(_testUserId);

        _connectionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection> { connection });

        // Decrypt returns empty string to simulate no valid token
        _encryptionServiceMock
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns(string.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PlatformsSynced.Should().Be(0);
        result.Errors.Should().Contain(e => e.Contains("No access token"));
    }

    [Fact]
    public async Task HandleShouldCreateNewMetricsWhenNotExists()
    {
        // Arrange
        var connection = CreateTestConnection(_testUserId);
        var command = new SyncPlatformMetricsCommand(_testUserId);
        var metricsData = CreateSampleMetricsData();

        _connectionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection> { connection });

        _encryptionServiceMock
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns("decrypted-token");

        _platformServiceMock
            .Setup(x => x.FetchMetricsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsData);

        _platformServiceMock
            .Setup(x => x.FetchTopContentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentData>());

        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlatformMetrics?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PlatformsSynced.Should().Be(1);
        _metricsRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PlatformMetrics>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldUpdateExistingMetrics()
    {
        // Arrange
        var connection = CreateTestConnection(_testUserId);
        var existingMetrics = PlatformMetrics.Create(
            _testConnectionId, PlatformType.YouTube,
            5000, 100000, 5000, 1000, 200);
        var command = new SyncPlatformMetricsCommand(_testUserId);
        var metricsData = CreateSampleMetricsData();

        _connectionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection> { connection });

        _encryptionServiceMock
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns("decrypted-token");

        _platformServiceMock
            .Setup(x => x.FetchMetricsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsData);

        _platformServiceMock
            .Setup(x => x.FetchTopContentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentData>());

        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMetrics);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PlatformsSynced.Should().Be(1);
        _metricsRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<PlatformMetrics>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldSyncContent()
    {
        // Arrange
        var connection = CreateTestConnection(_testUserId);
        var command = new SyncPlatformMetricsCommand(_testUserId);
        var metricsData = CreateSampleMetricsData();
        var contentData = CreateSampleContentData();

        _connectionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection> { connection });

        _encryptionServiceMock
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns("decrypted-token");

        _platformServiceMock
            .Setup(x => x.FetchMetricsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsData);

        _platformServiceMock
            .Setup(x => x.FetchTopContentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentData);

        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlatformMetrics?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ContentItemsSynced.Should().Be(2);
        _contentRepositoryMock.Verify(
            x => x.UpsertAsync(It.IsAny<ContentPerformance>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task HandleWithPlatformErrorShouldContinueWithOtherPlatforms()
    {
        // Arrange
        var command = new SyncPlatformMetricsCommand(_testUserId);

        _connectionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection>
            {
                CreateTestConnection(_testUserId),
                CreateTestConnection(_testUserId, PlatformType.Instagram)
            });

        _encryptionServiceMock
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns("decrypted-token");

        // First call throws, second succeeds
        var callCount = 0;
        _platformServiceMock
            .Setup(x => x.FetchMetricsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((_, _) =>
            {
                callCount++;
                if (callCount == 1)
                    throw new InvalidOperationException("API error");
                return Task.FromResult(CreateSampleMetricsData());
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("API error");
    }

    #region Helper Methods

    private static PlatformConnection CreateTestConnection(Guid userId, PlatformType platform = PlatformType.YouTube)
    {
        return PlatformConnection.Create(
            userId,
            platform,
            "encrypted-access-token",
            "encrypted-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "test-platform-user-id",
            "Test User");
    }

    private static PlatformMetricsData CreateSampleMetricsData()
    {
        return new PlatformMetricsData(
            FollowerCount: 10000,
            TotalViews: 500000,
            TotalLikes: 25000,
            TotalComments: 5000,
            TotalShares: 1000);
    }

    private static List<ContentData> CreateSampleContentData()
    {
        return
        [
            new ContentData(
                ContentId: "video_1",
                Title: "Test Video 1",
                ThumbnailUrl: "https://example.com/thumb1.jpg",
                ContentUrl: "https://youtube.com/watch?v=1",
                PublishedAt: DateTime.UtcNow.AddDays(-7),
                ViewCount: 50000,
                LikeCount: 2500,
                CommentCount: 500,
                ShareCount: 100),
            new ContentData(
                ContentId: "video_2",
                Title: "Test Video 2",
                ThumbnailUrl: "https://example.com/thumb2.jpg",
                ContentUrl: "https://youtube.com/watch?v=2",
                PublishedAt: DateTime.UtcNow.AddDays(-14),
                ViewCount: 30000,
                LikeCount: 1500,
                CommentCount: 300,
                ShareCount: 50)
        ];
    }

    #endregion
}
