using FluentAssertions;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for the SharedProject aggregate root.
/// Story: ACF-013
/// </summary>
[Trait("Story", "ACF-013")]
public class SharedProjectTests
{
    private readonly Guid _senderId = Guid.NewGuid();
    private readonly Guid _recipientId = Guid.NewGuid();
    private readonly Guid _collaborationRequestId = Guid.NewGuid();

    private SharedProject CreateValidProject(string name = "Joint Video Project")
    {
        return SharedProject.Create(_collaborationRequestId, _senderId, _recipientId, name, "A collaboration project");
    }

    // --- Create Tests ---

    [Fact]
    public void Create_WithValidData_ShouldCreateProjectWithBothMembers()
    {
        var project = CreateValidProject();

        project.Should().NotBeNull();
        project.Name.Should().Be("Joint Video Project");
        project.Description.Should().Be("A collaboration project");
        project.Status.Should().Be(SharedProjectStatus.Active);
        project.CollaborationRequestId.Should().Be(_collaborationRequestId);
        project.OwnerId.Should().Be(_senderId);
        project.Members.Should().HaveCount(2);
        project.Members.Should().Contain(m => m.UserId == _senderId && m.Role == SharedProjectRole.Owner);
        project.Members.Should().Contain(m => m.UserId == _recipientId && m.Role == SharedProjectRole.Member);
    }

    [Fact]
    public void Create_ShouldLogInitialActivities()
    {
        var project = CreateValidProject();

        project.Activities.Should().HaveCount(3);
        project.Activities.Should().Contain(a => a.ActivityType == SharedProjectActivityType.ProjectCreated);
        project.Activities.Should().Contain(a => a.ActivityType == SharedProjectActivityType.MemberJoined && a.UserId == _senderId);
        project.Activities.Should().Contain(a => a.ActivityType == SharedProjectActivityType.MemberJoined && a.UserId == _recipientId);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var project = CreateValidProject();
        project.DomainEvents.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrow(string name)
    {
        var act = () => SharedProject.Create(_collaborationRequestId, _senderId, _recipientId, name);
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldThrow()
    {
        var longName = new string('x', SharedProject.MaxNameLength + 1);
        var act = () => SharedProject.Create(_collaborationRequestId, _senderId, _recipientId, longName);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithSameSenderAndRecipient_ShouldThrow()
    {
        var act = () => SharedProject.Create(_collaborationRequestId, _senderId, _senderId, "Test");
        act.Should().Throw<ArgumentException>().WithMessage("*same*");
    }

    [Fact]
    public void Create_WithEmptyCollaborationRequestId_ShouldThrow()
    {
        var act = () => SharedProject.Create(Guid.Empty, _senderId, _recipientId, "Test");
        act.Should().Throw<ArgumentException>();
    }

    // --- Task Tests ---

    [Fact]
    public void AddTask_WithValidData_ShouldAddTask()
    {
        var project = CreateValidProject();
        var task = project.AddTask(_senderId, "Film intro", "Film the intro segment");

        task.Should().NotBeNull();
        task.Title.Should().Be("Film intro");
        task.Description.Should().Be("Film the intro segment");
        task.Status.Should().Be(SharedProjectTaskStatus.Open);
        project.Tasks.Should().HaveCount(1);
    }

    [Fact]
    public void AddTask_ShouldLogActivity()
    {
        var project = CreateValidProject();
        project.AddTask(_senderId, "Film intro");

        project.Activities.Should().Contain(a =>
            a.ActivityType == SharedProjectActivityType.TaskAdded &&
            a.Message.Contains("Film intro"));
    }

    [Fact]
    public void AddTask_WithAssignee_ShouldSetAssignee()
    {
        var project = CreateValidProject();
        var task = project.AddTask(_senderId, "Edit video", assigneeId: _recipientId);

        task.AssigneeId.Should().Be(_recipientId);
    }

    [Fact]
    public void AddTask_WithNonMemberAssignee_ShouldThrow()
    {
        var project = CreateValidProject();
        var nonMemberId = Guid.NewGuid();

        var act = () => project.AddTask(_senderId, "Task", assigneeId: nonMemberId);
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void AddTask_ByNonMember_ShouldThrow()
    {
        var project = CreateValidProject();
        var nonMemberId = Guid.NewGuid();

        var act = () => project.AddTask(nonMemberId, "Task");
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void AddTask_OnClosedProject_ShouldThrow()
    {
        var project = CreateValidProject();
        project.Close(_senderId);

        var act = () => project.AddTask(_senderId, "Task");
        act.Should().Throw<InvalidOperationException>().WithMessage("*closed*");
    }

    // --- Update Task Tests ---

    [Fact]
    public void UpdateTask_ShouldUpdateFields()
    {
        var project = CreateValidProject();
        var task = project.AddTask(_senderId, "Old title");

        project.UpdateTask(task.Id, _recipientId, title: "New title",
            status: SharedProjectTaskStatus.InProgress);

        task.Title.Should().Be("New title");
        task.Status.Should().Be(SharedProjectTaskStatus.InProgress);
    }

    [Fact]
    public void UpdateTask_ToCompleted_ShouldLogCompletion()
    {
        var project = CreateValidProject();
        var task = project.AddTask(_senderId, "Film scene");

        project.UpdateTask(task.Id, _senderId, status: SharedProjectTaskStatus.Completed);

        project.Activities.Should().Contain(a =>
            a.ActivityType == SharedProjectActivityType.TaskCompleted);
        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateTask_NonExistentTask_ShouldThrow()
    {
        var project = CreateValidProject();
        var act = () => project.UpdateTask(Guid.NewGuid(), _senderId, title: "Test");
        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    // --- Link Tests ---

    [Fact]
    public void AddLink_WithValidData_ShouldAddLink()
    {
        var project = CreateValidProject();
        var link = project.AddLink(_senderId, "Script Document", "https://docs.google.com/doc/123");

        link.Should().NotBeNull();
        link.Title.Should().Be("Script Document");
        link.Url.Should().Be("https://docs.google.com/doc/123");
        project.Links.Should().HaveCount(1);
    }

    [Fact]
    public void AddLink_ShouldLogActivity()
    {
        var project = CreateValidProject();
        project.AddLink(_senderId, "Script", "https://docs.google.com/doc");

        project.Activities.Should().Contain(a =>
            a.ActivityType == SharedProjectActivityType.LinkAdded);
    }

    [Fact]
    public void RemoveLink_ShouldRemoveAndLogActivity()
    {
        var project = CreateValidProject();
        var link = project.AddLink(_senderId, "Old doc", "https://example.com/old");

        project.RemoveLink(link.Id, _senderId);

        project.Links.Should().BeEmpty();
        project.Activities.Should().Contain(a =>
            a.ActivityType == SharedProjectActivityType.LinkRemoved);
    }

    [Fact]
    public void RemoveLink_NonExistentLink_ShouldThrow()
    {
        var project = CreateValidProject();
        var act = () => project.RemoveLink(Guid.NewGuid(), _senderId);
        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    // --- Leave Tests ---

    [Fact]
    public void Leave_Member_ShouldRemoveFromProject()
    {
        var project = CreateValidProject();
        project.Leave(_recipientId);

        project.Members.Should().HaveCount(1);
        project.Members.Should().NotContain(m => m.UserId == _recipientId);
    }

    [Fact]
    public void Leave_ShouldLogActivity()
    {
        var project = CreateValidProject();
        project.Leave(_recipientId);

        project.Activities.Should().Contain(a =>
            a.ActivityType == SharedProjectActivityType.MemberLeft &&
            a.UserId == _recipientId);
    }

    [Fact]
    public void Leave_Owner_ShouldTransferOwnership()
    {
        var project = CreateValidProject();
        project.Leave(_senderId);

        project.OwnerId.Should().Be(_recipientId);
        project.Members.Should().HaveCount(1);
        project.Members.First().Role.Should().Be(SharedProjectRole.Owner);
    }

    [Fact]
    public void Leave_AllMembers_ShouldCloseProject()
    {
        var project = CreateValidProject();
        project.Leave(_recipientId);
        project.Leave(_senderId);

        project.Status.Should().Be(SharedProjectStatus.Closed);
        project.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Leave_NonMember_ShouldThrow()
    {
        var project = CreateValidProject();
        var act = () => project.Leave(Guid.NewGuid());
        act.Should().Throw<UnauthorizedAccessException>();
    }

    // --- Close Tests ---

    [Fact]
    public void Close_ByOwner_ShouldCloseProject()
    {
        var project = CreateValidProject();
        project.Close(_senderId);

        project.Status.Should().Be(SharedProjectStatus.Closed);
        project.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Close_ByNonOwner_ShouldThrow()
    {
        var project = CreateValidProject();
        var act = () => project.Close(_recipientId);
        act.Should().Throw<UnauthorizedAccessException>().WithMessage("*owner*");
    }

    [Fact]
    public void Close_AlreadyClosed_ShouldThrow()
    {
        var project = CreateValidProject();
        project.Close(_senderId);
        var act = () => project.Close(_senderId);
        act.Should().Throw<InvalidOperationException>().WithMessage("*closed*");
    }

    [Fact]
    public void Close_ShouldLogActivity()
    {
        var project = CreateValidProject();
        project.Close(_senderId);

        project.Activities.Should().Contain(a =>
            a.ActivityType == SharedProjectActivityType.ProjectClosed);
    }

    // --- IsMember Tests ---

    [Fact]
    public void IsMember_WithMember_ShouldReturnTrue()
    {
        var project = CreateValidProject();
        project.IsMember(_senderId).Should().BeTrue();
        project.IsMember(_recipientId).Should().BeTrue();
    }

    [Fact]
    public void IsMember_WithNonMember_ShouldReturnFalse()
    {
        var project = CreateValidProject();
        project.IsMember(Guid.NewGuid()).Should().BeFalse();
    }
}
