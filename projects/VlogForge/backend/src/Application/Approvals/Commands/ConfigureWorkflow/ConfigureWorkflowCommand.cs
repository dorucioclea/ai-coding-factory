using MediatR;
using VlogForge.Application.Approvals.DTOs;

namespace VlogForge.Application.Approvals.Commands.ConfigureWorkflow;

/// <summary>
/// Command to configure approval workflow settings for a team.
/// Story: ACF-009
/// </summary>
public sealed record ConfigureWorkflowCommand(
    Guid TeamId,
    Guid UserId,
    bool RequiresApproval,
    IReadOnlyList<Guid>? ApproverIds
) : IRequest<WorkflowSettingsResponse>;
