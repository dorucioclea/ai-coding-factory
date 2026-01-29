using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.Commands.StartConversation;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Messaging;

/// <summary>
/// Unit tests for StartConversationCommandHandler.
/// Story: ACF-012
/// </summary>
[Trait("Story", "ACF-012")]
public class StartConversationCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepoMock;
    private readonly Mock<ICollaborationRequestRepository> _collaborationRepoMock;
    private readonly Mock<ITeamRepository> _teamRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly Mock<IMessageRepository> _messageRepoMock;
    private readonly StartConversationCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ParticipantId = Guid.NewGuid();

    public StartConversationCommandHandlerTests()
    {
        _conversationRepoMock = new Mock<IConversationRepository>();
        _collaborationRepoMock = new Mock<ICollaborationRequestRepository>();
        _teamRepoMock = new Mock<ITeamRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();
        _messageRepoMock = new Mock<IMessageRepository>();

        _handler = new StartConversationCommandHandler(
            _conversationRepoMock.Object,
            _collaborationRepoMock.Object,
            _teamRepoMock.Object,
            _profileRepoMock.Object,
            _messageRepoMock.Object,
            new Mock<ILogger<StartConversationCommandHandler>>().Object);

        SetupValidProfiles();
        SetupNoExistingConversation();
        SetupNoCollaboration();
        SetupNoTeam();
    }

    [Fact]
    public async Task Handle_WithAcceptedSentCollaboration_ShouldCreateConversation()
    {
        // Arrange
        SetupSentCollaborationWith(ParticipantId);
        var command = new StartConversationCommand(UserId, ParticipantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ParticipantId.Should().Be(ParticipantId);

        _conversationRepoMock.Verify(
            x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _conversationRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConversationExists_ShouldReturnExisting()
    {
        // Arrange
        var existingConversation = Conversation.Create(UserId, ParticipantId);
        _conversationRepoMock.Setup(x => x.GetByParticipantsAsync(
            UserId, ParticipantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConversation);

        _messageRepoMock.Setup(x => x.GetUnreadCountForConversationAsync(
            existingConversation.Id, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var command = new StartConversationCommand(UserId, ParticipantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingConversation.Id);
        result.UnreadCount.Should().Be(2);

        _conversationRepoMock.Verify(
            x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSenderHasNoProfile_ShouldThrow()
    {
        // Arrange
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        var command = new StartConversationCommand(UserId, ParticipantId);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenParticipantHasNoProfile_ShouldThrow()
    {
        // Arrange
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(ParticipantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        var command = new StartConversationCommand(UserId, ParticipantId);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNoMutualCollaboration_ShouldThrow()
    {
        // Arrange - no sent, no received, no team
        var command = new StartConversationCommand(UserId, ParticipantId);

        // Act & Assert
        await Assert.ThrowsAsync<NoMutualCollaborationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSharedTeamMembership_ShouldCreateConversation()
    {
        // Arrange
        _teamRepoMock.Setup(x => x.AreInSameTeamAsync(
            UserId, ParticipantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new StartConversationCommand(UserId, ParticipantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _conversationRepoMock.Verify(
            x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithReceivedAcceptedRequest_ShouldCreateConversation()
    {
        // Arrange
        SetupReceivedCollaborationFrom(ParticipantId);
        var command = new StartConversationCommand(UserId, ParticipantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _conversationRepoMock.Verify(
            x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupValidProfiles()
    {
        var userProfile = CreateProfile(UserId);
        var participantProfile = CreateProfile(ParticipantId);

        _profileRepoMock.Setup(x => x.GetByUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(ParticipantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participantProfile);
    }

    private void SetupNoExistingConversation()
    {
        _conversationRepoMock.Setup(x => x.GetByParticipantsAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);
    }

    private void SetupNoCollaboration()
    {
        var emptyList = new List<CollaborationRequest>().AsReadOnly() as IReadOnlyList<CollaborationRequest>;
        _collaborationRepoMock.Setup(x => x.GetSentRequestsAsync(
            It.IsAny<Guid>(), It.IsAny<CollaborationRequestStatus?>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyList, 0));
        _collaborationRepoMock.Setup(x => x.GetReceivedRequestsAsync(
            It.IsAny<Guid>(), It.IsAny<CollaborationRequestStatus?>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyList, 0));
    }

    private void SetupNoTeam()
    {
        _teamRepoMock.Setup(x => x.AreInSameTeamAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupSentCollaborationWith(Guid recipientId)
    {
        var request = CollaborationRequest.Create(UserId, recipientId, "Let's collab!");
        request.Accept(recipientId);
        var list = new List<CollaborationRequest> { request }.AsReadOnly() as IReadOnlyList<CollaborationRequest>;

        _collaborationRepoMock.Setup(x => x.GetSentRequestsAsync(
            UserId, It.IsAny<CollaborationRequestStatus?>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((list, 1));
    }

    private void SetupReceivedCollaborationFrom(Guid senderId)
    {
        var request = CollaborationRequest.Create(senderId, UserId, "Let's collab!");
        request.Accept(UserId);
        var list = new List<CollaborationRequest> { request }.AsReadOnly() as IReadOnlyList<CollaborationRequest>;

        _collaborationRepoMock.Setup(x => x.GetReceivedRequestsAsync(
            UserId, It.IsAny<CollaborationRequestStatus?>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((list, 1));
    }

    private static CreatorProfile CreateProfile(Guid userId)
    {
        var username = $"user_{userId.ToString()[..8]}";
        return CreatorProfile.Create(userId, username, $"Display {username}");
    }
}
