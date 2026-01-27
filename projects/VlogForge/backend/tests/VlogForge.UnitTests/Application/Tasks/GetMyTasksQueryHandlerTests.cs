using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.Queries.GetMyTasks;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Tasks;

/// <summary>
/// Unit tests for GetMyTasksQueryHandler.
/// Story: ACF-008
/// </summary>
[Trait("Story", "ACF-008")]
public class GetMyTasksQueryHandlerTests
{
    private readonly Mock<ITaskAssignmentRepository> _repositoryMock;
    private readonly Mock<ILogger<GetMyTasksQueryHandler>> _loggerMock;
    private readonly GetMyTasksQueryHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();

    public GetMyTasksQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITaskAssignmentRepository>();
        _loggerMock = new Mock<ILogger<GetMyTasksQueryHandler>>();
        _handler = new GetMyTasksQueryHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleShouldReturnTasksForUser()
    {
        // Arrange
        var tasks = CreateTestTasks();
        _repositoryMock.Setup(x => x.GetByAssigneeIdPagedAsync(
                _userId, 1, 20, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, tasks.Count));

        var query = new GetMyTasksQuery(_userId, 1, 20, true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task HandleShouldReturnEmptyListWhenNoTasks()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByAssigneeIdPagedAsync(
                _userId, 1, 20, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskAssignment>(), 0));

        var query = new GetMyTasksQuery(_userId, 1, 20, true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleShouldExcludeCompletedWhenRequested()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByAssigneeIdPagedAsync(
                _userId, 1, 20, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskAssignment>(), 0));

        var query = new GetMyTasksQuery(_userId, 1, 20, false);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetByAssigneeIdPagedAsync(
            _userId, 1, 20, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleShouldMapOverdueCorrectly()
    {
        // Arrange
        var contentItemId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var assignedById = Guid.NewGuid();

        var overdueTask = TaskAssignment.Create(
            contentItemId,
            teamId,
            _userId,
            assignedById,
            DateTime.UtcNow.AddDays(-1), // Past due date
            "Overdue task");

        _repositoryMock.Setup(x => x.GetByAssigneeIdPagedAsync(
                _userId, 1, 20, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskAssignment> { overdueTask }, 1));

        var query = new GetMyTasksQuery(_userId, 1, 20, true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].IsOverdue.Should().BeTrue();
    }

    [Fact]
    public async Task HandleShouldCalculatePaginationCorrectly()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByAssigneeIdPagedAsync(
                _userId, 2, 10, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskAssignment>(), 25)); // 25 total items

        var query = new GetMyTasksQuery(_userId, 2, 10, true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
    }

    private List<TaskAssignment> CreateTestTasks()
    {
        var contentItemId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var assignedById = Guid.NewGuid();

        return new List<TaskAssignment>
        {
            TaskAssignment.Create(contentItemId, teamId, _userId, assignedById, DateTime.UtcNow.AddDays(1), "Task 1"),
            TaskAssignment.Create(Guid.NewGuid(), teamId, _userId, assignedById, DateTime.UtcNow.AddDays(2), "Task 2"),
            TaskAssignment.Create(Guid.NewGuid(), teamId, _userId, assignedById, DateTime.UtcNow.AddDays(3), "Task 3")
        };
    }
}
