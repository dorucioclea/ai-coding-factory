using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for CollaborationRequest aggregate root.
/// Story: ACF-011
/// </summary>
[Trait("Story", "ACF-011")]
public class CollaborationRequestTests
{
    private static readonly Guid SenderId = Guid.NewGuid();
    private static readonly Guid RecipientId = Guid.NewGuid();
    private const string ValidMessage = "I'd love to collaborate on a gaming video!";

    #region Create

    [Fact]
    public void Create_WithValidData_ShouldReturnCollaborationRequest()
    {
        // Act
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Assert
        request.Should().NotBeNull();
        request.SenderId.Should().Be(SenderId);
        request.RecipientId.Should().Be(RecipientId);
        request.Message.Should().Be(ValidMessage);
        request.Status.Should().Be(CollaborationRequestStatus.Pending);
        request.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        request.RespondedAt.Should().BeNull();
        request.DeclineReason.Should().BeNull();
        request.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Act
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Assert
        request.DomainEvents.Should().HaveCount(1);
        request.DomainEvents.Should().ContainSingle(e => e is CollaborationRequestCreatedEvent);
        var evt = request.DomainEvents.OfType<CollaborationRequestCreatedEvent>().First();
        evt.SenderId.Should().Be(SenderId);
        evt.RecipientId.Should().Be(RecipientId);
    }

    [Fact]
    public void Create_WithEmptySenderId_ShouldThrow()
    {
        // Act & Assert
        var act = () => CollaborationRequest.Create(Guid.Empty, RecipientId, ValidMessage);
        act.Should().Throw<ArgumentException>().WithMessage("*Sender ID*");
    }

    [Fact]
    public void Create_WithEmptyRecipientId_ShouldThrow()
    {
        // Act & Assert
        var act = () => CollaborationRequest.Create(SenderId, Guid.Empty, ValidMessage);
        act.Should().Throw<ArgumentException>().WithMessage("*Recipient ID*");
    }

    [Fact]
    public void Create_WithSameIds_ShouldThrow()
    {
        // Act & Assert
        var act = () => CollaborationRequest.Create(SenderId, SenderId, ValidMessage);
        act.Should().Throw<ArgumentException>().WithMessage("*yourself*");
    }

    [Fact]
    public void Create_WithEmptyMessage_ShouldThrow()
    {
        // Act & Assert
        var act = () => CollaborationRequest.Create(SenderId, RecipientId, "");
        act.Should().Throw<ArgumentException>().WithMessage("*Message*");
    }

    [Fact]
    public void Create_WithTooLongMessage_ShouldThrow()
    {
        // Arrange
        var longMessage = new string('a', CollaborationRequest.MaxMessageLength + 1);

        // Act & Assert
        var act = () => CollaborationRequest.Create(SenderId, RecipientId, longMessage);
        act.Should().Throw<ArgumentException>().WithMessage("*1000*");
    }

    [Fact]
    public void Create_WithMessageAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxMessage = new string('a', CollaborationRequest.MaxMessageLength);

        // Act
        var request = CollaborationRequest.Create(SenderId, RecipientId, maxMessage);

        // Assert
        request.Message.Should().HaveLength(CollaborationRequest.MaxMessageLength);
    }

    [Fact]
    public void Create_ShouldTrimMessage()
    {
        // Act
        var request = CollaborationRequest.Create(SenderId, RecipientId, "  Hello  ");

        // Assert
        request.Message.Should().Be("Hello");
    }

    [Fact]
    public void Create_ShouldSetExpiresAtTo14Days()
    {
        // Act
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddDays(CollaborationRequest.ExpirationDays);
        request.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Accept

    [Fact]
    public void Accept_ByRecipient_ShouldChangeStatusToAccepted()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Act
        request.Accept(RecipientId);

        // Assert
        request.Status.Should().Be(CollaborationRequestStatus.Accepted);
        request.RespondedAt.Should().NotBeNull();
        request.RespondedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Accept_ShouldRaiseAcceptedEvent()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        request.ClearDomainEvents();

        // Act
        request.Accept(RecipientId);

        // Assert
        request.DomainEvents.Should().ContainSingle(e => e is CollaborationRequestAcceptedEvent);
    }

    [Fact]
    public void Accept_BySender_ShouldThrowUnauthorized()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Act & Assert
        var act = () => request.Accept(SenderId);
        act.Should().Throw<UnauthorizedAccessException>().WithMessage("*recipient*");
    }

    [Fact]
    public void Accept_WhenNotPending_ShouldThrow()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        request.Accept(RecipientId);

        // Act & Assert
        var act = () => request.Accept(RecipientId);
        act.Should().Throw<InvalidOperationException>().WithMessage("*status*");
    }

    #endregion

    #region Decline

    [Fact]
    public void Decline_ByRecipient_ShouldChangeStatusToDeclined()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Act
        request.Decline(RecipientId);

        // Assert
        request.Status.Should().Be(CollaborationRequestStatus.Declined);
        request.RespondedAt.Should().NotBeNull();
        request.DeclineReason.Should().BeNull();
    }

    [Fact]
    public void Decline_WithReason_ShouldStoreReason()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        var reason = "Not interested at this time";

        // Act
        request.Decline(RecipientId, reason);

        // Assert
        request.Status.Should().Be(CollaborationRequestStatus.Declined);
        request.DeclineReason.Should().Be(reason);
    }

    [Fact]
    public void Decline_ShouldRaiseDeclinedEvent()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        request.ClearDomainEvents();

        // Act
        request.Decline(RecipientId);

        // Assert
        request.DomainEvents.Should().ContainSingle(e => e is CollaborationRequestDeclinedEvent);
    }

    [Fact]
    public void Decline_BySender_ShouldThrowUnauthorized()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Act & Assert
        var act = () => request.Decline(SenderId);
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void Decline_WithTooLongReason_ShouldThrow()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        var longReason = new string('a', CollaborationRequest.MaxDeclineReasonLength + 1);

        // Act & Assert
        var act = () => request.Decline(RecipientId, longReason);
        act.Should().Throw<ArgumentException>().WithMessage("*500*");
    }

    [Fact]
    public void Decline_WhenAlreadyAccepted_ShouldThrow()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        request.Accept(RecipientId);

        // Act & Assert
        var act = () => request.Decline(RecipientId);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Withdraw

    [Fact]
    public void Withdraw_BySender_ShouldChangeStatusToWithdrawn()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Act
        request.Withdraw(SenderId);

        // Assert
        request.Status.Should().Be(CollaborationRequestStatus.Withdrawn);
        request.RespondedAt.Should().NotBeNull();
    }

    [Fact]
    public void Withdraw_ShouldRaiseWithdrawnEvent()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        request.ClearDomainEvents();

        // Act
        request.Withdraw(SenderId);

        // Assert
        request.DomainEvents.Should().ContainSingle(e => e is CollaborationRequestWithdrawnEvent);
    }

    [Fact]
    public void Withdraw_ByRecipient_ShouldThrowUnauthorized()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Act & Assert
        var act = () => request.Withdraw(RecipientId);
        act.Should().Throw<UnauthorizedAccessException>().WithMessage("*sender*");
    }

    [Fact]
    public void Withdraw_WhenNotPending_ShouldThrow()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        request.Accept(RecipientId);

        // Act & Assert
        var act = () => request.Withdraw(SenderId);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region MarkExpired

    [Fact]
    public void MarkExpired_WhenPending_ShouldChangeStatusToExpired()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        // Act
        request.MarkExpired();

        // Assert
        request.Status.Should().Be(CollaborationRequestStatus.Expired);
    }

    [Fact]
    public void MarkExpired_ShouldRaiseExpiredEvent()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        request.ClearDomainEvents();

        // Act
        request.MarkExpired();

        // Assert
        request.DomainEvents.Should().ContainSingle(e => e is CollaborationRequestExpiredEvent);
    }

    [Fact]
    public void MarkExpired_WhenAlreadyAccepted_ShouldNotChange()
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);
        request.Accept(RecipientId);
        request.ClearDomainEvents();

        // Act
        request.MarkExpired();

        // Assert
        request.Status.Should().Be(CollaborationRequestStatus.Accepted);
        request.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region State Transitions

    [Theory]
    [InlineData(CollaborationRequestStatus.Pending, true)]
    [InlineData(CollaborationRequestStatus.Accepted, false)]
    [InlineData(CollaborationRequestStatus.Declined, false)]
    [InlineData(CollaborationRequestStatus.Withdrawn, false)]
    public void IsActive_ReturnsCorrectValueForStatus(CollaborationRequestStatus expectedStatus, bool expectedActive)
    {
        // Arrange
        var request = CollaborationRequest.Create(SenderId, RecipientId, ValidMessage);

        switch (expectedStatus)
        {
            case CollaborationRequestStatus.Accepted:
                request.Accept(RecipientId);
                break;
            case CollaborationRequestStatus.Declined:
                request.Decline(RecipientId);
                break;
            case CollaborationRequestStatus.Withdrawn:
                request.Withdraw(SenderId);
                break;
        }

        // Assert
        request.IsActive.Should().Be(expectedActive);
    }

    #endregion
}
