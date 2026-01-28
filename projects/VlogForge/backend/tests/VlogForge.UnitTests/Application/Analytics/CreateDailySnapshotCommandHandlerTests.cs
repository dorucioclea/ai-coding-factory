using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Analytics.Commands.CreateSnapshot;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Analytics;

/// <summary>
/// Unit tests for CreateDailySnapshotCommandHandler.
/// Story: ACF-004
/// </summary>
[Trait("Story", "ACF-004")]
public class CreateDailySnapshotCommandHandlerTests
{
    private readonly Mock<IPlatformMetricsRepository> _metricsRepositoryMock;
    private readonly Mock<IMetricsSnapshotRepository> _snapshotRepositoryMock;
    private readonly Mock<IPlatformConnectionRepository> _connectionRepositoryMock;
    private readonly Mock<ILogger<CreateDailySnapshotCommandHandler>> _loggerMock;
    private readonly CreateDailySnapshotCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testConnectionId = Guid.NewGuid();

    public CreateDailySnapshotCommandHandlerTests()
    {
        _metricsRepositoryMock = new Mock<IPlatformMetricsRepository>();
        _snapshotRepositoryMock = new Mock<IMetricsSnapshotRepository>();
        _connectionRepositoryMock = new Mock<IPlatformConnectionRepository>();
        _loggerMock = new Mock<ILogger<CreateDailySnapshotCommandHandler>>();

        _handler = new CreateDailySnapshotCommandHandler(
            _metricsRepositoryMock.Object,
            _snapshotRepositoryMock.Object,
            _connectionRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithNoConnectionsShouldReturnZeroSnapshots()
    {
        // Arrange
        var command = new CreateDailySnapshotCommand(DateTime.UtcNow.Date);

        _connectionRepositoryMock
            .Setup(x => x.GetAllActiveConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SnapshotsCreated.Should().Be(0);
        result.UsersProcessed.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleShouldCreateSnapshotsForActiveConnections()
    {
        // Arrange
        var snapshotDate = DateTime.UtcNow.Date;
        var command = new CreateDailySnapshotCommand(snapshotDate);
        var connection = CreateTestConnection();
        var metrics = CreateTestMetrics();

        _connectionRepositoryMock
            .Setup(x => x.GetAllActiveConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection> { connection });

        // Use connection.Id as the key (not _testConnectionId)
        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, PlatformMetrics> { { connection.Id, metrics } });

        _snapshotRepositoryMock
            .Setup(x => x.GetExistingForDateAsync(snapshotDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(Guid, PlatformType)>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SnapshotsCreated.Should().Be(1);
        result.UsersProcessed.Should().Be(1);
        _snapshotRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<MetricsSnapshot>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldSkipExistingSnapshots()
    {
        // Arrange
        var snapshotDate = DateTime.UtcNow.Date;
        var command = new CreateDailySnapshotCommand(snapshotDate);
        var connection = CreateTestConnection();
        var metrics = CreateTestMetrics();

        _connectionRepositoryMock
            .Setup(x => x.GetAllActiveConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection> { connection });

        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, PlatformMetrics> { { _testConnectionId, metrics } });

        _snapshotRepositoryMock
            .Setup(x => x.GetExistingForDateAsync(snapshotDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(Guid, PlatformType)> { (_testUserId, PlatformType.YouTube) });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SnapshotsCreated.Should().Be(0);
        _snapshotRepositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<MetricsSnapshot>>(s => !s.Any()), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleShouldSkipConnectionsWithoutMetrics()
    {
        // Arrange
        var snapshotDate = DateTime.UtcNow.Date;
        var command = new CreateDailySnapshotCommand(snapshotDate);
        var connection = CreateTestConnection();

        _connectionRepositoryMock
            .Setup(x => x.GetAllActiveConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection> { connection });

        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, PlatformMetrics>()); // Empty - no metrics

        _snapshotRepositoryMock
            .Setup(x => x.GetExistingForDateAsync(snapshotDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(Guid, PlatformType)>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SnapshotsCreated.Should().Be(0);
    }

    [Fact]
    public async Task HandleShouldProcessMultipleConnectionsForSameUser()
    {
        // Arrange
        var snapshotDate = DateTime.UtcNow.Date;
        var command = new CreateDailySnapshotCommand(snapshotDate);
        var connection1 = CreateTestConnection(PlatformType.YouTube);
        var connection2 = CreateTestConnection(PlatformType.Instagram);

        var connections = new List<PlatformConnection> { connection1, connection2 };

        // Build metrics dict using actual connection IDs
        var metricsDict = new Dictionary<Guid, PlatformMetrics>
        {
            { connection1.Id, CreateTestMetrics(PlatformType.YouTube) },
            { connection2.Id, CreateTestMetrics(PlatformType.Instagram) }
        };

        _connectionRepositoryMock
            .Setup(x => x.GetAllActiveConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connections);

        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsDict);

        _snapshotRepositoryMock
            .Setup(x => x.GetExistingForDateAsync(snapshotDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(Guid, PlatformType)>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UsersProcessed.Should().Be(1); // Same user, multiple platforms
        result.SnapshotsCreated.Should().Be(2); // Two snapshots (one per platform)
    }

    [Fact]
    public async Task HandleWithBatchLoadFailureShouldNotCreateSnapshots()
    {
        // Arrange
        var snapshotDate = DateTime.UtcNow.Date;
        var command = new CreateDailySnapshotCommand(snapshotDate);
        var connection = CreateTestConnection();

        _connectionRepositoryMock
            .Setup(x => x.GetAllActiveConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlatformConnection> { connection });

        _metricsRepositoryMock
            .Setup(x => x.GetByConnectionIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        // When batch loading fails, the exception bubbles up
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    #region Helper Methods

    private PlatformConnection CreateTestConnection(PlatformType platform = PlatformType.YouTube)
    {
        return PlatformConnection.Create(
            _testUserId,
            platform,
            "encrypted-access-token",
            "encrypted-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "test-platform-user-id",
            "Test User");
    }

    private PlatformMetrics CreateTestMetrics(PlatformType platform = PlatformType.YouTube)
    {
        return PlatformMetrics.Create(
            _testConnectionId,
            platform,
            followerCount: 10000,
            totalViews: 500000,
            totalLikes: 25000,
            totalComments: 5000,
            totalShares: 1000);
    }

    #endregion
}
