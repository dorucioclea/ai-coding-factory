using FluentAssertions;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.Queries.GetContentIdeas;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.ContentIdeas;

/// <summary>
/// Unit tests for GetContentIdeasQueryHandler.
/// Story: ACF-005
/// </summary>
[Trait("Story", "ACF-005")]
public class GetContentIdeasQueryHandlerTests
{
    private readonly Mock<IContentItemRepository> _repositoryMock;
    private readonly GetContentIdeasQueryHandler _handler;

    public GetContentIdeasQueryHandlerTests()
    {
        _repositoryMock = new Mock<IContentItemRepository>();
        _handler = new GetContentIdeasQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleWithNoFiltersShouldReturnAllUserIdeas()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var items = new List<ContentItem>
        {
            ContentItem.Create(userId, "Idea 1", "Notes 1"),
            ContentItem.Create(userId, "Idea 2", "Notes 2"),
            ContentItem.Create(userId, "Idea 3", "Notes 3")
        };

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var query = new GetContentIdeasQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task HandleWithStatusFilterShouldReturnFilteredIdeas()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var draftItem = ContentItem.Create(userId, "Draft Idea", "Notes");
        draftItem.UpdateStatus(IdeaStatus.Draft);

        var items = new List<ContentItem> { draftItem };

        _repositoryMock
            .Setup(x => x.GetByUserIdWithFiltersAsync(
                userId,
                IdeaStatus.Draft,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var query = new GetContentIdeasQuery(userId, Status: IdeaStatus.Draft);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(IdeaStatus.Draft);
    }

    [Fact]
    public async Task HandleWithPlatformTagFilterShouldReturnFilteredIdeas()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var youtubeItem = ContentItem.Create(userId, "YouTube Idea", "Notes");
        youtubeItem.AddPlatformTag("YouTube");

        var items = new List<ContentItem> { youtubeItem };

        _repositoryMock
            .Setup(x => x.GetByUserIdWithFiltersAsync(
                userId,
                null,
                "youtube",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var query = new GetContentIdeasQuery(userId, PlatformTag: "youtube");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].PlatformTags.Should().Contain("youtube");
    }

    [Fact]
    public async Task HandleWithSearchTermShouldReturnMatchingIdeas()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var matchingItem = ContentItem.Create(userId, "Gaming Tutorial", "How to game");

        var items = new List<ContentItem> { matchingItem };

        _repositoryMock
            .Setup(x => x.SearchAsync(
                userId,
                "gaming",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var query = new GetContentIdeasQuery(userId, SearchTerm: "gaming");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Contain("Gaming");
    }

    [Fact]
    public async Task HandleWithEmptyResultShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var items = new List<ContentItem>();

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var query = new GetContentIdeasQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleSearchTermShouldTakePriorityOverFilters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var items = new List<ContentItem>
        {
            ContentItem.Create(userId, "Search Result", "Notes")
        };

        _repositoryMock
            .Setup(x => x.SearchAsync(userId, "search", It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        // Even with status filter, search term should be used
        var query = new GetContentIdeasQuery(userId, Status: IdeaStatus.Draft, SearchTerm: "search");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(x => x.SearchAsync(userId, "search", It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByUserIdWithFiltersAsync(
            It.IsAny<Guid>(),
            It.IsAny<IdeaStatus?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
