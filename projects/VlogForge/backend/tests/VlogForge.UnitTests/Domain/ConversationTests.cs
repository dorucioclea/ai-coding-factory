using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for Conversation aggregate root.
/// Story: ACF-012
/// </summary>
[Trait("Story", "ACF-012")]
public class ConversationTests
{
    private static readonly Guid Participant1Id = Guid.NewGuid();
    private static readonly Guid Participant2Id = Guid.NewGuid();
    private static readonly Guid UnrelatedUserId = Guid.NewGuid();

    #region Create

    [Fact]
    public void Create_WithValidData_ShouldReturnConversation()
    {
        // Act
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Assert
        conversation.Should().NotBeNull();
        conversation.Id.Should().NotBeEmpty();
        conversation.Participant1Id.Should().Be(Participant1Id);
        conversation.Participant2Id.Should().Be(Participant2Id);
        conversation.LastMessageAt.Should().BeNull();
        conversation.LastMessagePreview.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseConversationCreatedEvent()
    {
        // Act
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Assert
        conversation.DomainEvents.Should().HaveCount(1);
        conversation.DomainEvents.Should().ContainSingle(e => e is ConversationCreatedEvent);
        var evt = conversation.DomainEvents.OfType<ConversationCreatedEvent>().First();
        evt.ConversationId.Should().Be(conversation.Id);
        evt.Participant1Id.Should().Be(Participant1Id);
        evt.Participant2Id.Should().Be(Participant2Id);
    }

    [Fact]
    public void Create_WithEmptyParticipant1Id_ShouldThrow()
    {
        // Act & Assert
        var act = () => Conversation.Create(Guid.Empty, Participant2Id);
        act.Should().Throw<ArgumentException>().WithMessage("*Participant 1 ID*");
    }

    [Fact]
    public void Create_WithEmptyParticipant2Id_ShouldThrow()
    {
        // Act & Assert
        var act = () => Conversation.Create(Participant1Id, Guid.Empty);
        act.Should().Throw<ArgumentException>().WithMessage("*Participant 2 ID*");
    }

    [Fact]
    public void Create_WithSameParticipants_ShouldThrow()
    {
        // Act & Assert
        var act = () => Conversation.Create(Participant1Id, Participant1Id);
        act.Should().Throw<ArgumentException>().WithMessage("*yourself*");
    }

    #endregion

    #region UpdateLastMessage

    [Fact]
    public void UpdateLastMessage_WithValidPreview_ShouldSetPreviewAndTimestamp()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);
        var preview = "Hello, how are you?";

        // Act
        conversation.UpdateLastMessage(preview);

        // Assert
        conversation.LastMessagePreview.Should().Be(preview);
        conversation.LastMessageAt.Should().NotBeNull();
        conversation.LastMessageAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateLastMessage_WithLongPreview_ShouldTruncateWithEllipsis()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);
        var longPreview = new string('a', 150);

        // Act
        conversation.UpdateLastMessage(longPreview);

        // Assert
        conversation.LastMessagePreview.Should().HaveLength(Conversation.MaxPreviewLength + 3);
        conversation.LastMessagePreview.Should().EndWith("...");
        conversation.LastMessagePreview.Should().StartWith(new string('a', Conversation.MaxPreviewLength));
    }

    [Fact]
    public void UpdateLastMessage_WithEmptyPreview_ShouldThrow()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Act & Assert
        var act = () => conversation.UpdateLastMessage("");
        act.Should().Throw<ArgumentException>().WithMessage("*Preview*");
    }

    #endregion

    #region IsParticipant

    [Fact]
    public void IsParticipant_WithParticipant1Id_ShouldReturnTrue()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Act
        var result = conversation.IsParticipant(Participant1Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParticipant_WithParticipant2Id_ShouldReturnTrue()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Act
        var result = conversation.IsParticipant(Participant2Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParticipant_WithUnrelatedId_ShouldReturnFalse()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Act
        var result = conversation.IsParticipant(UnrelatedUserId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetOtherParticipantId

    [Fact]
    public void GetOtherParticipantId_WithParticipant1Id_ShouldReturnParticipant2Id()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Act
        var result = conversation.GetOtherParticipantId(Participant1Id);

        // Assert
        result.Should().Be(Participant2Id);
    }

    [Fact]
    public void GetOtherParticipantId_WithParticipant2Id_ShouldReturnParticipant1Id()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Act
        var result = conversation.GetOtherParticipantId(Participant2Id);

        // Assert
        result.Should().Be(Participant1Id);
    }

    [Fact]
    public void GetOtherParticipantId_WithUnrelatedId_ShouldThrow()
    {
        // Arrange
        var conversation = Conversation.Create(Participant1Id, Participant2Id);

        // Act & Assert
        var act = () => conversation.GetOtherParticipantId(UnrelatedUserId);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion
}
