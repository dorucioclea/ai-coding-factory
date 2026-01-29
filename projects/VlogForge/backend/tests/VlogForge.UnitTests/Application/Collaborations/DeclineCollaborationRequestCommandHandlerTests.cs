using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Collaborations.Commands.DeclineCollaborationRequest;
using Xunit;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.UnitTests.Application.Collaborations;

/// <summary>
/// Unit tests for DeclineCollaborationRequestCommandHandler.
/// Story: ACF-011
/// </summary>
[Trait("Story", "ACF-011")]
public class DeclineCollaborationRequestCommandHandlerTests
{
    private readonly Mock<ICollaborationRequestRepository> _collaborationRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly DeclineCollaborationRequestCommandHandler _handler;

    private static readonly Guid SenderId = Guid.NewGuid();
    private static readonly Guid RecipientId = Guid.NewGuid();

    public DeclineCollaborationRequestCommandHandlerTests()
    {
        _collaborationRepoMock = new Mock<ICollaborationRequestRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();

        _handler = new DeclineCollaborationRequestCommandHandler(
            _collaborationRepoMock.Object,
            _profileRepoMock.Object,
            new Mock<ILogger<DeclineCollaborationRequestCommandHandler>>().Object);

        SetupProfiles();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldDecline()
    {
        // Arrange
        var collabRequest = CollaborationRequest.Create(SenderId, RecipientId, "Let's work together!");
        _collaborationRepoMock.Setup(x => x.GetByIdAsync(collabRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collabRequest);

        var command = new DeclineCollaborationRequestCommand(collabRequest.Id, RecipientId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CollaborationRequestStatus.Declined);
        result.DeclineReason.Should().BeNull();

        _collaborationRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithReason_ShouldStoreDeclineReason()
    {
        // Arrange
        var collabRequest = CollaborationRequest.Create(SenderId, RecipientId, "Let's work together!");
        _collaborationRepoMock.Setup(x => x.GetByIdAsync(collabRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collabRequest);

        var reason = "Not interested at this time";
        var command = new DeclineCollaborationRequestCommand(collabRequest.Id, RecipientId, reason);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.DeclineReason.Should().Be(reason);
    }

    [Fact]
    public async Task Handle_WhenRequestNotFound_ShouldThrow()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        _collaborationRepoMock.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CollaborationRequest?)null);

        var command = new DeclineCollaborationRequestCommand(requestId, RecipientId);

        // Act & Assert
        await Assert.ThrowsAsync<CollaborationRequestNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_BySender_ShouldThrowUnauthorized()
    {
        // Arrange
        var collabRequest = CollaborationRequest.Create(SenderId, RecipientId, "Let's work together!");
        _collaborationRepoMock.Setup(x => x.GetByIdAsync(collabRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collabRequest);

        var command = new DeclineCollaborationRequestCommand(collabRequest.Id, SenderId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    private void SetupProfiles()
    {
        var senderProfile = CreatorProfile.Create(SenderId, $"sender_{SenderId.ToString()[..8]}", "Sender");
        var recipientProfile = CreatorProfile.Create(RecipientId, $"recipient_{RecipientId.ToString()[..8]}", "Recipient");

        _profileRepoMock.Setup(x => x.GetByUserIdAsync(SenderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(senderProfile);
        _profileRepoMock.Setup(x => x.GetByUserIdAsync(RecipientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipientProfile);
    }
}
