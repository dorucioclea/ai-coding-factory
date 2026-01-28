using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.Commands.UpdateContentIdeaStatus;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.ContentIdeas;

/// <summary>
/// Unit tests for UpdateContentIdeaStatusCommandHandler.
/// Story: ACF-005
/// </summary>
[Trait("Story", "ACF-005")]
public class UpdateContentIdeaStatusCommandHandlerTests
{
    private readonly Mock<IContentItemRepository> _repositoryMock;
    private readonly Mock<ILogger<UpdateContentIdeaStatusCommandHandler>> _loggerMock;
    private readonly UpdateContentIdeaStatusCommandHandler _handler;

    public UpdateContentIdeaStatusCommandHandlerTests()
    {
        _repositoryMock = new Mock<IContentItemRepository>();
        _loggerMock = new Mock<ILogger<UpdateContentIdeaStatusCommandHandler>>();
        _handler = new UpdateContentIdeaStatusCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidTransitionShouldUpdateStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Title", "Notes");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var command = new UpdateContentIdeaStatusCommand(itemId, userId, IdeaStatus.Draft);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(IdeaStatus.Draft);

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

        var command = new UpdateContentIdeaStatusCommand(itemId, userId, IdeaStatus.Draft);

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

        var command = new UpdateContentIdeaStatusCommand(itemId, attackerId, IdeaStatus.Draft);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task HandleWithInvalidTransitionShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Title", "Notes");
        // Item starts with Idea status, trying to skip to Scheduled should fail

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var command = new UpdateContentIdeaStatusCommand(itemId, userId, IdeaStatus.Scheduled);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot transition*Invalid status transition*");
    }

    [Fact]
    public async Task HandleShouldAllowProgressThroughEntireWorkflow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Title", "Notes");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        // Act & Assert - Progress through workflow
        // Idea -> Draft
        var command1 = new UpdateContentIdeaStatusCommand(itemId, userId, IdeaStatus.Draft);
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        result1.Status.Should().Be(IdeaStatus.Draft);

        // Draft -> InReview
        var command2 = new UpdateContentIdeaStatusCommand(itemId, userId, IdeaStatus.InReview);
        var result2 = await _handler.Handle(command2, CancellationToken.None);
        result2.Status.Should().Be(IdeaStatus.InReview);

        // InReview -> Approved
        var command3 = new UpdateContentIdeaStatusCommand(itemId, userId, IdeaStatus.Approved);
        var result3 = await _handler.Handle(command3, CancellationToken.None);
        result3.Status.Should().Be(IdeaStatus.Approved);

        // Approved -> Scheduled
        var command4 = new UpdateContentIdeaStatusCommand(itemId, userId, IdeaStatus.Scheduled);
        var result4 = await _handler.Handle(command4, CancellationToken.None);
        result4.Status.Should().Be(IdeaStatus.Scheduled);

        // Scheduled -> Published
        var command5 = new UpdateContentIdeaStatusCommand(itemId, userId, IdeaStatus.Published);
        var result5 = await _handler.Handle(command5, CancellationToken.None);
        result5.Status.Should().Be(IdeaStatus.Published);
    }
}
