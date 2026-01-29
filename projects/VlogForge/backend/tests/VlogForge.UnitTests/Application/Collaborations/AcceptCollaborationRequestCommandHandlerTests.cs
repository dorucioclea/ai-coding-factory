using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Collaborations.Commands.AcceptCollaborationRequest;
using Xunit;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.UnitTests.Application.Collaborations;

/// <summary>
/// Unit tests for AcceptCollaborationRequestCommandHandler.
/// Story: ACF-011
/// </summary>
[Trait("Story", "ACF-011")]
public class AcceptCollaborationRequestCommandHandlerTests
{
    private readonly Mock<ICollaborationRequestRepository> _collaborationRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly AcceptCollaborationRequestCommandHandler _handler;

    private static readonly Guid SenderId = Guid.NewGuid();
    private static readonly Guid RecipientId = Guid.NewGuid();

    public AcceptCollaborationRequestCommandHandlerTests()
    {
        _collaborationRepoMock = new Mock<ICollaborationRequestRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();

        _handler = new AcceptCollaborationRequestCommandHandler(
            _collaborationRepoMock.Object,
            _profileRepoMock.Object,
            new Mock<ILogger<AcceptCollaborationRequestCommandHandler>>().Object);

        SetupProfiles();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldAccept()
    {
        // Arrange
        var collabRequest = CollaborationRequest.Create(SenderId, RecipientId, "Let's work together!");
        _collaborationRepoMock.Setup(x => x.GetByIdAsync(collabRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collabRequest);

        var command = new AcceptCollaborationRequestCommand(collabRequest.Id, RecipientId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CollaborationRequestStatus.Accepted);
        result.RespondedAt.Should().NotBeNull();

        _collaborationRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<CollaborationRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _collaborationRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestNotFound_ShouldThrow()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        _collaborationRepoMock.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CollaborationRequest?)null);

        var command = new AcceptCollaborationRequestCommand(requestId, RecipientId);

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

        var command = new AcceptCollaborationRequestCommand(collabRequest.Id, SenderId);

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
