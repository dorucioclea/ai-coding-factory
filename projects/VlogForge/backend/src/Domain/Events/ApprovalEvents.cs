using VlogForge.Domain.Common;
using VlogForge.Domain.Entities;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when content is submitted for approval.
/// Story: ACF-009
/// </summary>
public sealed class ContentSubmittedForApprovalEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid TeamId { get; }
    public Guid SubmittedByUserId { get; }
    public IdeaStatus PreviousStatus { get; }

    public ContentSubmittedForApprovalEvent(
        Guid contentItemId,
        Guid teamId,
        Guid submittedByUserId,
        IdeaStatus previousStatus)
    {
        ContentItemId = contentItemId;
        TeamId = teamId;
        SubmittedByUserId = submittedByUserId;
        PreviousStatus = previousStatus;
    }
}

/// <summary>
/// Event raised when content is approved.
/// Story: ACF-009
/// </summary>
public sealed class ContentApprovedEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid TeamId { get; }
    public Guid ApprovedByUserId { get; }
    public Guid ContentOwnerId { get; }
    public string? Feedback { get; }

    public ContentApprovedEvent(
        Guid contentItemId,
        Guid teamId,
        Guid approvedByUserId,
        Guid contentOwnerId,
        string? feedback = null)
    {
        ContentItemId = contentItemId;
        TeamId = teamId;
        ApprovedByUserId = approvedByUserId;
        ContentOwnerId = contentOwnerId;
        Feedback = feedback;
    }
}

/// <summary>
/// Event raised when changes are requested on content.
/// Story: ACF-009
/// </summary>
public sealed class ChangesRequestedEvent : DomainEvent
{
    public Guid ContentItemId { get; }
    public Guid TeamId { get; }
    public Guid RequestedByUserId { get; }
    public Guid ContentOwnerId { get; }
    public string Feedback { get; }

    public ChangesRequestedEvent(
        Guid contentItemId,
        Guid teamId,
        Guid requestedByUserId,
        Guid contentOwnerId,
        string feedback)
    {
        ContentItemId = contentItemId;
        TeamId = teamId;
        RequestedByUserId = requestedByUserId;
        ContentOwnerId = contentOwnerId;
        Feedback = feedback;
    }
}

/// <summary>
/// Event raised when team workflow settings are configured.
/// Story: ACF-009
/// </summary>
public sealed class TeamWorkflowConfiguredEvent : DomainEvent
{
    public Guid TeamId { get; }
    public Guid ConfiguredByUserId { get; }
    public bool RequiresApproval { get; }
    public IReadOnlyList<Guid> ApproverIds { get; }

    public TeamWorkflowConfiguredEvent(
        Guid teamId,
        Guid configuredByUserId,
        bool requiresApproval,
        IReadOnlyList<Guid> approverIds)
    {
        TeamId = teamId;
        ConfiguredByUserId = configuredByUserId;
        RequiresApproval = requiresApproval;
        ApproverIds = approverIds;
    }
}
