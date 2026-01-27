using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.Commands.UpdateTaskStatus;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Tasks;

/// <summary>
/// Unit tests for UpdateTaskStatusCommandHandler.
/// Story: ACF-008
/// </summary>
[Trait("Story", "ACF-008")]
public class UpdateTaskStatusCommandHandlerTests
{
    private readonly Mock<ITaskAssignmentRepository> _taskRepositoryMock;
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<ILogger<UpdateTaskStatusCommandHandler>> _loggerMock;
    private readonly UpdateTaskStatusCommandHandler _handler;

    private readonly Guid _taskId = Guid.NewGuid();
    private readonly Guid _assigneeId = Guid.NewGuid();
    private readonly Guid _assignedById = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();

    public UpdateTaskStatusCommandHandlerTests()
    {
        _taskRepositoryMock = new Mock<ITaskAssignmentRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<UpdateTaskStatusCommandHandler>>();

        _handler = new UpdateTaskStatusCommandHandler(
            _taskRepositoryMock.Object,
            _teamRepositoryMock.Object,
            _loggerMock.Object);

        SetupValidTeam();
    }

    [Fact]
    public async Task HandleByAssigneeShouldSucceed()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new UpdateTaskStatusCommand(_taskId, AssignmentStatus.InProgress, _assigneeId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(AssignmentStatus.InProgress);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        _taskRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleByAssignerShouldSucceed()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new UpdateTaskStatusCommand(_taskId, AssignmentStatus.InProgress, _assignedById);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(AssignmentStatus.InProgress);
    }

    [Fact]
    public async Task HandleToCompletedShouldSetCompletedAt()
    {
        // Arrange
        var task = CreateTestTask();
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new UpdateTaskStatusCommand(_taskId, AssignmentStatus.Completed, _assigneeId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(AssignmentStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleWithNonexistentTaskShouldThrow()
    {
        // Arrange
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskAssignment?)null);

        var command = new UpdateTaskStatusCommand(_taskId, AssignmentStatus.InProgress, _assigneeId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TaskAssignmentNotFoundException>();
    }

    [Fact]
    public async Task HandleByUnauthorizedUserShouldThrow()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var unauthorizedUserId = Guid.NewGuid();
        var command = new UpdateTaskStatusCommand(_taskId, AssignmentStatus.InProgress, unauthorizedUserId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TaskAccessDeniedException>();
    }

    [Fact]
    public async Task HandleByTeamAdminShouldSucceed()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Setup team where the new user is an admin
        var adminId = Guid.NewGuid();
        var team = Team.Create(adminId, "Test Team", null);
        var inviteToken = team.InviteMember("assignee@example.com", TeamRole.Editor, adminId);
        team.AcceptInvitation(inviteToken, _assigneeId, "assignee@example.com");

        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new UpdateTaskStatusCommand(_taskId, AssignmentStatus.InProgress, adminId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(AssignmentStatus.InProgress);
    }

    private TaskAssignment CreateTestTask()
    {
        var contentItemId = Guid.NewGuid();
        var task = TaskAssignment.Create(
            contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            DateTime.UtcNow.AddDays(7),
            "Test task");

        // Use reflection to set the Id for testing
        typeof(TaskAssignment).GetProperty("Id")!.DeclaringType!
            .GetProperty("Id")!.SetValue(task, _taskId);

        return task;
    }

    private void SetupValidTeam()
    {
        var team = Team.Create(_assignedById, "Test Team", null);
        var inviteToken = team.InviteMember("assignee@example.com", TeamRole.Editor, _assignedById);
        team.AcceptInvitation(inviteToken, _assigneeId, "assignee@example.com");

        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
    }
}
