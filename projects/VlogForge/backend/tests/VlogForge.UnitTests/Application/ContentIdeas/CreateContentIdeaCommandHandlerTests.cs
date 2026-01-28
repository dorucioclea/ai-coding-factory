using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.Commands.CreateContentIdea;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.ContentIdeas;

/// <summary>
/// Unit tests for CreateContentIdeaCommandHandler.
/// Story: ACF-005
/// </summary>
[Trait("Story", "ACF-005")]
public class CreateContentIdeaCommandHandlerTests
{
    private readonly Mock<IContentItemRepository> _repositoryMock;
    private readonly Mock<ILogger<CreateContentIdeaCommandHandler>> _loggerMock;
    private readonly CreateContentIdeaCommandHandler _handler;

    public CreateContentIdeaCommandHandlerTests()
    {
        _repositoryMock = new Mock<IContentItemRepository>();
        _loggerMock = new Mock<ILogger<CreateContentIdeaCommandHandler>>();
        _handler = new CreateContentIdeaCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidDataShouldCreateContentIdea()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateContentIdeaCommand(userId, "Test Video Idea", "Some notes about the video");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Video Idea");
        result.Notes.Should().Be("Some notes about the video");
        result.Status.Should().Be(IdeaStatus.Idea);
        result.UserId.Should().Be(userId);

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<ContentItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithPlatformTagsShouldIncludeNormalizedTags()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tags = new List<string> { "YouTube", "TikTok", "Instagram" };
        var command = new CreateContentIdeaCommand(userId, "Test Video", Notes: null, PlatformTags: tags);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PlatformTags.Should().HaveCount(3);
        result.PlatformTags.Should().Contain("youtube");
        result.PlatformTags.Should().Contain("tiktok");
        result.PlatformTags.Should().Contain("instagram");
    }

    [Fact]
    public async Task HandleWithNullNotesShouldCreateWithNullOrEmptyNotes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateContentIdeaCommand(userId, "Test Video");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Video");
        result.Notes.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task HandleWithEmptyPlatformTagsShouldCreateWithNoTags()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateContentIdeaCommand(userId, "Test Video", "Notes", PlatformTags: new List<string>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PlatformTags.Should().BeEmpty();
    }

}
