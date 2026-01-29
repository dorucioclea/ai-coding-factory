using FluentAssertions;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.Queries.GetMessages;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Messaging;

/// <summary>
/// Unit tests for GetMessagesQueryHandler.
/// Story: ACF-012
/// </summary>
[Trait("Story", "ACF-012")]
public class GetMessagesQueryHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepoMock;
    private readonly Mock<IMessageRepository> _messageRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly GetMessagesQueryHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ParticipantId = Guid.NewGuid();
    private static readonly Guid ConversationId = Guid.NewGuid();

    public GetMessagesQueryHandlerTests()
    {
        _conversationRepoMock = new Mock<IConversationRepository>();
        _messageRepoMock = new Mock<IMessageRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();

        _handler = new GetMessagesQueryHandler(
            _conversationRepoMock.Object,
            _messageRepoMock.Object,
            _profileRepoMock.Object);

        SetupValidConversation();
        SetupSenderProfile();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedMessages()
    {
        // Arrange
        var conversation = Conversation.Create(UserId, ParticipantId);
        var messages = new List<Message>
        {
            Message.Create(conversation.Id, UserId, "Hello!"),
            Message.Create(conversation.Id, ParticipantId, "Hi there!")
        };

        _messageRepoMock.Setup(x => x.GetConversationMessagesAsync(
            It.IsAny<Guid>(), 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((messages.AsReadOnly() as IReadOnlyList<Message>, 2));

        var query = new GetMessagesQuery(UserId, ConversationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrow()
    {
        // Arrange
        _conversationRepoMock.Setup(x => x.GetByIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var query = new GetMessagesQuery(UserId, ConversationId);

        // Act & Assert
        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNotParticipant_ShouldThrow()
    {
        // Arrange
        var nonParticipantId = Guid.NewGuid();
        var query = new GetMessagesQuery(nonParticipantId, ConversationId);

        // Act & Assert
        await Assert.ThrowsAsync<NotConversationParticipantException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldPassPageParameters()
    {
        // Arrange
        var conversation = Conversation.Create(UserId, ParticipantId);
        var messages = new List<Message>
        {
            Message.Create(conversation.Id, UserId, "Page 2 message")
        };

        _messageRepoMock.Setup(x => x.GetConversationMessagesAsync(
            It.IsAny<Guid>(), 2, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync((messages.AsReadOnly() as IReadOnlyList<Message>, 30));

        var query = new GetMessagesQuery(UserId, ConversationId, Page: 2, PageSize: 25);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(30);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(25);
        _messageRepoMock.Verify(
            x => x.GetConversationMessagesAsync(It.IsAny<Guid>(), 2, 25, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoMessages_ShouldReturnEmptyList()
    {
        // Arrange
        _messageRepoMock.Setup(x => x.GetConversationMessagesAsync(
            It.IsAny<Guid>(), 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Message>() as IReadOnlyList<Message>, 0));

        var query = new GetMessagesQuery(UserId, ConversationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    private void SetupValidConversation()
    {
        var conversation = Conversation.Create(UserId, ParticipantId);
        _conversationRepoMock.Setup(x => x.GetByIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
    }

    private void SetupSenderProfile()
    {
        var profiles = new Dictionary<Guid, CreatorProfile>
        {
            { UserId, CreatorProfile.Create(UserId, $"user_{UserId.ToString()[..8]}", "Test User") },
            { ParticipantId, CreatorProfile.Create(ParticipantId, $"user_{ParticipantId.ToString()[..8]}", "Participant") }
        };

        _profileRepoMock.Setup(x => x.GetByUserIdsAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> ids, CancellationToken _) =>
            {
                var result = new Dictionary<Guid, CreatorProfile>();
                foreach (var id in ids)
                {
                    if (profiles.TryGetValue(id, out var profile))
                        result[id] = profile;
                }
                return result as IReadOnlyDictionary<Guid, CreatorProfile>;
            });
    }
}
