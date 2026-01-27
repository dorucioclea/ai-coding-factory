using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.Commands.UpdateContentIdea;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.ContentIdeas;

/// <summary>
/// Unit tests for UpdateContentIdeaCommandHandler.
/// Story: ACF-005
/// </summary>
[Trait("Story", "ACF-005")]
public class UpdateContentIdeaCommandHandlerTests
{
    private readonly Mock<IContentItemRepository> _repositoryMock;
    private readonly Mock<ILogger<UpdateContentIdeaCommandHandler>> _loggerMock;
    private readonly UpdateContentIdeaCommandHandler _handler;

    public UpdateContentIdeaCommandHandlerTests()
    {
        _repositoryMock = new Mock<IContentItemRepository>();
        _loggerMock = new Mock<ILogger<UpdateContentIdeaCommandHandler>>();
        _handler = new UpdateContentIdeaCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidDataShouldUpdateContentIdea()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Original Title", "Original notes");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var command = new UpdateContentIdeaCommand(itemId, userId, "Updated Title", "Updated notes");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Notes.Should().Be("Updated notes");

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

        var command = new UpdateContentIdeaCommand(itemId, userId, "Title", "Notes");

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

        var command = new UpdateContentIdeaCommand(itemId, attackerId, "Hacked Title", "Hacked notes");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task HandleWithPlatformTagsShouldUpdateTags()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Title", "Notes");
        existingItem.SetPlatformTags(new List<string> { "YouTube" });

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var newTags = new List<string> { "TikTok", "Instagram" };
        var command = new UpdateContentIdeaCommand(itemId, userId, "Title", "Notes", PlatformTags: newTags);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PlatformTags.Should().HaveCount(2);
        result.PlatformTags.Should().Contain("tiktok");
        result.PlatformTags.Should().Contain("instagram");
        result.PlatformTags.Should().NotContain("youtube");
    }

    [Fact]
    public async Task HandleWithStatusShouldUpdateStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Title", "Notes");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var command = new UpdateContentIdeaCommand(itemId, userId, "Title", "Notes", Status: IdeaStatus.Draft);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(IdeaStatus.Draft);
    }

    [Fact]
    public async Task HandleWithNullPlatformTagsShouldNotChangeTags()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Title", "Notes");
        existingItem.SetPlatformTags(new List<string> { "YouTube" });

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var command = new UpdateContentIdeaCommand(itemId, userId, "Updated Title", "Updated notes", PlatformTags: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PlatformTags.Should().Contain("youtube");
    }
}
