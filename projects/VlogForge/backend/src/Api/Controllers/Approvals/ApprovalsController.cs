using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.Approvals.Requests;
using VlogForge.Application.Approvals.Commands.ApproveContent;
using VlogForge.Application.Approvals.Commands.ConfigureWorkflow;
using VlogForge.Application.Approvals.Commands.RequestChanges;
using VlogForge.Application.Approvals.Commands.SubmitForApproval;
using VlogForge.Application.Approvals.DTOs;
using VlogForge.Application.Approvals.Queries.GetApprovalHistory;
using VlogForge.Application.Approvals.Queries.GetPendingApprovals;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Api.Controllers.Approvals;

/// <summary>
/// Approval workflow management endpoints.
/// Story: ACF-009
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class ApprovalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ApprovalsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Configures workflow settings for a team.
    /// AC1: Configure Workflow - team owner/admin can enable/disable approval and set approvers.
    /// </summary>
    /// <param name="teamId">The team ID.</param>
    /// <param name="request">The workflow configuration.</param>
    /// <returns>The updated workflow settings.</returns>
    [HttpPost("teams/{teamId:guid}/workflow")]
    [ProducesResponseType(typeof(WorkflowSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowSettingsResponse>> ConfigureWorkflow(
        Guid teamId,
        [FromBody] ConfigureWorkflowRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new ConfigureWorkflowCommand(teamId, userId, request.RequiresApproval, request.ApproverIds);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (TeamNotFoundException)
        {
            return NotFound(new { error = "Team not found." });
        }
        catch (System.UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets pending approvals for a team.
    /// </summary>
    /// <param name="teamId">The team ID.</param>
    /// <returns>List of content pending approval.</returns>
    [HttpGet("teams/{teamId:guid}/pending-approvals")]
    [ProducesResponseType(typeof(PendingApprovalsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PendingApprovalsResponse>> GetPendingApprovals(Guid teamId)
    {
        var userId = GetCurrentUserId();
        var query = new GetPendingApprovalsQuery(teamId, userId);

        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (TeamNotFoundException)
        {
            return NotFound(new { error = "Team not found." });
        }
        catch (TeamAccessDeniedException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Submits content for approval.
    /// AC2: Submit for Approval - content moves to "In Review" status.
    /// </summary>
    /// <param name="id">The content item ID.</param>
    /// <param name="request">The submission request.</param>
    /// <returns>The updated content item.</returns>
    [HttpPost("content/{id:guid}/submit")]
    [ProducesResponseType(typeof(ContentIdeaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentIdeaResponse>> SubmitForApproval(
        Guid id,
        [FromBody] SubmitForApprovalRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new SubmitForApprovalCommand(id, request.TeamId, userId);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ContentItemNotFoundException)
        {
            return NotFound(new { error = "Content item not found." });
        }
        catch (TeamNotFoundException)
        {
            return NotFound(new { error = "Team not found." });
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (TeamAccessDeniedException)
        {
            return Forbid();
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Approves content.
    /// AC3: Approve Content - status changes to "Approved".
    /// </summary>
    /// <param name="id">The content item ID.</param>
    /// <param name="request">The approval request.</param>
    /// <returns>The updated content item.</returns>
    [HttpPost("content/{id:guid}/approve")]
    [ProducesResponseType(typeof(ContentIdeaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentIdeaResponse>> ApproveContent(
        Guid id,
        [FromBody] ApproveContentRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new ApproveContentCommand(id, request.TeamId, userId, request.Feedback);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ContentItemNotFoundException)
        {
            return NotFound(new { error = "Content item not found." });
        }
        catch (TeamNotFoundException)
        {
            return NotFound(new { error = "Team not found." });
        }
        catch (TeamAccessDeniedException)
        {
            return Forbid();
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Requests changes on content.
    /// AC4: Request Changes - status returns to "Draft" with feedback.
    /// </summary>
    /// <param name="id">The content item ID.</param>
    /// <param name="request">The request changes request.</param>
    /// <returns>The updated content item.</returns>
    [HttpPost("content/{id:guid}/request-changes")]
    [ProducesResponseType(typeof(ContentIdeaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentIdeaResponse>> RequestChanges(
        Guid id,
        [FromBody] RequestChangesRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new RequestChangesCommand(id, request.TeamId, userId, request.Feedback);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ContentItemNotFoundException)
        {
            return NotFound(new { error = "Content item not found." });
        }
        catch (TeamNotFoundException)
        {
            return NotFound(new { error = "Team not found." });
        }
        catch (TeamAccessDeniedException)
        {
            return Forbid();
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets approval history for a content item.
    /// AC5: Approval History - full timeline visible.
    /// </summary>
    /// <param name="id">The content item ID.</param>
    /// <returns>The approval history.</returns>
    [HttpGet("content/{id:guid}/approval-history")]
    [ProducesResponseType(typeof(ApprovalHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalHistoryResponse>> GetApprovalHistory(Guid id)
    {
        var userId = GetCurrentUserId();
        var query = new GetApprovalHistoryQuery(id, userId);

        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (ContentItemNotFoundException)
        {
            return NotFound(new { error = "Content item not found." });
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new System.UnauthorizedAccessException("User ID not found in claims.");
        }
        return userId;
    }
}
