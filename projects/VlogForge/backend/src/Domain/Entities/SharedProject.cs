using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Status of a shared project.
/// Story: ACF-013
/// </summary>
public enum SharedProjectStatus
{
    /// <summary>Project is active.</summary>
    Active = 0,

    /// <summary>Project has been closed by the owner.</summary>
    Closed = 1
}

/// <summary>
/// Role within a shared project.
/// Story: ACF-013
/// </summary>
public enum SharedProjectRole
{
    /// <summary>Regular member with read/write access.</summary>
    Member = 0,

    /// <summary>Project owner with full control.</summary>
    Owner = 1
}

/// <summary>
/// Status of a task within a shared project.
/// Story: ACF-013
/// </summary>
public enum SharedProjectTaskStatus
{
    /// <summary>Task is open.</summary>
    Open = 0,

    /// <summary>Task is in progress.</summary>
    InProgress = 1,

    /// <summary>Task is completed.</summary>
    Completed = 2
}

/// <summary>
/// Type of activity in a shared project.
/// Story: ACF-013
/// </summary>
public enum SharedProjectActivityType
{
    /// <summary>Project was created.</summary>
    ProjectCreated = 0,

    /// <summary>A member joined the project.</summary>
    MemberJoined = 1,

    /// <summary>A member left the project.</summary>
    MemberLeft = 2,

    /// <summary>A task was added.</summary>
    TaskAdded = 3,

    /// <summary>A task was updated.</summary>
    TaskUpdated = 4,

    /// <summary>A task was completed.</summary>
    TaskCompleted = 5,

    /// <summary>A link was added.</summary>
    LinkAdded = 6,

    /// <summary>A link was removed.</summary>
    LinkRemoved = 7,

    /// <summary>Project was closed.</summary>
    ProjectClosed = 8
}

/// <summary>
/// Shared project aggregate root.
/// Represents a shared workspace for collaborators to coordinate joint content.
/// Story: ACF-013
/// </summary>
public sealed class SharedProject : AggregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 1000;

    private readonly List<SharedProjectMember> _members = new();
    private readonly List<SharedProjectTask> _tasks = new();
    private readonly List<SharedProjectLink> _links = new();
    private readonly List<SharedProjectActivity> _activities = new();

    /// <summary>
    /// Gets the name of the project.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the project description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public SharedProjectStatus Status { get; private set; }

    /// <summary>
    /// Gets the collaboration request ID that created this project.
    /// </summary>
    public Guid CollaborationRequestId { get; private set; }

    /// <summary>
    /// Gets the user ID of the project owner.
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Gets the date the project was closed, if applicable.
    /// </summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>
    /// Gets the project members.
    /// </summary>
    public IReadOnlyCollection<SharedProjectMember> Members => _members.AsReadOnly();

    /// <summary>
    /// Gets the project tasks.
    /// </summary>
    public IReadOnlyCollection<SharedProjectTask> Tasks => _tasks.AsReadOnly();

    /// <summary>
    /// Gets the project links.
    /// </summary>
    public IReadOnlyCollection<SharedProjectLink> Links => _links.AsReadOnly();

    /// <summary>
    /// Gets the project activity feed.
    /// </summary>
    public IReadOnlyCollection<SharedProjectActivity> Activities => _activities.AsReadOnly();

    private SharedProject() : base()
    {
        Name = string.Empty;
    }

    private SharedProject(string name, string? description, Guid collaborationRequestId, Guid ownerId) : base()
    {
        Name = name;
        Description = description;
        Status = SharedProjectStatus.Active;
        CollaborationRequestId = collaborationRequestId;
        OwnerId = ownerId;
    }

    /// <summary>
    /// Creates a shared project when a collaboration request is accepted.
    /// </summary>
    /// <param name="collaborationRequestId">The collaboration request ID that triggered creation.</param>
    /// <param name="senderId">The sender of the original collaboration request.</param>
    /// <param name="recipientId">The recipient who accepted.</param>
    /// <param name="name">The project name.</param>
    /// <param name="description">Optional project description.</param>
    /// <returns>A new SharedProject instance with both creators as members.</returns>
    public static SharedProject Create(
        Guid collaborationRequestId,
        Guid senderId,
        Guid recipientId,
        string name,
        string? description = null)
    {
        if (collaborationRequestId == Guid.Empty)
            throw new ArgumentException("Collaboration request ID cannot be empty.", nameof(collaborationRequestId));

        if (senderId == Guid.Empty)
            throw new ArgumentException("Sender ID cannot be empty.", nameof(senderId));

        if (recipientId == Guid.Empty)
            throw new ArgumentException("Recipient ID cannot be empty.", nameof(recipientId));

        if (senderId == recipientId)
            throw new ArgumentException("Sender and recipient cannot be the same user.", nameof(recipientId));

        ValidateName(name);
        ValidateDescription(description);

        var project = new SharedProject(name.Trim(), description?.Trim(), collaborationRequestId, senderId);

        // Add both creators as members
        var ownerMember = new SharedProjectMember(project.Id, senderId, SharedProjectRole.Owner);
        project._members.Add(ownerMember);

        var recipientMember = new SharedProjectMember(project.Id, recipientId, SharedProjectRole.Member);
        project._members.Add(recipientMember);

        // Log activity
        project._activities.Add(SharedProjectActivity.Create(
            project.Id, senderId, SharedProjectActivityType.ProjectCreated,
            $"Project '{name.Trim()}' was created"));

        project._activities.Add(SharedProjectActivity.Create(
            project.Id, senderId, SharedProjectActivityType.MemberJoined,
            "Joined the project as owner"));

        project._activities.Add(SharedProjectActivity.Create(
            project.Id, recipientId, SharedProjectActivityType.MemberJoined,
            "Joined the project as member"));

        project.RaiseDomainEvent(new SharedProjectCreatedEvent(
            project.Id, collaborationRequestId, senderId, recipientId));

        return project;
    }

    /// <summary>
    /// Adds a task to the project.
    /// </summary>
    /// <param name="userId">The user adding the task.</param>
    /// <param name="title">The task title.</param>
    /// <param name="description">Optional task description.</param>
    /// <param name="assigneeId">Optional assignee user ID.</param>
    /// <param name="dueDate">Optional due date.</param>
    /// <returns>The created task.</returns>
    public SharedProjectTask AddTask(Guid userId, string title, string? description = null, Guid? assigneeId = null, DateTime? dueDate = null)
    {
        EnsureActive();
        EnsureMember(userId);

        if (assigneeId.HasValue)
            EnsureMember(assigneeId.Value);

        var task = SharedProjectTask.Create(Id, userId, title, description, assigneeId, dueDate);
        _tasks.Add(task);

        _activities.Add(SharedProjectActivity.Create(
            Id, userId, SharedProjectActivityType.TaskAdded,
            $"Added task '{title.Trim()}'"));

        IncrementVersion();

        RaiseDomainEvent(new SharedProjectTaskAddedEvent(Id, task.Id, userId));

        return task;
    }

    /// <summary>
    /// Updates a task in the project.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user updating the task.</param>
    /// <param name="title">The new title.</param>
    /// <param name="description">The new description.</param>
    /// <param name="status">The new status.</param>
    /// <param name="assigneeId">The new assignee.</param>
    /// <param name="dueDate">The new due date.</param>
    public void UpdateTask(Guid taskId, Guid userId, string? title = null, string? description = null,
        SharedProjectTaskStatus? status = null, Guid? assigneeId = null, DateTime? dueDate = null)
    {
        EnsureActive();
        EnsureMember(userId);

        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            throw new InvalidOperationException("Task not found in this project.");

        if (assigneeId.HasValue)
            EnsureMember(assigneeId.Value);

        var wasCompleted = task.Status != SharedProjectTaskStatus.Completed
                           && status == SharedProjectTaskStatus.Completed;

        task.Update(title, description, status, assigneeId, dueDate);

        var activityType = wasCompleted
            ? SharedProjectActivityType.TaskCompleted
            : SharedProjectActivityType.TaskUpdated;

        var message = wasCompleted
            ? $"Completed task '{task.Title}'"
            : $"Updated task '{task.Title}'";

        _activities.Add(SharedProjectActivity.Create(Id, userId, activityType, message));

        IncrementVersion();

        RaiseDomainEvent(new SharedProjectTaskUpdatedEvent(Id, taskId, userId));
    }

    /// <summary>
    /// Adds a link/resource to the project.
    /// </summary>
    /// <param name="userId">The user adding the link.</param>
    /// <param name="title">The link title.</param>
    /// <param name="url">The link URL.</param>
    /// <param name="description">Optional link description.</param>
    /// <returns>The created link.</returns>
    public SharedProjectLink AddLink(Guid userId, string title, string url, string? description = null)
    {
        EnsureActive();
        EnsureMember(userId);

        var link = SharedProjectLink.Create(Id, userId, title, url, description);
        _links.Add(link);

        _activities.Add(SharedProjectActivity.Create(
            Id, userId, SharedProjectActivityType.LinkAdded,
            $"Added link '{title.Trim()}'"));

        IncrementVersion();

        RaiseDomainEvent(new SharedProjectLinkAddedEvent(Id, link.Id, userId));

        return link;
    }

    /// <summary>
    /// Removes a link from the project.
    /// </summary>
    /// <param name="linkId">The link ID to remove.</param>
    /// <param name="userId">The user removing the link.</param>
    public void RemoveLink(Guid linkId, Guid userId)
    {
        EnsureActive();
        EnsureMember(userId);

        var link = _links.FirstOrDefault(l => l.Id == linkId);
        if (link is null)
            throw new InvalidOperationException("Link not found in this project.");

        _links.Remove(link);

        _activities.Add(SharedProjectActivity.Create(
            Id, userId, SharedProjectActivityType.LinkRemoved,
            $"Removed link '{link.Title}'"));

        IncrementVersion();
    }

    /// <summary>
    /// Allows a member to leave the project.
    /// </summary>
    /// <param name="userId">The user leaving.</param>
    public void Leave(Guid userId)
    {
        EnsureActive();
        EnsureMember(userId);

        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            throw new InvalidOperationException("Member not found.");

        _members.Remove(member);

        _activities.Add(SharedProjectActivity.Create(
            Id, userId, SharedProjectActivityType.MemberLeft,
            "Left the project"));

        // If no members remain, close the project
        if (_members.Count == 0)
        {
            Status = SharedProjectStatus.Closed;
            ClosedAt = DateTime.UtcNow;
        }
        else if (userId == OwnerId)
        {
            // Owner left but others remain — transfer ownership
            var newOwner = _members.First();
            OwnerId = newOwner.UserId;
            newOwner.PromoteToOwner();
        }

        IncrementVersion();

        RaiseDomainEvent(new SharedProjectMemberLeftEvent(Id, userId));
    }

    /// <summary>
    /// Closes the project (owner only).
    /// </summary>
    /// <param name="userId">The user closing the project (must be owner).</param>
    public void Close(Guid userId)
    {
        EnsureActive();

        if (userId != OwnerId)
            throw new UnauthorizedAccessException("Only the project owner can close the project.");

        Status = SharedProjectStatus.Closed;
        ClosedAt = DateTime.UtcNow;

        _activities.Add(SharedProjectActivity.Create(
            Id, userId, SharedProjectActivityType.ProjectClosed,
            "Project was closed"));

        IncrementVersion();

        RaiseDomainEvent(new SharedProjectClosedEvent(Id, userId));
    }

    /// <summary>
    /// Checks if a user is a member of this project.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user is a member.</returns>
    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    private void EnsureActive()
    {
        if (Status != SharedProjectStatus.Active)
            throw new InvalidOperationException("Cannot perform this action on a closed project.");
    }

    private void EnsureMember(Guid userId)
    {
        if (!IsMember(userId))
            throw new UnauthorizedAccessException("You are not a member of this project.");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.", nameof(name));

        if (name.Trim().Length > MaxNameLength)
            throw new ArgumentException($"Project name cannot exceed {MaxNameLength} characters.", nameof(name));
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
    }
}
