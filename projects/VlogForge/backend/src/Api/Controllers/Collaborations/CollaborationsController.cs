using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VlogForge.Api.Controllers.Collaborations.Requests;
using VlogForge.Application.Collaborations.Commands.AcceptCollaborationRequest;
using VlogForge.Application.Collaborations.Commands.DeclineCollaborationRequest;
using VlogForge.Application.Collaborations.Commands.SendCollaborationRequest;
using VlogForge.Application.Collaborations.DTOs;
using VlogForge.Application.Collaborations.Queries.GetCollaborationInbox;
using VlogForge.Application.Collaborations.Queries.GetSentCollaborationRequests;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Api.Controllers.Collaborations;

/// <summary>
/// Collaboration request endpoints.
/// Story: ACF-011
/// </summary>
[ApiController]
[Route("api/collaborations")]
[Authorize]
[Produces("application/json")]
public class CollaborationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CollaborationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Sends a collaboration request to another creator.
    /// </summary>
    [HttpPost("request")]
    [ProducesResponseType(typeof(CollaborationRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CollaborationRequestResponse>> SendRequest(
        [FromBody] SendCollaborationRequestRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new SendCollaborationRequestCommand(
                userId,
                request.RecipientId,
                request.Message);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetInbox), null, result);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (RecipientNotOpenToCollaborationsException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (CollaborationRateLimitExceededException ex)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { error = ex.Message });
        }
        catch (DuplicateCollaborationRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the user's collaboration inbox (received requests).
    /// </summary>
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(CollaborationRequestListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CollaborationRequestListResponse>> GetInbox(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        // Validate pagination bounds
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        CollaborationRequestStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<CollaborationRequestStatus>(status, ignoreCase: true, out var parsedStatus))
                return BadRequest(new { error = $"Invalid status value: '{status}'." });
            statusFilter = parsedStatus;
        }

        var query = new GetCollaborationInboxQuery(userId, statusFilter, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets the user's sent collaboration requests.
    /// </summary>
    [HttpGet("sent")]
    [ProducesResponseType(typeof(CollaborationRequestListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CollaborationRequestListResponse>> GetSent(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        // Validate pagination bounds
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        CollaborationRequestStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<CollaborationRequestStatus>(status, ignoreCase: true, out var parsedStatus))
                return BadRequest(new { error = $"Invalid status value: '{status}'." });
            statusFilter = parsedStatus;
        }

        var query = new GetSentCollaborationRequestsQuery(userId, statusFilter, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Accepts a collaboration request.
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(typeof(CollaborationRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollaborationRequestResponse>> AcceptRequest(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            var command = new AcceptCollaborationRequestCommand(id, userId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (CollaborationRequestNotFoundException)
        {
            return NotFound(new { error = "Collaboration request not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Declines a collaboration request.
    /// </summary>
    [HttpPost("{id:guid}/decline")]
    [ProducesResponseType(typeof(CollaborationRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollaborationRequestResponse>> DeclineRequest(
        Guid id,
        [FromBody] DeclineCollaborationRequestRequest? request = null)
    {
        var userId = GetCurrentUserId();

        try
        {
            var command = new DeclineCollaborationRequestCommand(id, userId, request?.Reason);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (CollaborationRequestNotFoundException)
        {
            return NotFound(new { error = "Collaboration request not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in claims.");
        return userId;
    }
}
