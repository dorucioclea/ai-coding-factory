using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Collaborations.Commands.SendCollaborationRequest;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.UnitTests.Application.Collaborations;

/// <summary>
/// Unit tests for SendCollaborationRequestCommandHandler.
/// Story: ACF-011
/// </summary>
[Trait("Story", "ACF-011")]
public class SendCollaborationRequestCommandHandlerTests
{
    private readonly Mock<ICollaborationRequestRepository> _collaborationRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly SendCollaborationRequestCommandHandler _handler;

    private static readonly Guid SenderId = Guid.NewGuid();
    private static readonly Guid RecipientId = Guid.NewGuid();
    private const string ValidMessage = "Let's collaborate on a video!";

    public SendCollaborationRequestCommandHandlerTests()
    {
        _collaborationRepoMock = new Mock<ICollaborationRequestRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();

        _handler = new SendCollaborationRequestCommandHandler(
            _collaborationRepoMock.Object,
            _profileRepoMock.Object,
            new Mock<ILogger<SendCollaborationRequestCommandHandler>>().Object);

        SetupValidProfiles();
        SetupNoDuplicates();
        SetupNoRateLimit();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateRequest()
    {
        // Arrange
        var command = new SendCollaborationRequestCommand(SenderId, RecipientId, ValidMessage);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SenderId.Should().Be(SenderId);
        result.RecipientId.Should().Be(RecipientId);
        result.Message.Should().Be(ValidMessage);
        result.Status.Should().Be(CollaborationRequestStatus.Pending);

        _collaborationRepoMock.Verify(
            x => x.AddAsync(It.IsAny<CollaborationRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _collaborationRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSenderHasNoProfile_ShouldThrow()
    {
        // Arrange
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(SenderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        var command = new SendCollaborationRequestCommand(SenderId, RecipientId, ValidMessage);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRecipientHasNoProfile_ShouldThrow()
    {
        // Arrange
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(RecipientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        var command = new SendCollaborationRequestCommand(SenderId, RecipientId, ValidMessage);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRecipientNotOpenToCollaborations_ShouldThrow()
    {
        // Arrange
        var closedProfile = CreateProfile(RecipientId, openToCollab: false);
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(RecipientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedProfile);

        var command = new SendCollaborationRequestCommand(SenderId, RecipientId, ValidMessage);

        // Act & Assert
        await Assert.ThrowsAsync<RecipientNotOpenToCollaborationsException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRateLimitExceeded_ShouldThrow()
    {
        // Arrange
        _collaborationRepoMock.Setup(x => x.CountSentTodayAsync(SenderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CollaborationRequest.MaxRequestsPerDayPerUser);

        var command = new SendCollaborationRequestCommand(SenderId, RecipientId, ValidMessage);

        // Act & Assert
        await Assert.ThrowsAsync<CollaborationRateLimitExceededException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenDuplicatePendingExists_ShouldThrow()
    {
        // Arrange
        _collaborationRepoMock.Setup(x => x.ExistsPendingBetweenAsync(
            SenderId, RecipientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new SendCollaborationRequestCommand(SenderId, RecipientId, ValidMessage);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateCollaborationRequestException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldPopulateSenderAndRecipientInfo()
    {
        // Arrange
        var command = new SendCollaborationRequestCommand(SenderId, RecipientId, ValidMessage);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SenderDisplayName.Should().NotBeEmpty();
        result.SenderUsername.Should().NotBeEmpty();
        result.RecipientDisplayName.Should().NotBeEmpty();
        result.RecipientUsername.Should().NotBeEmpty();
    }

    private void SetupValidProfiles()
    {
        var senderProfile = CreateProfile(SenderId, openToCollab: true);
        var recipientProfile = CreateProfile(RecipientId, openToCollab: true);

        _profileRepoMock.Setup(x => x.GetByUserIdAsync(SenderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(senderProfile);
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(RecipientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipientProfile);
    }

    private void SetupNoDuplicates()
    {
        _collaborationRepoMock.Setup(x => x.ExistsPendingBetweenAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupNoRateLimit()
    {
        _collaborationRepoMock.Setup(x => x.CountSentTodayAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    private static CreatorProfile CreateProfile(Guid userId, bool openToCollab)
    {
        var username = $"user_{userId.ToString()[..8]}";
        var profile = CreatorProfile.Create(userId, username, $"Display {username}");
        profile.SetCollaborationSettings(openToCollab);
        return profile;
    }
}
