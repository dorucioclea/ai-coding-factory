using VlogForge.Domain.Common;

namespace VlogForge.Domain.Events;

/// <summary>
/// Raised when a shared project is created.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectCreatedEvent(
    Guid projectId, Guid collaborationRequestId, Guid senderId, Guid recipientId) : DomainEvent
{
    public Guid ProjectId { get; } = projectId;
    public Guid CollaborationRequestId { get; } = collaborationRequestId;
    public Guid SenderId { get; } = senderId;
    public Guid RecipientId { get; } = recipientId;
}

/// <summary>
/// Raised when a task is added to a shared project.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectTaskAddedEvent(
    Guid projectId, Guid taskId, Guid userId) : DomainEvent
{
    public Guid ProjectId { get; } = projectId;
    public Guid TaskId { get; } = taskId;
    public Guid UserId { get; } = userId;
}

/// <summary>
/// Raised when a task is updated in a shared project.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectTaskUpdatedEvent(
    Guid projectId, Guid taskId, Guid userId) : DomainEvent
{
    public Guid ProjectId { get; } = projectId;
    public Guid TaskId { get; } = taskId;
    public Guid UserId { get; } = userId;
}

/// <summary>
/// Raised when a link is added to a shared project.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectLinkAddedEvent(
    Guid projectId, Guid linkId, Guid userId) : DomainEvent
{
    public Guid ProjectId { get; } = projectId;
    public Guid LinkId { get; } = linkId;
    public Guid UserId { get; } = userId;
}

/// <summary>
/// Raised when a member leaves a shared project.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectMemberLeftEvent(
    Guid projectId, Guid userId) : DomainEvent
{
    public Guid ProjectId { get; } = projectId;
    public Guid UserId { get; } = userId;
}

/// <summary>
/// Raised when a shared project is closed.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectClosedEvent(
    Guid projectId, Guid closedByUserId) : DomainEvent
{
    public Guid ProjectId { get; } = projectId;
    public Guid ClosedByUserId { get; } = closedByUserId;
}
