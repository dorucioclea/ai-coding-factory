using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.Commands.SendMessage;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using VlogForge.Domain.Interfaces;
using Xunit;

namespace VlogForge.UnitTests.Application.Messaging;

/// <summary>
/// Unit tests for SendMessageCommandHandler.
/// Story: ACF-012
/// </summary>
[Trait("Story", "ACF-012")]
public class SendMessageCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepoMock;
    private readonly Mock<IMessageRepository> _messageRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SendMessageCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ParticipantId = Guid.NewGuid();
    private static readonly Guid ConversationId = Guid.NewGuid();
    private const string ValidContent = "Hello, let's plan the video!";

    public SendMessageCommandHandlerTests()
    {
        _conversationRepoMock = new Mock<IConversationRepository>();
        _messageRepoMock = new Mock<IMessageRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new SendMessageCommandHandler(
            _conversationRepoMock.Object,
            _messageRepoMock.Object,
            _profileRepoMock.Object,
            _unitOfWorkMock.Object,
            new Mock<ILogger<SendMessageCommandHandler>>().Object);

        SetupValidConversation();
        SetupNoRateLimit();
        SetupSenderProfile();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateMessage()
    {
        // Arrange
        var command = new SendMessageCommand(UserId, ConversationId, ValidContent);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(ValidContent);
        result.SenderId.Should().Be(UserId);
        result.ConversationId.Should().NotBeEmpty();

        _messageRepoMock.Verify(
            x => x.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrow()
    {
        // Arrange
        _conversationRepoMock.Setup(x => x.GetByIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var command = new SendMessageCommand(UserId, ConversationId, ValidContent);

        // Act & Assert
        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNotParticipant_ShouldThrow()
    {
        // Arrange
        var nonParticipantId = Guid.NewGuid();
        var command = new SendMessageCommand(nonParticipantId, ConversationId, ValidContent);

        // Act & Assert
        await Assert.ThrowsAsync<NotConversationParticipantException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRateLimitExceeded_ShouldThrow()
    {
        // Arrange
        _messageRepoMock.Setup(x => x.CountSentInLastMinuteAsync(
            UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Message.MaxMessagesPerMinute);

        var command = new SendMessageCommand(UserId, ConversationId, ValidContent);

        // Act & Assert
        await Assert.ThrowsAsync<MessagingRateLimitExceededException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldUpdateConversationLastMessage()
    {
        // Arrange
        var command = new SendMessageCommand(UserId, ConversationId, ValidContent);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _conversationRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupValidConversation()
    {
        var conversation = Conversation.Create(UserId, ParticipantId);
        _conversationRepoMock.Setup(x => x.GetByIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
    }

    private void SetupNoRateLimit()
    {
        _messageRepoMock.Setup(x => x.CountSentInLastMinuteAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    private void SetupSenderProfile()
    {
        var profile = CreatorProfile.Create(UserId, "sender_user", "Sender User");
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(
            UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
    }
}
