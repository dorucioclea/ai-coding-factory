using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.ContentIdeas.Requests;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.ContentIdeas.Commands.CreateContentIdea;
using VlogForge.Application.ContentIdeas.Commands.DeleteContentIdea;
using VlogForge.Application.ContentIdeas.Commands.UpdateContentIdea;
using VlogForge.Application.ContentIdeas.Commands.UpdateContentIdeaStatus;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Application.Calendar.Commands.UpdateScheduledDate;
using VlogForge.Application.ContentIdeas.Queries.GetContentIdeaById;
using VlogForge.Application.ContentIdeas.Queries.GetContentIdeas;
using VlogForge.Application.Tasks.Commands.AssignTask;
using VlogForge.Application.Tasks.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Api.Controllers.ContentIdeas;

/// <summary>
/// Content ideas management endpoints.
/// Story: ACF-005
/// </summary>
[ApiController]
[Route("api/content")]
[Authorize]
[Produces("application/json")]
public class ContentIdeasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ContentIdeasController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets all content ideas for the current user.
    /// </summary>
    /// <param name="status">Filter by status (optional).</param>
    /// <param name="platform">Filter by platform tag (optional).</param>
    /// <param name="search">Search term for title/notes (optional).</param>
    /// <returns>List of content ideas.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ContentIdeasListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContentIdeasListResponse>> GetAll(
        [FromQuery] IdeaStatus? status = null,
        [FromQuery] string? platform = null,
        [FromQuery] string? search = null)
    {
        var userId = GetCurrentUserId();
        var query = new GetContentIdeasQuery(userId, status, platform, search);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a content idea by ID.
    /// </summary>
    /// <param name="id">The content idea ID.</param>
    /// <returns>The content idea.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContentIdeaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentIdeaResponse>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var query = new GetContentIdeaByIdQuery(id, userId);
        var result = await _mediator.Send(query);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new content idea.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <returns>The created content idea.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ContentIdeaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContentIdeaResponse>> Create([FromBody] CreateContentIdeaRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new CreateContentIdeaCommand(
            userId,
            request.Title,
            request.Notes,
            request.PlatformTags);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates a content idea.
    /// </summary>
    /// <param name="id">The content idea ID.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated content idea.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContentIdeaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentIdeaResponse>> Update(Guid id, [FromBody] UpdateContentIdeaRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateContentIdeaCommand(
            id,
            userId,
            request.Title,
            request.Notes,
            request.PlatformTags,
            request.Status);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Updates a content idea's status.
    /// </summary>
    /// <param name="id">The content idea ID.</param>
    /// <param name="request">The status update request.</param>
    /// <returns>The updated content idea.</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ContentIdeaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentIdeaResponse>> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateContentIdeaStatusCommand(id, userId, request.Status);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a content idea (soft delete).
    /// </summary>
    /// <param name="id">The content idea ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteContentIdeaCommand(id, userId);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Updates a content idea's scheduled date.
    /// Story: ACF-006
    /// </summary>
    /// <param name="id">The content idea ID.</param>
    /// <param name="request">The schedule update request.</param>
    /// <returns>The updated content idea.</returns>
    [HttpPatch("{id:guid}/schedule")]
    [ProducesResponseType(typeof(ContentIdeaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentIdeaResponse>> UpdateSchedule(Guid id, [FromBody] UpdateScheduleRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateScheduledDateCommand(id, userId, request.ScheduledDate);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Assigns a content item to a team member.
    /// AC1: Assign Task - team lead can assign content to team member with due date.
    /// Story: ACF-008
    /// </summary>
    /// <param name="id">The content item ID.</param>
    /// <param name="request">The assignment request.</param>
    /// <returns>The created task assignment.</returns>
    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(typeof(TaskAssignmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskAssignmentResponse>> AssignTask(Guid id, [FromBody] AssignTaskToMemberRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new AssignTaskCommand(
            id,
            request.TeamId,
            request.AssigneeId,
            userId,
            request.DueDate,
            request.Notes);

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtRoute("GetTaskById", new { id = result.Id }, result);
        }
        catch (ContentItemNotFoundException)
        {
            return NotFound(new { error = "Content item not found." });
        }
        catch (TeamNotFoundException)
        {
            return NotFound(new { error = "Team not found." });
        }
        catch (TeamMemberNotFoundException)
        {
            return NotFound(new { error = "Assignee is not a team member." });
        }
        catch (TeamAccessDeniedException)
        {
            return Forbid();
        }
        catch (BusinessRuleException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }
        return userId;
    }
}
