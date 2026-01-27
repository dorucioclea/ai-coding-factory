using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.Commands.AddTaskComment;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Tasks;

/// <summary>
/// Unit tests for AddTaskCommentCommandHandler.
/// Story: ACF-008
/// </summary>
[Trait("Story", "ACF-008")]
public class AddTaskCommentCommandHandlerTests
{
    private readonly Mock<ITaskAssignmentRepository> _taskRepositoryMock;
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<ILogger<AddTaskCommentCommandHandler>> _loggerMock;
    private readonly AddTaskCommentCommandHandler _handler;

    private readonly Guid _taskId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _teamOwnerId = Guid.NewGuid();

    public AddTaskCommentCommandHandlerTests()
    {
        _taskRepositoryMock = new Mock<ITaskAssignmentRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<AddTaskCommentCommandHandler>>();

        _handler = new AddTaskCommentCommandHandler(
            _taskRepositoryMock.Object,
            _teamRepositoryMock.Object,
            _loggerMock.Object);

        SetupValidTeam();
    }

    [Fact]
    public async Task HandleWithValidDataShouldAddComment()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new AddTaskCommentCommand(_taskId, _authorId, "Great progress!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("Great progress!");
        result.AuthorId.Should().Be(_authorId);
        result.ParentCommentId.Should().BeNull();

        _taskRepositoryMock.Verify(x => x.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        _taskRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithParentCommentShouldCreateThreadedReply()
    {
        // Arrange
        var task = CreateTestTask();
        var parentComment = task.AddComment(_teamOwnerId, "Parent comment");

        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new AddTaskCommentCommand(_taskId, _authorId, "Reply to parent", parentComment.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ParentCommentId.Should().Be(parentComment.Id);
        result.Content.Should().Be("Reply to parent");
    }

    [Fact]
    public async Task HandleWithNonexistentTaskShouldThrow()
    {
        // Arrange
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskAssignment?)null);

        var command = new AddTaskCommentCommand(_taskId, _authorId, "Comment");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TaskAssignmentNotFoundException>();
    }

    [Fact]
    public async Task HandleWithNonexistentTeamShouldThrow()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new AddTaskCommentCommand(_taskId, _authorId, "Comment");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamNotFoundException>();
    }

    [Fact]
    public async Task HandleByNonTeamMemberShouldThrow()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var nonMemberId = Guid.NewGuid();
        var command = new AddTaskCommentCommand(_taskId, nonMemberId, "Comment");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamAccessDeniedException>()
            .WithMessage("*team member*");
    }

    [Fact]
    public async Task HandleWithInvalidParentCommentShouldThrow()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(x => x.GetByIdWithCommentsAsync(_taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var invalidParentId = Guid.NewGuid();
        var command = new AddTaskCommentCommand(_taskId, _authorId, "Reply", invalidParentId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Parent comment not found*");
    }

    private TaskAssignment CreateTestTask()
    {
        var contentItemId = Guid.NewGuid();
        var task = TaskAssignment.Create(
            contentItemId,
            _teamId,
            _authorId,
            _teamOwnerId,
            DateTime.UtcNow.AddDays(7),
            "Test task");

        // Use reflection to set the Id for testing
        typeof(TaskAssignment).GetProperty("Id")!.DeclaringType!
            .GetProperty("Id")!.SetValue(task, _taskId);

        return task;
    }

    private void SetupValidTeam()
    {
        var team = Team.Create(_teamOwnerId, "Test Team", null);
        var inviteToken = team.InviteMember("author@example.com", TeamRole.Editor, _teamOwnerId);
        team.AcceptInvitation(inviteToken, _authorId, "author@example.com");

        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
    }
}
