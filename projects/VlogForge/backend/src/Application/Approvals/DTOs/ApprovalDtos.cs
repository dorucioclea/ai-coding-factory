using VlogForge.Domain.Entities;

namespace VlogForge.Application.Approvals.DTOs;

/// <summary>
/// Response DTO for an approval record.
/// Story: ACF-009
/// </summary>
public sealed class ApprovalRecordResponse
{
    public Guid Id { get; init; }
    public Guid ContentItemId { get; init; }
    public Guid TeamId { get; init; }
    public Guid ActorId { get; init; }
    public ApprovalAction Action { get; init; }
    public string? Feedback { get; init; }
    public IdeaStatus PreviousStatus { get; init; }
    public IdeaStatus NewStatus { get; init; }
    public DateTime CreatedAt { get; init; }

    public static ApprovalRecordResponse FromEntity(ApprovalRecord record) => new()
    {
        Id = record.Id,
        ContentItemId = record.ContentItemId,
        TeamId = record.TeamId,
        ActorId = record.ActorId,
        Action = record.Action,
        Feedback = record.Feedback,
        PreviousStatus = record.PreviousStatus,
        NewStatus = record.NewStatus,
        CreatedAt = record.CreatedAt
    };
}

/// <summary>
/// Response DTO for approval history.
/// Story: ACF-009
/// </summary>
public sealed class ApprovalHistoryResponse
{
    public Guid ContentItemId { get; init; }
    public IReadOnlyList<ApprovalRecordResponse> Records { get; init; } = Array.Empty<ApprovalRecordResponse>();
}

/// <summary>
/// Response DTO for pending approvals list.
/// Story: ACF-009
/// </summary>
public sealed class PendingApprovalsResponse
{
    public IReadOnlyList<PendingApprovalItem> Items { get; init; } = Array.Empty<PendingApprovalItem>();
    public int TotalCount { get; init; }
}

/// <summary>
/// Item in pending approvals list.
/// Story: ACF-009
/// </summary>
public sealed class PendingApprovalItem
{
    public Guid ContentItemId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public Guid SubmittedByUserId { get; init; }
    public DateTime SubmittedAt { get; init; }
    public IReadOnlyList<string> PlatformTags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Response DTO for workflow settings.
/// Story: ACF-009
/// </summary>
public sealed class WorkflowSettingsResponse
{
    public Guid TeamId { get; init; }
    public bool RequiresApproval { get; init; }
    public IReadOnlyList<Guid> ApproverIds { get; init; } = Array.Empty<Guid>();
}
