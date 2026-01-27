using VlogForge.Domain.Common;
using VlogForge.Domain.Entities;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when a team is created.
/// Story: ACF-007
/// </summary>
public sealed class TeamCreatedEvent : DomainEvent
{
    public Guid TeamId { get; }
    public Guid OwnerId { get; }
    public string Name { get; }

    public TeamCreatedEvent(Guid teamId, Guid ownerId, string name)
    {
        TeamId = teamId;
        OwnerId = ownerId;
        Name = name;
    }
}

/// <summary>
/// Event raised when a team is updated.
/// Story: ACF-007
/// </summary>
public sealed class TeamUpdatedEvent : DomainEvent
{
    public Guid TeamId { get; }
    public Guid UpdatedByUserId { get; }

    public TeamUpdatedEvent(Guid teamId, Guid updatedByUserId)
    {
        TeamId = teamId;
        UpdatedByUserId = updatedByUserId;
    }
}

/// <summary>
/// Event raised when a member is invited to a team.
/// Note: Email is intentionally not stored to avoid PII in event logs.
/// Story: ACF-007
/// </summary>
public sealed class TeamMemberInvitedEvent : DomainEvent
{
    public Guid TeamId { get; }
    public Guid InvitationId { get; }
    public TeamRole Role { get; }
    public Guid InvitedByUserId { get; }

    public TeamMemberInvitedEvent(
        Guid teamId,
        Guid invitationId,
        TeamRole role,
        Guid invitedByUserId)
    {
        TeamId = teamId;
        InvitationId = invitationId;
        Role = role;
        InvitedByUserId = invitedByUserId;
    }
}

/// <summary>
/// Event raised when a member joins a team.
/// Story: ACF-007
/// </summary>
public sealed class TeamMemberJoinedEvent : DomainEvent
{
    public Guid TeamId { get; }
    public Guid UserId { get; }
    public TeamRole Role { get; }

    public TeamMemberJoinedEvent(Guid teamId, Guid userId, TeamRole role)
    {
        TeamId = teamId;
        UserId = userId;
        Role = role;
    }
}

/// <summary>
/// Event raised when a member's role changes.
/// Story: ACF-007
/// </summary>
public sealed class TeamMemberRoleChangedEvent : DomainEvent
{
    public Guid TeamId { get; }
    public Guid UserId { get; }
    public TeamRole OldRole { get; }
    public TeamRole NewRole { get; }
    public Guid ChangedByUserId { get; }

    public TeamMemberRoleChangedEvent(
        Guid teamId,
        Guid userId,
        TeamRole oldRole,
        TeamRole newRole,
        Guid changedByUserId)
    {
        TeamId = teamId;
        UserId = userId;
        OldRole = oldRole;
        NewRole = newRole;
        ChangedByUserId = changedByUserId;
    }
}

/// <summary>
/// Event raised when a member is removed from a team.
/// Story: ACF-007
/// </summary>
public sealed class TeamMemberRemovedEvent : DomainEvent
{
    public Guid TeamId { get; }
    public Guid UserId { get; }
    public Guid RemovedByUserId { get; }

    public TeamMemberRemovedEvent(Guid teamId, Guid userId, Guid removedByUserId)
    {
        TeamId = teamId;
        UserId = userId;
        RemovedByUserId = removedByUserId;
    }
}

/// <summary>
/// Event raised when team ownership is transferred.
/// Story: ACF-007
/// </summary>
public sealed class TeamOwnershipTransferredEvent : DomainEvent
{
    public Guid TeamId { get; }
    public Guid PreviousOwnerId { get; }
    public Guid NewOwnerId { get; }

    public TeamOwnershipTransferredEvent(Guid teamId, Guid previousOwnerId, Guid newOwnerId)
    {
        TeamId = teamId;
        PreviousOwnerId = previousOwnerId;
        NewOwnerId = newOwnerId;
    }
}
