using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Approvals.Commands.RequestChanges;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using VlogForge.Domain.Interfaces;
using Xunit;

namespace VlogForge.UnitTests.Application.Approvals;

/// <summary>
/// Unit tests for RequestChangesCommandHandler.
/// Story: ACF-009
/// </summary>
[Trait("Story", "ACF-009")]
public class RequestChangesCommandHandlerTests
{
    private readonly Mock<IContentItemRepository> _contentRepositoryMock;
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<IApprovalRecordRepository> _approvalRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RequestChangesCommandHandler>> _loggerMock;
    private readonly RequestChangesCommandHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();

    public RequestChangesCommandHandlerTests()
    {
        _contentRepositoryMock = new Mock<IContentItemRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _approvalRepositoryMock = new Mock<IApprovalRecordRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RequestChangesCommandHandler>>();
        _handler = new RequestChangesCommandHandler(
            _contentRepositoryMock.Object,
            _teamRepositoryMock.Object,
            _approvalRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleShouldRequestChanges()
    {
        // Arrange
        var contentCreatorId = Guid.NewGuid();
        var contentItem = CreateContentItemInStatus(contentCreatorId, IdeaStatus.InReview);
        var team = CreateTeamWithWorkflow(_ownerId);
        SetupRepositories(contentItem, team);

        var command = new RequestChangesCommand(contentItem.Id, _teamId, _ownerId, "Please improve the intro section");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(IdeaStatus.ChangesRequested);

        _approvalRepositoryMock.Verify(x => x.AddAsync(
            It.Is<ApprovalRecord>(r =>
                r.Action == ApprovalAction.ChangesRequested &&
                r.Feedback == "Please improve the intro section"),
            It.IsAny<CancellationToken>()), Times.Once);
        _contentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ContentItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNonExistentContentShouldThrow()
    {
        // Arrange
        _contentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);

        var command = new RequestChangesCommand(Guid.NewGuid(), _teamId, _ownerId, "Feedback");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ContentItemNotFoundException>();
    }

    [Fact]
    public async Task HandleWithNonExistentTeamShouldThrow()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_ownerId, IdeaStatus.InReview);
        _contentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentItem);
        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new RequestChangesCommand(contentItem.Id, _teamId, _ownerId, "Feedback");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamNotFoundException>();
    }

    [Fact]
    public async Task HandleByNonReviewerShouldThrow()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_ownerId, IdeaStatus.InReview);
        var team = CreateTeamWithWorkflow(_ownerId);
        var viewerId = AddMemberToTeam(team, TeamRole.Viewer);
        SetupRepositories(contentItem, team);

        var command = new RequestChangesCommand(contentItem.Id, _teamId, viewerId, "Feedback");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamAccessDeniedException>()
            .WithMessage("*permission to request changes*");
    }

    [Fact]
    public async Task HandleByDesignatedApproverShouldSucceed()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_ownerId, IdeaStatus.InReview);
        var team = Team.Create(_ownerId, "Test Team");
        var editorId = AddMemberToTeam(team, TeamRole.Editor);
        team.ConfigureWorkflow(true, new[] { editorId }, _ownerId);
        SetupRepositories(contentItem, team);

        var command = new RequestChangesCommand(contentItem.Id, _teamId, editorId, "Needs work");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(IdeaStatus.ChangesRequested);
    }

    [Fact]
    public async Task HandleWithContentNotSubmittedToTeamShouldThrow()
    {
        // Arrange - content submitted to different team
        var contentItem = CreateContentItemInStatus(_ownerId, IdeaStatus.InReview);
        var team = CreateTeamWithWorkflow(_ownerId);
        var differentTeamId = Guid.NewGuid();

        // Create approval record for a different team
        var approvalRecord = ApprovalRecord.Create(
            contentItem.Id,
            differentTeamId, // Different team!
            contentItem.UserId,
            ApprovalAction.Submitted,
            IdeaStatus.Draft,
            IdeaStatus.InReview,
            null);

        SetupRepositories(contentItem, team, approvalRecord);

        var command = new RequestChangesCommand(contentItem.Id, _teamId, _ownerId, "Feedback");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert - should throw because content wasn't submitted to this team
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.RuleName == "InvalidTeam");
    }

    [Theory]
    [InlineData(IdeaStatus.Idea)]
    [InlineData(IdeaStatus.Draft)]
    [InlineData(IdeaStatus.Approved)]
    [InlineData(IdeaStatus.ChangesRequested)]
    [InlineData(IdeaStatus.Scheduled)]
    [InlineData(IdeaStatus.Published)]
    public async Task HandleWithInvalidStatusShouldThrow(IdeaStatus status)
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_ownerId, status);
        var team = CreateTeamWithWorkflow(_ownerId);
        SetupRepositories(contentItem, team);

        var command = new RequestChangesCommand(contentItem.Id, _teamId, _ownerId, "Feedback");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.RuleName == "InvalidStatus");
    }

    [Fact]
    public async Task HandleWithNullFeedbackShouldSucceed()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_ownerId, IdeaStatus.InReview);
        var team = CreateTeamWithWorkflow(_ownerId);
        SetupRepositories(contentItem, team);

        var command = new RequestChangesCommand(contentItem.Id, _teamId, _ownerId, string.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(IdeaStatus.ChangesRequested);
    }

    private void SetupRepositories(ContentItem contentItem, Team team, ApprovalRecord? approvalRecord = null)
    {
        _contentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentItem);
        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Setup approval record - by default create one matching the team
        if (approvalRecord is null)
        {
            approvalRecord = ApprovalRecord.Create(
                contentItem.Id,
                _teamId,
                contentItem.UserId,
                ApprovalAction.Submitted,
                IdeaStatus.Draft,
                IdeaStatus.InReview,
                null);
        }
        _approvalRepositoryMock.Setup(x => x.GetLatestByContentItemIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalRecord);
    }

    private static ContentItem CreateContentItemInStatus(Guid userId, IdeaStatus status)
    {
        var item = ContentItem.Create(userId, "Test Content", "Notes");

        var path = status switch
        {
            IdeaStatus.Idea => Array.Empty<IdeaStatus>(),
            IdeaStatus.Draft => new[] { IdeaStatus.Draft },
            IdeaStatus.InReview => new[] { IdeaStatus.Draft, IdeaStatus.InReview },
            IdeaStatus.Approved => new[] { IdeaStatus.Draft, IdeaStatus.InReview, IdeaStatus.Approved },
            IdeaStatus.ChangesRequested => new[] { IdeaStatus.Draft, IdeaStatus.InReview, IdeaStatus.ChangesRequested },
            IdeaStatus.Scheduled => new[] { IdeaStatus.Draft, IdeaStatus.InReview, IdeaStatus.Approved, IdeaStatus.Scheduled },
            IdeaStatus.Published => new[] { IdeaStatus.Draft, IdeaStatus.InReview, IdeaStatus.Approved, IdeaStatus.Scheduled, IdeaStatus.Published },
            _ => throw new ArgumentException($"Unknown status: {status}")
        };

        foreach (var s in path)
        {
            item.UpdateStatus(s);
        }

        return item;
    }

    private static Team CreateTeamWithWorkflow(Guid ownerId)
    {
        var team = Team.Create(ownerId, "Test Team");
        team.ConfigureWorkflow(true, null, ownerId);
        return team;
    }

    private Guid AddMemberToTeam(Team team, TeamRole role)
    {
        var memberId = Guid.NewGuid();
        var email = $"member_{Guid.NewGuid():N}@example.com";
        var token = team.InviteMember(email, role, _ownerId);
        team.AcceptInvitation(token, memberId, email);
        return memberId;
    }
}
