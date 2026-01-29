using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.Queries.GetTaskById;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Tasks;

/// <summary>
/// Unit tests for GetTaskByIdQueryHandler with history support.
/// Covers ACF-014 AC5: Task details modal with comments and history.
/// Story: ACF-014
/// </summary>
[Trait("Story", "ACF-014")]
public class GetTaskByIdWithHistoryTests
{
    private readonly Mock<ITaskAssignmentRepository> _taskRepositoryMock;
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<ILogger<GetTaskByIdQueryHandler>> _loggerMock;
    private readonly GetTaskByIdQueryHandler _handler;

    private readonly Guid _taskId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();
    private readonly Guid _assignedById = Guid.NewGuid();

    public GetTaskByIdWithHistoryTests()
    {
        _taskRepositoryMock = new Mock<ITaskAssignmentRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<GetTaskByIdQueryHandler>>();
        _handler = new GetTaskByIdQueryHandler(
            _taskRepositoryMock.Object,
            _teamRepositoryMock.Object,
            _loggerMock.Object);

        SetupValidTeam();
    }

    [Fact]
    public async Task HandleWithIncludeHistoryShouldLoadCommentsAndHistory()
    {
        // Arrange
        var task = CreateTaskWithHistory();
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAndHistoryAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var query = new GetTaskByIdQuery(_taskId, _userId, IncludeComments: true, IncludeHistory: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.History.Should().NotBeEmpty();
        _taskRepositoryMock.Verify(x => x.GetByIdWithCommentsAndHistoryAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithHistoryOnlyShouldLoadHistoryWithoutComments()
    {
        // Arrange
        var task = CreateTaskWithHistory();
        _taskRepositoryMock.Setup(x => x.GetByIdWithHistoryAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var query = new GetTaskByIdQuery(_taskId, _userId, IncludeComments: false, IncludeHistory: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.History.Should().NotBeEmpty();
        _taskRepositoryMock.Verify(x => x.GetByIdWithHistoryAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithCommentsOnlyShouldNotLoadHistory()
    {
        // Arrange
        var task = CreateBasicTask();
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var query = new GetTaskByIdQuery(_taskId, _userId, IncludeComments: true, IncludeHistory: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.History.Should().BeEmpty();
        _taskRepositoryMock.Verify(x => x.GetByIdWithCommentsAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNeitherShouldLoadBasicTask()
    {
        // Arrange
        var task = CreateBasicTask();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var query = new GetTaskByIdQuery(_taskId, _userId, IncludeComments: false, IncludeHistory: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Comments.Should().BeEmpty();
        result.History.Should().BeEmpty();
        _taskRepositoryMock.Verify(x => x.GetByIdAsync(_taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithHistoryShouldMapHistoryFields()
    {
        // Arrange
        var task = CreateTaskWithHistory();
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAndHistoryAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var query = new GetTaskByIdQuery(_taskId, _userId, IncludeComments: true, IncludeHistory: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.History.Should().NotBeEmpty();
        var firstHistory = result.History[0];
        firstHistory.Action.Should().Be(TaskHistoryAction.Created);
        firstHistory.Description.Should().NotBeNullOrWhiteSpace();
        firstHistory.ChangedByUserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleWithNonexistentTaskShouldThrowNotFound()
    {
        // Arrange
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAndHistoryAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskAssignment?)null);

        var query = new GetTaskByIdQuery(_taskId, _userId, IncludeComments: true, IncludeHistory: true);

        // Act & Assert
        await Assert.ThrowsAsync<TaskAssignmentNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task HandleWithNonMemberShouldThrowAccessDenied()
    {
        // Arrange
        var nonMemberId = Guid.NewGuid();
        var task = CreateBasicTask();
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAndHistoryAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var query = new GetTaskByIdQuery(_taskId, nonMemberId, IncludeComments: true, IncludeHistory: true);

        // Act & Assert
        await Assert.ThrowsAsync<TeamAccessDeniedException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    private void SetupValidTeam()
    {
        var team = Team.Create(_assignedById, "Test Team", "Test team description");
        team.InviteMember("member@test.com", TeamRole.Editor, _assignedById);

        // Accept invitation to add userId as member
        var invitation = team.Invitations.First();
        team.AcceptInvitation(invitation.Token, _userId, "member@test.com");

        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
    }

    private TaskAssignment CreateBasicTask()
    {
        return TaskAssignment.Create(
            Guid.NewGuid(), _teamId, _userId, _assignedById,
            DateTime.UtcNow.AddDays(5), "Test task");
    }

    private TaskAssignment CreateTaskWithHistory()
    {
        var task = TaskAssignment.Create(
            Guid.NewGuid(), _teamId, _userId, _assignedById,
            DateTime.UtcNow.AddDays(5), "Task with history");

        // Add some history by changing status
        task.UpdateStatus(AssignmentStatus.InProgress, _userId);

        return task;
    }
}
