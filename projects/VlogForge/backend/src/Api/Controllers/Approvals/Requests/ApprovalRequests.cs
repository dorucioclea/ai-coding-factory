using System.ComponentModel.DataAnnotations;
using VlogForge.Domain.Entities;

namespace VlogForge.Api.Controllers.Approvals.Requests;

/// <summary>
/// Request to configure workflow settings.
/// Story: ACF-009
/// </summary>
public sealed class ConfigureWorkflowRequest
{
    /// <summary>
    /// Whether approval is required before scheduling.
    /// </summary>
    [Required]
    public bool RequiresApproval { get; set; }

    /// <summary>
    /// User IDs of designated approvers (optional).
    /// If empty, Admins and Owner can approve.
    /// </summary>
    public List<Guid>? ApproverIds { get; set; }
}

/// <summary>
/// Request to submit content for approval.
/// Story: ACF-009
/// </summary>
public sealed class SubmitForApprovalRequest
{
    /// <summary>
    /// The team ID to submit the content to.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }
}

/// <summary>
/// Request to approve content.
/// Story: ACF-009
/// </summary>
public sealed class ApproveContentRequest
{
    /// <summary>
    /// The team ID.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }

    /// <summary>
    /// Optional feedback for the creator.
    /// </summary>
    [MaxLength(ApprovalRecord.MaxFeedbackLength)]
    public string? Feedback { get; set; }
}

/// <summary>
/// Request to request changes on content.
/// Story: ACF-009
/// </summary>
public sealed class RequestChangesRequest
{
    /// <summary>
    /// The team ID.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }

    /// <summary>
    /// Feedback explaining what changes are needed.
    /// </summary>
    [Required]
    [MaxLength(ApprovalRecord.MaxFeedbackLength)]
    public string Feedback { get; set; } = string.Empty;
}
