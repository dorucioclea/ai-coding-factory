using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.Approvals.Commands.ApproveContent;

/// <summary>
/// Command to approve content.
/// Story: ACF-009
/// </summary>
public sealed record ApproveContentCommand(
    Guid ContentItemId,
    Guid TeamId,
    Guid ApproverId,
    string? Feedback
) : IRequest<ContentIdeaResponse>;
