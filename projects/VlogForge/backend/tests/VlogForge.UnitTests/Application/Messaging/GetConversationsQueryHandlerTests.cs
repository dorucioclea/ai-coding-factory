using FluentAssertions;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.Queries.GetConversations;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Messaging;

/// <summary>
/// Unit tests for GetConversationsQueryHandler.
/// Story: ACF-012
/// </summary>
[Trait("Story", "ACF-012")]
public class GetConversationsQueryHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepoMock;
    private readonly Mock<IMessageRepository> _messageRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly GetConversationsQueryHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid Participant1Id = Guid.NewGuid();
    private static readonly Guid Participant2Id = Guid.NewGuid();

    public GetConversationsQueryHandlerTests()
    {
        _conversationRepoMock = new Mock<IConversationRepository>();
        _messageRepoMock = new Mock<IMessageRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();

        _handler = new GetConversationsQueryHandler(
            _conversationRepoMock.Object,
            _messageRepoMock.Object,
            _profileRepoMock.Object);

        SetupProfiles();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedConversations()
    {
        // Arrange
        var conversations = new List<Conversation>
        {
            Conversation.Create(UserId, Participant1Id),
            Conversation.Create(UserId, Participant2Id)
        };

        _conversationRepoMock.Setup(x => x.GetUserConversationsAsync(
            UserId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((conversations.AsReadOnly() as IReadOnlyList<Conversation>, 2));

        _messageRepoMock.Setup(x => x.GetUnreadCountForConversationAsync(
            It.IsAny<Guid>(), UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var query = new GetConversationsQuery(UserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_WhenNoConversations_ShouldReturnEmptyList()
    {
        // Arrange
        _conversationRepoMock.Setup(x => x.GetUserConversationsAsync(
            UserId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Conversation>() as IReadOnlyList<Conversation>, 0));

        var query = new GetConversationsQuery(UserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldIncludeUnreadCountPerConversation()
    {
        // Arrange
        var conversation = Conversation.Create(UserId, Participant1Id);
        var conversations = new List<Conversation> { conversation };

        _conversationRepoMock.Setup(x => x.GetUserConversationsAsync(
            UserId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((conversations.AsReadOnly() as IReadOnlyList<Conversation>, 1));

        _messageRepoMock.Setup(x => x.GetUnreadCountForConversationAsync(
            conversation.Id, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var query = new GetConversationsQuery(UserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].UnreadCount.Should().Be(7);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldPassPageParameters()
    {
        // Arrange
        var conversation = Conversation.Create(UserId, Participant1Id);
        var conversations = new List<Conversation> { conversation };

        _conversationRepoMock.Setup(x => x.GetUserConversationsAsync(
            UserId, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((conversations.AsReadOnly() as IReadOnlyList<Conversation>, 15));

        _messageRepoMock.Setup(x => x.GetUnreadCountForConversationAsync(
            It.IsAny<Guid>(), UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetConversationsQuery(UserId, Page: 2, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        _conversationRepoMock.Verify(
            x => x.GetUserConversationsAsync(UserId, 2, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupProfiles()
    {
        var profiles = new Dictionary<Guid, CreatorProfile>
        {
            { Participant1Id, CreatorProfile.Create(Participant1Id, $"user_{Participant1Id.ToString()[..8]}", "Participant One") },
            { Participant2Id, CreatorProfile.Create(Participant2Id, $"user_{Participant2Id.ToString()[..8]}", "Participant Two") }
        };

        _profileRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid userId, CancellationToken _) =>
                profiles.TryGetValue(userId, out var p) ? p : null);
    }
}
