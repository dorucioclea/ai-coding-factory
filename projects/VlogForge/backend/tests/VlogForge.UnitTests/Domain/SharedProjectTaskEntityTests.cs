using FluentAssertions;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for the SharedProjectTask entity.
/// Story: ACF-013
/// </summary>
[Trait("Story", "ACF-013")]
public class SharedProjectTaskEntityTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTask()
    {
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var task = SharedProjectTask.Create(projectId, userId, "Write script", "Draft the video script");

        task.Should().NotBeNull();
        task.SharedProjectId.Should().Be(projectId);
        task.CreatedByUserId.Should().Be(userId);
        task.Title.Should().Be("Write script");
        task.Description.Should().Be("Draft the video script");
        task.Status.Should().Be(SharedProjectTaskStatus.Open);
        task.AssigneeId.Should().BeNull();
        task.DueDate.Should().BeNull();
        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithAssigneeAndDueDate_ShouldSetFields()
    {
        var assigneeId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(7);

        var task = SharedProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Task",
            assigneeId: assigneeId, dueDate: dueDate);

        task.AssigneeId.Should().Be(assigneeId);
        task.DueDate.Should().BeCloseTo(dueDate, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ShouldThrow(string title)
    {
        var act = () => SharedProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), title);
        act.Should().Throw<ArgumentException>().WithMessage("*title*");
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldThrow()
    {
        var longTitle = new string('x', SharedProjectTask.MaxTitleLength + 1);
        var act = () => SharedProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), longTitle);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_Title_ShouldUpdateTitle()
    {
        var task = SharedProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Original");
        task.Update(title: "Updated");
        task.Title.Should().Be("Updated");
    }

    [Fact]
    public void Update_Status_ToCompleted_ShouldSetCompletedAt()
    {
        var task = SharedProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Task");
        task.Update(status: SharedProjectTaskStatus.Completed);

        task.Status.Should().Be(SharedProjectTaskStatus.Completed);
        task.CompletedAt.Should().NotBeNull();
        task.CompletedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_Status_FromCompletedToInProgress_ShouldClearCompletedAt()
    {
        var task = SharedProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Task");
        task.Update(status: SharedProjectTaskStatus.Completed);
        task.Update(status: SharedProjectTaskStatus.InProgress);

        task.Status.Should().Be(SharedProjectTaskStatus.InProgress);
        task.CompletedAt.Should().BeNull();
    }
}
