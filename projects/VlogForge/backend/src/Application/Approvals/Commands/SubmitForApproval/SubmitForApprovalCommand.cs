using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.Approvals.Commands.SubmitForApproval;

/// <summary>
/// Command to submit content for approval.
/// Story: ACF-009
/// </summary>
public sealed record SubmitForApprovalCommand(
    Guid ContentItemId,
    Guid TeamId,
    Guid UserId
) : IRequest<ContentIdeaResponse>;
