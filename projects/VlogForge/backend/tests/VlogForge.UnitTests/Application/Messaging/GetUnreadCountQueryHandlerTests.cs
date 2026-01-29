using FluentAssertions;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.Queries.GetUnreadCount;
using Xunit;

namespace VlogForge.UnitTests.Application.Messaging;

/// <summary>
/// Unit tests for GetUnreadCountQueryHandler.
/// Story: ACF-012
/// </summary>
[Trait("Story", "ACF-012")]
public class GetUnreadCountQueryHandlerTests
{
    private readonly Mock<IMessageRepository> _messageRepoMock;
    private readonly GetUnreadCountQueryHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();

    public GetUnreadCountQueryHandlerTests()
    {
        _messageRepoMock = new Mock<IMessageRepository>();
        _handler = new GetUnreadCountQueryHandler(_messageRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnreadCount()
    {
        // Arrange
        _messageRepoMock.Setup(x => x.GetUnreadCountAsync(
            UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);

        var query = new GetUnreadCountQuery(UserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(12);
        _messageRepoMock.Verify(
            x => x.GetUnreadCountAsync(UserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoUnreadMessages_ShouldReturnZero()
    {
        // Arrange
        _messageRepoMock.Setup(x => x.GetUnreadCountAsync(
            UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetUnreadCountQuery(UserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }
}
