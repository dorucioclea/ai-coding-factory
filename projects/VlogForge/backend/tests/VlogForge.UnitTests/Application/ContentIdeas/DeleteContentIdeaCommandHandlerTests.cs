using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.Commands.DeleteContentIdea;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.ContentIdeas;

/// <summary>
/// Unit tests for DeleteContentIdeaCommandHandler.
/// Story: ACF-005
/// </summary>
[Trait("Story", "ACF-005")]
public class DeleteContentIdeaCommandHandlerTests
{
    private readonly Mock<IContentItemRepository> _repositoryMock;
    private readonly Mock<ILogger<DeleteContentIdeaCommandHandler>> _loggerMock;
    private readonly DeleteContentIdeaCommandHandler _handler;

    public DeleteContentIdeaCommandHandlerTests()
    {
        _repositoryMock = new Mock<IContentItemRepository>();
        _loggerMock = new Mock<ILogger<DeleteContentIdeaCommandHandler>>();
        _handler = new DeleteContentIdeaCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidIdShouldSoftDeleteContentIdea()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Title", "Notes");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var command = new DeleteContentIdeaCommand(itemId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingItem.IsDeleted.Should().BeTrue();
        existingItem.DeletedAt.Should().NotBeNull();

        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ContentItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNonExistentIdShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);

        var command = new DeleteContentIdeaCommand(itemId, userId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleWithDifferentUserIdShouldThrowForbiddenAccessException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(ownerId, "Title", "Notes");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var command = new DeleteContentIdeaCommand(itemId, attackerId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task HandleShouldSetDeletedAtTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Title", "Notes");
        var beforeDelete = DateTime.UtcNow;

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var command = new DeleteContentIdeaCommand(itemId, userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        var afterDelete = DateTime.UtcNow;

        // Assert
        existingItem.DeletedAt.Should().NotBeNull();
        existingItem.DeletedAt.Should().BeOnOrAfter(beforeDelete);
        existingItem.DeletedAt.Should().BeOnOrBefore(afterDelete);
    }
}
