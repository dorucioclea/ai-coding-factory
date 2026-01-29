using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.Queries.GetMyTasks;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Tasks;

/// <summary>
/// Unit tests for GetMyTasksQuery with status filter and sorting (ACF-014).
/// Covers AC1 (cross-team aggregation), AC2 (status grouping), AC3 (due date sorting).
/// Story: ACF-014
/// </summary>
[Trait("Story", "ACF-014")]
public class GetMyTasksGroupedTests
{
    private readonly Mock<ITaskAssignmentRepository> _repositoryMock;
    private readonly Mock<ILogger<GetMyTasksQueryHandler>> _loggerMock;
    private readonly GetMyTasksQueryHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _team1Id = Guid.NewGuid();
    private readonly Guid _team2Id = Guid.NewGuid();
    private readonly Guid _assignedById = Guid.NewGuid();

    public GetMyTasksGroupedTests()
    {
        _repositoryMock = new Mock<ITaskAssignmentRepository>();
        _loggerMock = new Mock<ILogger<GetMyTasksQueryHandler>>();
        _handler = new GetMyTasksQueryHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithStatusFilterShouldReturnFilteredTasks()
    {
        // Arrange
        var inProgressTasks = new List<TaskAssignment>
        {
            TaskAssignment.Create(Guid.NewGuid(), _team1Id, _userId, _assignedById, DateTime.UtcNow.AddDays(1), "Task 1"),
        };
        inProgressTasks[0].UpdateStatus(AssignmentStatus.InProgress, _userId);

        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 1, 20, AssignmentStatus.InProgress, "dueDate", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((inProgressTasks, 1));

        var query = new GetMyTasksQuery(
            _userId, 1, 20,
            Status: AssignmentStatus.InProgress,
            SortBy: "dueDate",
            SortDirection: "asc");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(AssignmentStatus.InProgress);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleWithNotStartedFilterShouldReturnOnlyNotStartedTasks()
    {
        // Arrange
        var notStartedTasks = CreateTasksWithStatus(AssignmentStatus.NotStarted, 2);

        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 1, 20, AssignmentStatus.NotStarted, "dueDate", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((notStartedTasks, 2));

        var query = new GetMyTasksQuery(
            _userId, 1, 20,
            Status: AssignmentStatus.NotStarted);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(t => t.Status.Should().Be(AssignmentStatus.NotStarted));
    }

    [Fact]
    public async Task HandleWithCompletedFilterShouldReturnOnlyCompletedTasks()
    {
        // Arrange
        var completedTasks = CreateTasksWithStatus(AssignmentStatus.Completed, 3);

        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 1, 20, AssignmentStatus.Completed, "dueDate", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((completedTasks, 3));

        var query = new GetMyTasksQuery(
            _userId, 1, 20,
            Status: AssignmentStatus.Completed);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Should().AllSatisfy(t => t.Status.Should().Be(AssignmentStatus.Completed));
    }

    [Fact]
    public async Task HandleWithNoStatusFilterShouldReturnAllTasks()
    {
        // Arrange
        var allTasks = CreateMixedStatusTasks();

        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 1, 20, null, "dueDate", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((allTasks, allTasks.Count));

        var query = new GetMyTasksQuery(_userId, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(allTasks.Count);
        result.TotalCount.Should().Be(allTasks.Count);
    }

    [Fact]
    public async Task HandleWithSortByStatusShouldPassCorrectSortParam()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 1, 20, null, "status", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskAssignment>(), 0));

        var query = new GetMyTasksQuery(
            _userId, 1, 20,
            SortBy: "status",
            SortDirection: "asc");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetByAssigneeIdFilteredPagedAsync(
            _userId, 1, 20, null, "status", "asc", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleWithDescSortShouldPassCorrectDirection()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 1, 20, null, "dueDate", "desc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskAssignment>(), 0));

        var query = new GetMyTasksQuery(
            _userId, 1, 20,
            SortDirection: "desc");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetByAssigneeIdFilteredPagedAsync(
            _userId, 1, 20, null, "dueDate", "desc", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldAggregateTasksAcrossTeams()
    {
        // Arrange - tasks from two different teams
        var task1 = TaskAssignment.Create(Guid.NewGuid(), _team1Id, _userId, _assignedById, DateTime.UtcNow.AddDays(1), "Team 1 task");
        var task2 = TaskAssignment.Create(Guid.NewGuid(), _team2Id, _userId, _assignedById, DateTime.UtcNow.AddDays(2), "Team 2 task");
        var tasks = new List<TaskAssignment> { task1, task2 };

        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 1, 20, null, "dueDate", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, 2));

        var query = new GetMyTasksQuery(_userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].TeamId.Should().Be(_team1Id);
        result.Items[1].TeamId.Should().Be(_team2Id);
    }

    [Fact]
    public async Task HandleShouldCorrectlyMapOverdueFlag()
    {
        // Arrange
        var overdueTask = TaskAssignment.Create(
            Guid.NewGuid(), _team1Id, _userId, _assignedById,
            DateTime.UtcNow.AddDays(-2), "Overdue task");

        var futureTask = TaskAssignment.Create(
            Guid.NewGuid(), _team1Id, _userId, _assignedById,
            DateTime.UtcNow.AddDays(5), "Future task");

        var tasks = new List<TaskAssignment> { overdueTask, futureTask };

        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 1, 20, null, "dueDate", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, 2));

        var query = new GetMyTasksQuery(_userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items[0].IsOverdue.Should().BeTrue();
        result.Items[1].IsOverdue.Should().BeFalse();
    }

    [Fact]
    public async Task HandleWithPaginationShouldReturnCorrectMetadata()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByAssigneeIdFilteredPagedAsync(
                _userId, 2, 5, null, "dueDate", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskAssignment>(), 12));

        var query = new GetMyTasksQuery(_userId, Page: 2, PageSize: 5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    private List<TaskAssignment> CreateTasksWithStatus(AssignmentStatus status, int count)
    {
        var tasks = new List<TaskAssignment>();
        for (var i = 0; i < count; i++)
        {
            var task = TaskAssignment.Create(
                Guid.NewGuid(), _team1Id, _userId, _assignedById,
                DateTime.UtcNow.AddDays(i + 1), $"Task {i + 1}");

            if (status == AssignmentStatus.InProgress)
            {
                task.UpdateStatus(AssignmentStatus.InProgress, _userId);
            }
            else if (status == AssignmentStatus.Completed)
            {
                task.UpdateStatus(AssignmentStatus.InProgress, _userId);
                task.UpdateStatus(AssignmentStatus.Completed, _userId);
            }

            tasks.Add(task);
        }
        return tasks;
    }

    private List<TaskAssignment> CreateMixedStatusTasks()
    {
        var tasks = new List<TaskAssignment>
        {
            TaskAssignment.Create(Guid.NewGuid(), _team1Id, _userId, _assignedById, DateTime.UtcNow.AddDays(1), "Not started"),
        };

        var inProgress = TaskAssignment.Create(Guid.NewGuid(), _team1Id, _userId, _assignedById, DateTime.UtcNow.AddDays(2), "In progress");
        inProgress.UpdateStatus(AssignmentStatus.InProgress, _userId);
        tasks.Add(inProgress);

        var completed = TaskAssignment.Create(Guid.NewGuid(), _team2Id, _userId, _assignedById, DateTime.UtcNow.AddDays(3), "Completed");
        completed.UpdateStatus(AssignmentStatus.InProgress, _userId);
        completed.UpdateStatus(AssignmentStatus.Completed, _userId);
        tasks.Add(completed);

        return tasks;
    }
}
