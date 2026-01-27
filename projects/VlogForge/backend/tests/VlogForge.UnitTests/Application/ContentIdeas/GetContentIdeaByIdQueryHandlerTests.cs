using FluentAssertions;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.Queries.GetContentIdeaById;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.ContentIdeas;

/// <summary>
/// Unit tests for GetContentIdeaByIdQueryHandler.
/// Story: ACF-005
/// </summary>
[Trait("Story", "ACF-005")]
public class GetContentIdeaByIdQueryHandlerTests
{
    private readonly Mock<IContentItemRepository> _repositoryMock;
    private readonly GetContentIdeaByIdQueryHandler _handler;

    public GetContentIdeaByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IContentItemRepository>();
        _handler = new GetContentIdeaByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleWithExistingIdShouldReturnContentIdea()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Test Idea", "Test notes");
        existingItem.AddPlatformTag("YouTube");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var query = new GetContentIdeaByIdQuery(itemId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Idea");
        result.Notes.Should().Be("Test notes");
        result.UserId.Should().Be(userId);
        result.PlatformTags.Should().Contain("youtube");
    }

    [Fact]
    public async Task HandleWithNonExistentIdShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);

        var query = new GetContentIdeaByIdQuery(itemId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
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

        var query = new GetContentIdeaByIdQuery(itemId, attackerId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task HandleShouldReturnCorrectStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = ContentItem.Create(userId, "Test Idea", "Notes");
        existingItem.UpdateStatus(IdeaStatus.Draft);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var query = new GetContentIdeaByIdQuery(itemId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(IdeaStatus.Draft);
    }

    [Fact]
    public async Task HandleShouldReturnCorrectTimestamps()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var beforeCreate = DateTime.UtcNow;
        var existingItem = ContentItem.Create(userId, "Test Idea", "Notes");
        var afterCreate = DateTime.UtcNow;

        _repositoryMock
            .Setup(x => x.GetByIdAsync(itemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var query = new GetContentIdeaByIdQuery(itemId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        result.CreatedAt.Should().BeOnOrBefore(afterCreate);
    }
}
