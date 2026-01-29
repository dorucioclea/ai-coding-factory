using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.Commands.MarkMessagesAsRead;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Messaging;

/// <summary>
/// Unit tests for MarkMessagesAsReadCommandHandler.
/// Story: ACF-012
/// </summary>
[Trait("Story", "ACF-012")]
public class MarkMessagesAsReadCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepoMock;
    private readonly Mock<IMessageRepository> _messageRepoMock;
    private readonly MarkMessagesAsReadCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ParticipantId = Guid.NewGuid();
    private static readonly Guid ConversationId = Guid.NewGuid();

    public MarkMessagesAsReadCommandHandlerTests()
    {
        _conversationRepoMock = new Mock<IConversationRepository>();
        _messageRepoMock = new Mock<IMessageRepository>();

        _handler = new MarkMessagesAsReadCommandHandler(
            _conversationRepoMock.Object,
            _messageRepoMock.Object,
            new Mock<ILogger<MarkMessagesAsReadCommandHandler>>().Object);

        SetupValidConversation();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnMarkedCount()
    {
        // Arrange
        _messageRepoMock.Setup(x => x.MarkConversationMessagesAsReadAsync(
            It.IsAny<Guid>(), UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var command = new MarkMessagesAsReadCommand(UserId, ConversationId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrow()
    {
        // Arrange
        _conversationRepoMock.Setup(x => x.GetByIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var command = new MarkMessagesAsReadCommand(UserId, ConversationId);

        // Act & Assert
        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNotParticipant_ShouldThrow()
    {
        // Arrange
        var nonParticipantId = Guid.NewGuid();
        var command = new MarkMessagesAsReadCommand(nonParticipantId, ConversationId);

        // Act & Assert
        await Assert.ThrowsAsync<NotConversationParticipantException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNoUnreadMessages_ShouldReturnZero()
    {
        // Arrange
        _messageRepoMock.Setup(x => x.MarkConversationMessagesAsReadAsync(
            It.IsAny<Guid>(), UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var command = new MarkMessagesAsReadCommand(UserId, ConversationId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    private void SetupValidConversation()
    {
        var conversation = Conversation.Create(UserId, ParticipantId);
        _conversationRepoMock.Setup(x => x.GetByIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
    }
}
