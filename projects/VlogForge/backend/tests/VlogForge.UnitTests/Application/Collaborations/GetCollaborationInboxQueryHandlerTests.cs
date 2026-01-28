using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Collaborations.Queries.GetCollaborationInbox;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.UnitTests.Application.Collaborations;

/// <summary>
/// Unit tests for GetCollaborationInboxQueryHandler.
/// Story: ACF-011
/// </summary>
[Trait("Story", "ACF-011")]
public class GetCollaborationInboxQueryHandlerTests
{
    private readonly Mock<ICollaborationRequestRepository> _collaborationRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly GetCollaborationInboxQueryHandler _handler;

    private static readonly Guid RecipientId = Guid.NewGuid();
    private static readonly Guid Sender1Id = Guid.NewGuid();
    private static readonly Guid Sender2Id = Guid.NewGuid();

    public GetCollaborationInboxQueryHandlerTests()
    {
        _collaborationRepoMock = new Mock<ICollaborationRequestRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();

        _handler = new GetCollaborationInboxQueryHandler(
            _collaborationRepoMock.Object,
            _profileRepoMock.Object,
            new Mock<ILogger<GetCollaborationInboxQueryHandler>>().Object);

        SetupProfiles();
    }

    [Fact]
    public async Task Handle_ShouldReturnReceivedRequests()
    {
        // Arrange
        var requests = new List<CollaborationRequest>
        {
            CollaborationRequest.Create(Sender1Id, RecipientId, "Collab idea 1"),
            CollaborationRequest.Create(Sender2Id, RecipientId, "Collab idea 2")
        };

        _collaborationRepoMock.Setup(x => x.GetReceivedRequestsAsync(
            RecipientId, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((requests.AsReadOnly() as IReadOnlyList<CollaborationRequest>, 2));

        var query = new GetCollaborationInboxQuery(RecipientId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        _collaborationRepoMock.Setup(x => x.GetReceivedRequestsAsync(
            RecipientId, CollaborationRequestStatus.Pending, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<CollaborationRequest>() as IReadOnlyList<CollaborationRequest>, 0));

        var query = new GetCollaborationInboxQuery(RecipientId, CollaborationRequestStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        _collaborationRepoMock.Verify(
            x => x.GetReceivedRequestsAsync(
                RecipientId, CollaborationRequestStatus.Pending, 1, 20, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoRequests()
    {
        // Arrange
        _collaborationRepoMock.Setup(x => x.GetReceivedRequestsAsync(
            RecipientId, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<CollaborationRequest>() as IReadOnlyList<CollaborationRequest>, 0));

        var query = new GetCollaborationInboxQuery(RecipientId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    private void SetupProfiles()
    {
        var profiles = new Dictionary<Guid, CreatorProfile>
        {
            { Sender1Id, CreatorProfile.Create(Sender1Id, $"sender1_{Sender1Id.ToString()[..6]}", "Sender One") },
            { Sender2Id, CreatorProfile.Create(Sender2Id, $"sender2_{Sender2Id.ToString()[..6]}", "Sender Two") },
            { RecipientId, CreatorProfile.Create(RecipientId, $"recip_{RecipientId.ToString()[..6]}", "Recipient") }
        };

        _profileRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid userId, CancellationToken _) =>
                profiles.TryGetValue(userId, out var p) ? p : null);
    }
}
