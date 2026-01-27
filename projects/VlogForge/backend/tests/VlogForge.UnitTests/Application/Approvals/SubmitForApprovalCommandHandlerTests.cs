using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Approvals.Commands.SubmitForApproval;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using VlogForge.Domain.Interfaces;
using Xunit;

namespace VlogForge.UnitTests.Application.Approvals;

/// <summary>
/// Unit tests for SubmitForApprovalCommandHandler.
/// Story: ACF-009
/// </summary>
[Trait("Story", "ACF-009")]
public class SubmitForApprovalCommandHandlerTests
{
    private readonly Mock<IContentItemRepository> _contentRepositoryMock;
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<IApprovalRecordRepository> _approvalRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<SubmitForApprovalCommandHandler>> _loggerMock;
    private readonly SubmitForApprovalCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();
    private readonly Guid _contentId = Guid.NewGuid();

    public SubmitForApprovalCommandHandlerTests()
    {
        _contentRepositoryMock = new Mock<IContentItemRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _approvalRepositoryMock = new Mock<IApprovalRecordRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<SubmitForApprovalCommandHandler>>();
        _handler = new SubmitForApprovalCommandHandler(
            _contentRepositoryMock.Object,
            _teamRepositoryMock.Object,
            _approvalRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithDraftContentShouldSubmitForApproval()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_userId, IdeaStatus.Draft);
        var team = CreateTeamWithWorkflow(_userId);
        SetupRepositories(contentItem, team);

        var command = new SubmitForApprovalCommand(contentItem.Id, _teamId, _userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(IdeaStatus.InReview);

        _approvalRepositoryMock.Verify(x => x.AddAsync(
            It.Is<ApprovalRecord>(r => r.Action == ApprovalAction.Submitted),
            It.IsAny<CancellationToken>()), Times.Once);
        _contentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ContentItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithChangesRequestedContentShouldResubmit()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_userId, IdeaStatus.ChangesRequested);
        var team = CreateTeamWithWorkflow(_userId);
        SetupRepositories(contentItem, team);

        var command = new SubmitForApprovalCommand(contentItem.Id, _teamId, _userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(IdeaStatus.InReview);

        _approvalRepositoryMock.Verify(x => x.AddAsync(
            It.Is<ApprovalRecord>(r => r.Action == ApprovalAction.Resubmitted),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNonExistentContentShouldThrow()
    {
        // Arrange
        _contentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);

        var command = new SubmitForApprovalCommand(_contentId, _teamId, _userId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ContentItemNotFoundException>();
    }

    [Fact]
    public async Task HandleByNonOwnerShouldThrow()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var contentItem = CreateContentItemInStatus(ownerId, IdeaStatus.Draft);
        var team = CreateTeamWithWorkflow(ownerId);
        SetupRepositories(contentItem, team);

        var nonOwnerId = Guid.NewGuid();
        var command = new SubmitForApprovalCommand(contentItem.Id, _teamId, nonOwnerId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*content owner*");
    }

    [Fact]
    public async Task HandleWithNonExistentTeamShouldThrow()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_userId, IdeaStatus.Draft);
        _contentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentItem);
        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new SubmitForApprovalCommand(contentItem.Id, _teamId, _userId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamNotFoundException>();
    }

    [Fact]
    public async Task HandleByNonMemberShouldThrow()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_userId, IdeaStatus.Draft);
        var differentOwnerId = Guid.NewGuid();
        var team = CreateTeamWithWorkflow(differentOwnerId); // User is not a member
        SetupRepositories(contentItem, team);

        var command = new SubmitForApprovalCommand(contentItem.Id, _teamId, _userId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamAccessDeniedException>();
    }

    [Fact]
    public async Task HandleWhenWorkflowNotEnabledShouldThrow()
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_userId, IdeaStatus.Draft);
        var team = Team.Create(_userId, "Test Team"); // Workflow not enabled
        SetupRepositories(contentItem, team);

        var command = new SubmitForApprovalCommand(contentItem.Id, _teamId, _userId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.RuleName == "WorkflowNotEnabled");
    }

    [Theory]
    [InlineData(IdeaStatus.Idea)]
    [InlineData(IdeaStatus.InReview)]
    [InlineData(IdeaStatus.Approved)]
    [InlineData(IdeaStatus.Scheduled)]
    [InlineData(IdeaStatus.Published)]
    public async Task HandleWithInvalidStatusShouldThrow(IdeaStatus status)
    {
        // Arrange
        var contentItem = CreateContentItemInStatus(_userId, status);
        var team = CreateTeamWithWorkflow(_userId);
        SetupRepositories(contentItem, team);

        var command = new SubmitForApprovalCommand(contentItem.Id, _teamId, _userId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.RuleName == "InvalidStatus");
    }

    private void SetupRepositories(ContentItem contentItem, Team team)
    {
        _contentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentItem);
        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
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
}
