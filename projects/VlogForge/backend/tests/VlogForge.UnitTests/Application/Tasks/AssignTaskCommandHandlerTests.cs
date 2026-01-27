using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.Commands.AssignTask;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Tasks;

/// <summary>
/// Unit tests for AssignTaskCommandHandler.
/// Story: ACF-008
/// </summary>
[Trait("Story", "ACF-008")]
public class AssignTaskCommandHandlerTests
{
    private readonly Mock<ITaskAssignmentRepository> _taskRepositoryMock;
    private readonly Mock<IContentItemRepository> _contentRepositoryMock;
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<ILogger<AssignTaskCommandHandler>> _loggerMock;
    private readonly AssignTaskCommandHandler _handler;

    private readonly Guid _contentItemId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();
    private readonly Guid _assigneeId = Guid.NewGuid();
    private readonly Guid _assignedById = Guid.NewGuid();
    private readonly DateTime _dueDate = DateTime.UtcNow.AddDays(7);

    public AssignTaskCommandHandlerTests()
    {
        _taskRepositoryMock = new Mock<ITaskAssignmentRepository>();
        _contentRepositoryMock = new Mock<IContentItemRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<AssignTaskCommandHandler>>();

        _handler = new AssignTaskCommandHandler(
            _taskRepositoryMock.Object,
            _contentRepositoryMock.Object,
            _teamRepositoryMock.Object,
            _loggerMock.Object);

        // Default setup
        SetupValidContentItem();
        SetupValidTeam();
        _taskRepositoryMock.Setup(x => x.ExistsActiveForContentItemAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task HandleWithValidDataShouldCreateTaskAssignment()
    {
        // Arrange
        var command = new AssignTaskCommand(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            "Test notes");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ContentItemId.Should().Be(_contentItemId);
        result.TeamId.Should().Be(_teamId);
        result.AssigneeId.Should().Be(_assigneeId);
        result.AssignedById.Should().Be(_assignedById);
        result.Notes.Should().Be("Test notes");
        result.Status.Should().Be(AssignmentStatus.NotStarted);

        _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TaskAssignment>(), It.IsAny<CancellationToken>()), Times.Once);
        _taskRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNonexistentContentItemShouldThrow()
    {
        // Arrange
        _contentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);

        var command = new AssignTaskCommand(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ContentItemNotFoundException>();
    }

    [Fact]
    public async Task HandleWithNonexistentTeamShouldThrow()
    {
        // Arrange
        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new AssignTaskCommand(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamNotFoundException>();
    }

    [Fact]
    public async Task HandleWithUnauthorizedUserShouldThrow()
    {
        // Arrange
        var unauthorizedUserId = Guid.NewGuid();
        var command = new AssignTaskCommand(
            _contentItemId,
            _teamId,
            _assigneeId,
            unauthorizedUserId, // Not a team member with permission
            _dueDate,
            null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamAccessDeniedException>();
    }

    [Fact]
    public async Task HandleWithNonMemberAssigneeShouldThrow()
    {
        // Arrange
        var nonMemberId = Guid.NewGuid();
        var command = new AssignTaskCommand(
            _contentItemId,
            _teamId,
            nonMemberId, // Not a team member
            _assignedById,
            _dueDate,
            null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamMemberNotFoundException>();
    }

    [Fact]
    public async Task HandleWithExistingActiveAssignmentShouldThrow()
    {
        // Arrange
        _taskRepositoryMock.Setup(x => x.ExistsActiveForContentItemAsync(_contentItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new AssignTaskCommand(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already has an active task assignment*");
    }

    private void SetupValidContentItem()
    {
        var contentItem = ContentItem.Create(Guid.NewGuid(), "Test Content", "Notes");
        _contentRepositoryMock.Setup(x => x.GetByIdAsync(_contentItemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentItem);
    }

    private void SetupValidTeam()
    {
        var team = Team.Create(_assignedById, "Test Team", "Description");

        // Add assignee as a member
        var inviteToken = team.InviteMember("assignee@example.com", TeamRole.Editor, _assignedById);
        team.AcceptInvitation(inviteToken, _assigneeId, "assignee@example.com");

        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
    }
}
