using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for TaskAssignment aggregate root.
/// Story: ACF-008
/// </summary>
[Trait("Story", "ACF-008")]
public class TaskAssignmentTests
{
    private readonly Guid _contentItemId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();
    private readonly Guid _assigneeId = Guid.NewGuid();
    private readonly Guid _assignedById = Guid.NewGuid();
    private readonly DateTime _dueDate = DateTime.UtcNow.AddDays(7);
    private const string TaskNotes = "Complete the video editing";

    #region Task Creation

    [Fact]
    public void CreateWithValidDataShouldReturnTaskAssignment()
    {
        // Act
        var task = TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            TaskNotes);

        // Assert
        task.Should().NotBeNull();
        task.Id.Should().NotBe(Guid.Empty);
        task.ContentItemId.Should().Be(_contentItemId);
        task.TeamId.Should().Be(_teamId);
        task.AssigneeId.Should().Be(_assigneeId);
        task.AssignedById.Should().Be(_assignedById);
        task.DueDate.Should().BeCloseTo(_dueDate, TimeSpan.FromSeconds(1));
        task.Notes.Should().Be(TaskNotes);
        task.Status.Should().Be(AssignmentStatus.NotStarted);
        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void CreateWithNullNotesShouldSucceed()
    {
        // Act
        var task = TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            null);

        // Assert
        task.Should().NotBeNull();
        task.Notes.Should().BeNull();
    }

    [Fact]
    public void CreateShouldRaiseTaskAssignedEvent()
    {
        // Act
        var task = TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            TaskNotes);

        // Assert
        task.DomainEvents.Should().HaveCount(1);
        var evt = task.DomainEvents.First();
        evt.Should().BeOfType<TaskAssignedEvent>();
        var assignedEvent = (TaskAssignedEvent)evt;
        assignedEvent.TaskAssignmentId.Should().Be(task.Id);
        assignedEvent.ContentItemId.Should().Be(_contentItemId);
        assignedEvent.AssigneeId.Should().Be(_assigneeId);
    }

    [Fact]
    public void CreateShouldRecordHistoryEntry()
    {
        // Act
        var task = TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            TaskNotes);

        // Assert
        task.History.Should().HaveCount(1);
        var historyEntry = task.History.First();
        historyEntry.Action.Should().Be(TaskHistoryAction.Created);
        historyEntry.ChangedByUserId.Should().Be(_assignedById);
    }

    [Fact]
    public void CreateWithEmptyContentItemIdShouldThrow()
    {
        // Act
        var act = () => TaskAssignment.Create(
            Guid.Empty,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            TaskNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Content item ID*");
    }

    [Fact]
    public void CreateWithEmptyTeamIdShouldThrow()
    {
        // Act
        var act = () => TaskAssignment.Create(
            _contentItemId,
            Guid.Empty,
            _assigneeId,
            _assignedById,
            _dueDate,
            TaskNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Team ID*");
    }

    [Fact]
    public void CreateWithEmptyAssigneeIdShouldThrow()
    {
        // Act
        var act = () => TaskAssignment.Create(
            _contentItemId,
            _teamId,
            Guid.Empty,
            _assignedById,
            _dueDate,
            TaskNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Assignee ID*");
    }

    [Fact]
    public void CreateWithEmptyAssignedByIdShouldThrow()
    {
        // Act
        var act = () => TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            Guid.Empty,
            _dueDate,
            TaskNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Assigned by ID*");
    }

    [Fact]
    public void CreateWithNotesExceedingMaxLengthShouldThrow()
    {
        // Arrange
        var longNotes = new string('a', TaskAssignment.MaxNotesLength + 1);

        // Act
        var act = () => TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            longNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Notes*{TaskAssignment.MaxNotesLength}*");
    }

    [Fact]
    public void CreateShouldNormalizeDueDateToUtc()
    {
        // Arrange
        var localDate = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Local);

        // Act
        var task = TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            localDate,
            TaskNotes);

        // Assert
        task.DueDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region Status Updates

    [Fact]
    public void UpdateStatusToInProgressShouldSucceed()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);

        // Assert
        task.Status.Should().Be(AssignmentStatus.InProgress);
    }

    [Fact]
    public void UpdateStatusToCompletedShouldSetCompletedAt()
    {
        // Arrange
        var task = CreateDefaultTask();
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);

        // Act
        task.UpdateStatus(AssignmentStatus.Completed, _assigneeId);

        // Assert
        task.Status.Should().Be(AssignmentStatus.Completed);
        task.CompletedAt.Should().NotBeNull();
        task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ReopeningCompletedTaskShouldClearCompletedAt()
    {
        // Arrange
        var task = CreateDefaultTask();
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);
        task.UpdateStatus(AssignmentStatus.Completed, _assigneeId);
        task.CompletedAt.Should().NotBeNull();

        // Act
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);

        // Assert
        task.Status.Should().Be(AssignmentStatus.InProgress);
        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateStatusShouldRaiseTaskStatusChangedEvent()
    {
        // Arrange
        var task = CreateDefaultTask();
        task.ClearDomainEvents();

        // Act
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);

        // Assert
        task.DomainEvents.Should().HaveCount(1);
        var evt = task.DomainEvents.First();
        evt.Should().BeOfType<TaskStatusChangedEvent>();
        var statusEvent = (TaskStatusChangedEvent)evt;
        statusEvent.OldStatus.Should().Be(AssignmentStatus.NotStarted);
        statusEvent.NewStatus.Should().Be(AssignmentStatus.InProgress);
    }

    [Fact]
    public void UpdateStatusShouldRecordHistoryEntry()
    {
        // Arrange
        var task = CreateDefaultTask();
        var initialHistoryCount = task.History.Count;

        // Act
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);

        // Assert
        task.History.Should().HaveCount(initialHistoryCount + 1);
        var lastEntry = task.History.Last();
        lastEntry.Action.Should().Be(TaskHistoryAction.StatusChanged);
        lastEntry.Description.Should().Contain("NotStarted").And.Contain("InProgress");
    }

    [Fact]
    public void UpdateStatusShouldIncrementVersion()
    {
        // Arrange
        var task = CreateDefaultTask();
        var initialVersion = task.Version;

        // Act
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);

        // Assert
        task.Version.Should().Be(initialVersion + 1);
    }

    [Fact]
    public void UpdateStatusToSameStatusShouldNotChangeAnything()
    {
        // Arrange
        var task = CreateDefaultTask();
        var initialVersion = task.Version;
        task.ClearDomainEvents();

        // Act
        task.UpdateStatus(AssignmentStatus.NotStarted, _assigneeId);

        // Assert
        task.Version.Should().Be(initialVersion);
        task.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateStatusWithEmptyUserIdShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        var act = () => task.UpdateStatus(AssignmentStatus.InProgress, Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Updated by user ID*");
    }

    [Fact]
    public void UpdateStatusWithInvalidEnumShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();
        var invalidStatus = (AssignmentStatus)999;

        // Act
        var act = () => task.UpdateStatus(invalidStatus, _assigneeId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid status value*");
    }

    #endregion

    #region Due Date Updates

    [Fact]
    public void UpdateDueDateShouldSucceed()
    {
        // Arrange
        var task = CreateDefaultTask();
        var newDueDate = DateTime.UtcNow.AddDays(14);

        // Act
        task.UpdateDueDate(newDueDate, _assignedById);

        // Assert
        task.DueDate.Should().BeCloseTo(newDueDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateDueDateShouldRaiseEvent()
    {
        // Arrange
        var task = CreateDefaultTask();
        var newDueDate = DateTime.UtcNow.AddDays(14);
        task.ClearDomainEvents();

        // Act
        task.UpdateDueDate(newDueDate, _assignedById);

        // Assert
        task.DomainEvents.Should().Contain(e => e is TaskDueDateChangedEvent);
    }

    [Fact]
    public void UpdateDueDateShouldRecordHistory()
    {
        // Arrange
        var task = CreateDefaultTask();
        var initialHistoryCount = task.History.Count;
        var newDueDate = DateTime.UtcNow.AddDays(14);

        // Act
        task.UpdateDueDate(newDueDate, _assignedById);

        // Assert
        task.History.Should().HaveCount(initialHistoryCount + 1);
        var lastEntry = task.History.Last();
        lastEntry.Action.Should().Be(TaskHistoryAction.DueDateChanged);
    }

    [Fact]
    public void UpdateDueDateToSameDateShouldNotChangeAnything()
    {
        // Arrange
        var task = CreateDefaultTask();
        var initialVersion = task.Version;
        task.ClearDomainEvents();

        // Act
        task.UpdateDueDate(task.DueDate, _assignedById);

        // Assert
        task.Version.Should().Be(initialVersion);
        task.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region Reassignment

    [Fact]
    public void ReassignShouldUpdateAssignee()
    {
        // Arrange
        var task = CreateDefaultTask();
        var newAssigneeId = Guid.NewGuid();

        // Act
        task.Reassign(newAssigneeId, _assignedById);

        // Assert
        task.AssigneeId.Should().Be(newAssigneeId);
    }

    [Fact]
    public void ReassignShouldRaiseEvent()
    {
        // Arrange
        var task = CreateDefaultTask();
        var newAssigneeId = Guid.NewGuid();
        task.ClearDomainEvents();

        // Act
        task.Reassign(newAssigneeId, _assignedById);

        // Assert
        task.DomainEvents.Should().Contain(e => e is TaskReassignedEvent);
        var evt = task.DomainEvents.OfType<TaskReassignedEvent>().First();
        evt.OldAssigneeId.Should().Be(_assigneeId);
        evt.NewAssigneeId.Should().Be(newAssigneeId);
    }

    [Fact]
    public void ReassignShouldRecordHistory()
    {
        // Arrange
        var task = CreateDefaultTask();
        var initialHistoryCount = task.History.Count;
        var newAssigneeId = Guid.NewGuid();

        // Act
        task.Reassign(newAssigneeId, _assignedById);

        // Assert
        task.History.Should().HaveCount(initialHistoryCount + 1);
        var lastEntry = task.History.Last();
        lastEntry.Action.Should().Be(TaskHistoryAction.Reassigned);
    }

    [Fact]
    public void ReassignToSameAssigneeShouldNotChangeAnything()
    {
        // Arrange
        var task = CreateDefaultTask();
        var initialVersion = task.Version;
        task.ClearDomainEvents();

        // Act
        task.Reassign(_assigneeId, _assignedById);

        // Assert
        task.Version.Should().Be(initialVersion);
        task.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ReassignWithEmptyNewAssigneeIdShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        var act = () => task.Reassign(Guid.Empty, _assignedById);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*New assignee ID*");
    }

    #endregion

    #region Comments

    [Fact]
    public void AddCommentShouldSucceed()
    {
        // Arrange
        var task = CreateDefaultTask();
        var commentContent = "Great progress on this!";

        // Act
        var comment = task.AddComment(_assigneeId, commentContent);

        // Assert
        comment.Should().NotBeNull();
        comment.Content.Should().Be(commentContent);
        comment.AuthorId.Should().Be(_assigneeId);
        task.Comments.Should().HaveCount(1);
    }

    [Fact]
    public void AddCommentShouldRaiseEvent()
    {
        // Arrange
        var task = CreateDefaultTask();
        task.ClearDomainEvents();

        // Act
        var comment = task.AddComment(_assigneeId, "Test comment");

        // Assert
        task.DomainEvents.Should().Contain(e => e is TaskCommentAddedEvent);
        var evt = task.DomainEvents.OfType<TaskCommentAddedEvent>().First();
        evt.CommentId.Should().Be(comment.Id);
        evt.AuthorId.Should().Be(_assigneeId);
    }

    [Fact]
    public void AddCommentShouldRecordHistory()
    {
        // Arrange
        var task = CreateDefaultTask();
        var initialHistoryCount = task.History.Count;

        // Act
        task.AddComment(_assigneeId, "Test comment");

        // Assert
        task.History.Should().HaveCount(initialHistoryCount + 1);
        var lastEntry = task.History.Last();
        lastEntry.Action.Should().Be(TaskHistoryAction.CommentAdded);
    }

    [Fact]
    public void AddThreadedCommentShouldSucceed()
    {
        // Arrange
        var task = CreateDefaultTask();
        var parentComment = task.AddComment(_assigneeId, "Parent comment");

        // Act
        var reply = task.AddComment(_assignedById, "Reply to parent", parentComment.Id);

        // Assert
        reply.ParentCommentId.Should().Be(parentComment.Id);
        task.Comments.Should().HaveCount(2);
    }

    [Fact]
    public void AddThreadedCommentWithInvalidParentShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        var act = () => task.AddComment(_assigneeId, "Reply", Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Parent comment not found*");
    }

    [Fact]
    public void AddCommentWithEmptyAuthorIdShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        var act = () => task.AddComment(Guid.Empty, "Test comment");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Author ID*");
    }

    [Fact]
    public void AddCommentWithEmptyContentShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        var act = () => task.AddComment(_assigneeId, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*content*empty*");
    }

    [Fact]
    public void AddCommentExceedingMaxCommentsShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();
        for (int i = 0; i < TaskAssignment.MaxComments; i++)
        {
            task.AddComment(_assigneeId, $"Comment {i}");
        }

        // Act
        var act = () => task.AddComment(_assigneeId, "One too many");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{TaskAssignment.MaxComments}*");
    }

    #endregion

    #region Comment Editing

    [Fact]
    public void EditCommentShouldUpdateContent()
    {
        // Arrange
        var task = CreateDefaultTask();
        var comment = task.AddComment(_assigneeId, "Original content");
        var newContent = "Updated content";

        // Act
        comment.Edit(newContent, _assigneeId);

        // Assert
        comment.Content.Should().Be(newContent);
        comment.IsEdited.Should().BeTrue();
        comment.EditedAt.Should().NotBeNull();
    }

    [Fact]
    public void EditCommentByNonAuthorShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();
        var comment = task.AddComment(_assigneeId, "Original content");

        // Act
        var act = () => comment.Edit("New content", _assignedById);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*Only the author*");
    }

    [Fact]
    public void EditCommentWithEmptyContentShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();
        var comment = task.AddComment(_assigneeId, "Original content");

        // Act
        var act = () => comment.Edit("", _assigneeId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Content*empty*");
    }

    [Fact]
    public void EditCommentExceedingMaxLengthShouldThrow()
    {
        // Arrange
        var task = CreateDefaultTask();
        var comment = task.AddComment(_assigneeId, "Original content");
        var longContent = new string('a', TaskComment.MaxContentLength + 1);

        // Act
        var act = () => comment.Edit(longContent, _assigneeId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Content*{TaskComment.MaxContentLength}*");
    }

    #endregion

    #region Overdue Detection

    [Fact]
    public void IsOverdueShouldReturnTrueForPastDueDate()
    {
        // Arrange
        var pastDueDate = DateTime.UtcNow.AddDays(-1);
        var task = TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            pastDueDate,
            TaskNotes);

        // Act
        var isOverdue = task.IsOverdue();

        // Assert
        isOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdueShouldReturnFalseForFutureDueDate()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        var isOverdue = task.IsOverdue();

        // Assert
        isOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdueShouldReturnFalseForCompletedTask()
    {
        // Arrange
        var pastDueDate = DateTime.UtcNow.AddDays(-1);
        var task = TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            pastDueDate,
            TaskNotes);
        task.UpdateStatus(AssignmentStatus.InProgress, _assigneeId);
        task.UpdateStatus(AssignmentStatus.Completed, _assigneeId);

        // Act
        var isOverdue = task.IsOverdue();

        // Assert
        isOverdue.Should().BeFalse();
    }

    #endregion

    #region Permission Checks

    [Fact]
    public void CanModifyShouldReturnTrueForAssignee()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        var canModify = task.CanModify(_assigneeId, false);

        // Assert
        canModify.Should().BeTrue();
    }

    [Fact]
    public void CanModifyShouldReturnTrueForAssigner()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        var canModify = task.CanModify(_assignedById, false);

        // Assert
        canModify.Should().BeTrue();
    }

    [Fact]
    public void CanModifyShouldReturnTrueForTeamAdmin()
    {
        // Arrange
        var task = CreateDefaultTask();
        var randomUserId = Guid.NewGuid();

        // Act
        var canModify = task.CanModify(randomUserId, true);

        // Assert
        canModify.Should().BeTrue();
    }

    [Fact]
    public void CanModifyShouldReturnFalseForUnauthorizedUser()
    {
        // Arrange
        var task = CreateDefaultTask();
        var randomUserId = Guid.NewGuid();

        // Act
        var canModify = task.CanModify(randomUserId, false);

        // Assert
        canModify.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private TaskAssignment CreateDefaultTask()
    {
        return TaskAssignment.Create(
            _contentItemId,
            _teamId,
            _assigneeId,
            _assignedById,
            _dueDate,
            TaskNotes);
    }

    #endregion
}
