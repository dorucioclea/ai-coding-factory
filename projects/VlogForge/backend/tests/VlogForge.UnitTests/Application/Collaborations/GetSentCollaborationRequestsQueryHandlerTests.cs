using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Collaborations.Queries.GetSentCollaborationRequests;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.UnitTests.Application.Collaborations;

/// <summary>
/// Unit tests for GetSentCollaborationRequestsQueryHandler.
/// Story: ACF-011
/// </summary>
[Trait("Story", "ACF-011")]
public class GetSentCollaborationRequestsQueryHandlerTests
{
    private readonly Mock<ICollaborationRequestRepository> _collaborationRepoMock;
    private readonly Mock<ICreatorProfileRepository> _profileRepoMock;
    private readonly GetSentCollaborationRequestsQueryHandler _handler;

    private static readonly Guid SenderId = Guid.NewGuid();
    private static readonly Guid Recipient1Id = Guid.NewGuid();
    private static readonly Guid Recipient2Id = Guid.NewGuid();

    public GetSentCollaborationRequestsQueryHandlerTests()
    {
        _collaborationRepoMock = new Mock<ICollaborationRequestRepository>();
        _profileRepoMock = new Mock<ICreatorProfileRepository>();

        _handler = new GetSentCollaborationRequestsQueryHandler(
            _collaborationRepoMock.Object,
            _profileRepoMock.Object,
            new Mock<ILogger<GetSentCollaborationRequestsQueryHandler>>().Object);

        SetupProfiles();
    }

    [Fact]
    public async Task Handle_ShouldReturnSentRequests()
    {
        // Arrange
        var requests = new List<CollaborationRequest>
        {
            CollaborationRequest.Create(SenderId, Recipient1Id, "Collab idea 1"),
            CollaborationRequest.Create(SenderId, Recipient2Id, "Collab idea 2")
        };

        _collaborationRepoMock.Setup(x => x.GetSentRequestsAsync(
            SenderId, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((requests.AsReadOnly() as IReadOnlyList<CollaborationRequest>, 2));

        var query = new GetSentCollaborationRequestsQuery(SenderId);

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
        _collaborationRepoMock.Setup(x => x.GetSentRequestsAsync(
            SenderId, CollaborationRequestStatus.Accepted, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<CollaborationRequest>() as IReadOnlyList<CollaborationRequest>, 0));

        var query = new GetSentCollaborationRequestsQuery(SenderId, CollaborationRequestStatus.Accepted);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        _collaborationRepoMock.Verify(
            x => x.GetSentRequestsAsync(
                SenderId, CollaborationRequestStatus.Accepted, 1, 20, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoRequests()
    {
        // Arrange
        _collaborationRepoMock.Setup(x => x.GetSentRequestsAsync(
            SenderId, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<CollaborationRequest>() as IReadOnlyList<CollaborationRequest>, 0));

        var query = new GetSentCollaborationRequestsQuery(SenderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldPassPageParameters()
    {
        // Arrange
        var requests = new List<CollaborationRequest>
        {
            CollaborationRequest.Create(SenderId, Recipient1Id, "Page 2 collab")
        };

        _collaborationRepoMock.Setup(x => x.GetSentRequestsAsync(
            SenderId, null, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((requests.AsReadOnly() as IReadOnlyList<CollaborationRequest>, 15));

        var query = new GetSentCollaborationRequestsQuery(SenderId, Page: 2, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        _collaborationRepoMock.Verify(
            x => x.GetSentRequestsAsync(SenderId, null, 2, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupProfiles()
    {
        var profiles = new Dictionary<Guid, CreatorProfile>
        {
            { SenderId, CreatorProfile.Create(SenderId, $"sender_{SenderId.ToString()[..6]}", "The Sender") },
            { Recipient1Id, CreatorProfile.Create(Recipient1Id, $"recip1_{Recipient1Id.ToString()[..6]}", "Recipient One") },
            { Recipient2Id, CreatorProfile.Create(Recipient2Id, $"recip2_{Recipient2Id.ToString()[..6]}", "Recipient Two") }
        };

        _profileRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid userId, CancellationToken _) =>
                profiles.TryGetValue(userId, out var p) ? p : null);
    }
}
