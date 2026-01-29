using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for Message entity.
/// Story: ACF-012
/// </summary>
[Trait("Story", "ACF-012")]
public class MessageTests
{
    private static readonly Guid ConversationId = Guid.NewGuid();
    private static readonly Guid SenderId = Guid.NewGuid();
    private const string ValidContent = "Hey, let's collaborate on a video!";

    #region Create

    [Fact]
    public void Create_WithValidData_ShouldReturnMessage()
    {
        // Act
        var message = Message.Create(ConversationId, SenderId, ValidContent);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().NotBeEmpty();
        message.ConversationId.Should().Be(ConversationId);
        message.SenderId.Should().Be(SenderId);
        message.Content.Should().Be(ValidContent);
        message.IsRead.Should().BeFalse();
        message.ReadAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseMessageSentEvent()
    {
        // Act
        var message = Message.Create(ConversationId, SenderId, ValidContent);

        // Assert
        message.DomainEvents.Should().HaveCount(1);
        message.DomainEvents.Should().ContainSingle(e => e is MessageSentEvent);
        var evt = message.DomainEvents.OfType<MessageSentEvent>().First();
        evt.MessageId.Should().Be(message.Id);
        evt.ConversationId.Should().Be(ConversationId);
        evt.SenderId.Should().Be(SenderId);
        evt.Content.Should().Be(ValidContent);
    }

    [Fact]
    public void Create_WithEmptyConversationId_ShouldThrow()
    {
        // Act & Assert
        var act = () => Message.Create(Guid.Empty, SenderId, ValidContent);
        act.Should().Throw<ArgumentException>().WithMessage("*Conversation ID*");
    }

    [Fact]
    public void Create_WithEmptySenderId_ShouldThrow()
    {
        // Act & Assert
        var act = () => Message.Create(ConversationId, Guid.Empty, ValidContent);
        act.Should().Throw<ArgumentException>().WithMessage("*Sender ID*");
    }

    [Fact]
    public void Create_WithEmptyContent_ShouldThrow()
    {
        // Act & Assert
        var act = () => Message.Create(ConversationId, SenderId, "");
        act.Should().Throw<ArgumentException>().WithMessage("*content*");
    }

    [Fact]
    public void Create_WithWhitespaceContent_ShouldThrow()
    {
        // Act & Assert
        var act = () => Message.Create(ConversationId, SenderId, "   ");
        act.Should().Throw<ArgumentException>().WithMessage("*content*");
    }

    [Fact]
    public void Create_WithTooLongContent_ShouldThrow()
    {
        // Arrange
        var longContent = new string('a', Message.MaxContentLength + 1);

        // Act & Assert
        var act = () => Message.Create(ConversationId, SenderId, longContent);
        act.Should().Throw<ArgumentException>().WithMessage("*2000*");
    }

    [Fact]
    public void Create_WithContentAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxContent = new string('a', Message.MaxContentLength);

        // Act
        var message = Message.Create(ConversationId, SenderId, maxContent);

        // Assert
        message.Content.Should().HaveLength(Message.MaxContentLength);
    }

    [Fact]
    public void Create_ShouldTrimContent()
    {
        // Act
        var message = Message.Create(ConversationId, SenderId, "  Hello  ");

        // Assert
        message.Content.Should().Be("Hello");
    }

    #endregion

    #region MarkAsRead

    [Fact]
    public void MarkAsRead_WhenUnread_ShouldSetIsReadAndReadAt()
    {
        // Arrange
        var message = Message.Create(ConversationId, SenderId, ValidContent);

        // Act
        message.MarkAsRead();

        // Assert
        message.IsRead.Should().BeTrue();
        message.ReadAt.Should().NotBeNull();
        message.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsRead_WhenUnread_ShouldRaiseMessageReadEvent()
    {
        // Arrange
        var message = Message.Create(ConversationId, SenderId, ValidContent);
        message.ClearDomainEvents();

        // Act
        message.MarkAsRead();

        // Assert
        message.DomainEvents.Should().HaveCount(1);
        message.DomainEvents.Should().ContainSingle(e => e is MessageReadEvent);
        var evt = message.DomainEvents.OfType<MessageReadEvent>().First();
        evt.MessageId.Should().Be(message.Id);
        evt.ConversationId.Should().Be(ConversationId);
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_ShouldBeNoOp()
    {
        // Arrange
        var message = Message.Create(ConversationId, SenderId, ValidContent);
        message.MarkAsRead();
        message.ClearDomainEvents();
        var originalReadAt = message.ReadAt;

        // Act
        message.MarkAsRead();

        // Assert
        message.IsRead.Should().BeTrue();
        message.ReadAt.Should().Be(originalReadAt);
        message.DomainEvents.Should().BeEmpty();
    }

    #endregion
}
