using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.SharedProjects.Requests;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.Commands.AddProjectLink;
using VlogForge.Application.SharedProjects.Commands.AddProjectTask;
using VlogForge.Application.SharedProjects.Commands.CloseProject;
using VlogForge.Application.SharedProjects.Commands.LeaveProject;
using VlogForge.Application.SharedProjects.Commands.UpdateProjectTask;
using VlogForge.Application.SharedProjects.DTOs;
using VlogForge.Application.SharedProjects.Queries.GetProjectActivity;
using VlogForge.Application.SharedProjects.Queries.GetSharedProject;
using VlogForge.Application.SharedProjects.Queries.GetUserSharedProjects;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Api.Controllers.SharedProjects;

/// <summary>
/// Shared project endpoints for collaboration workspaces.
/// Story: ACF-013
/// </summary>
[ApiController]
[Route("api/projects")]
[Authorize]
[Produces("application/json")]
public class SharedProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public SharedProjectsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the user's shared projects.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SharedProjectListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SharedProjectListResponse>> GetUserProjects(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        SharedProjectStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<SharedProjectStatus>(status, ignoreCase: true, out var parsedStatus))
                return BadRequest(new { error = $"Invalid status value: '{status}'." });
            statusFilter = parsedStatus;
        }

        var query = new GetUserSharedProjectsQuery(userId, statusFilter, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a shared project by ID with full details.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SharedProjectDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedProjectDetailResponse>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            var query = new GetSharedProjectQuery(id, userId);
            var result = await _mediator.Send(query);
            if (result is null)
                return NotFound(new { error = "Shared project not found." });
            return Ok(result);
        }
        catch (SharedProjectAccessDeniedException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Adds a task to a shared project.
    /// </summary>
    [HttpPost("{id:guid}/tasks")]
    [ProducesResponseType(typeof(SharedProjectTaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedProjectTaskResponse>> AddTask(
        Guid id,
        [FromBody] AddProjectTaskRequest request)
    {
        var userId = GetCurrentUserId();

        try
        {
            var command = new AddProjectTaskCommand(
                id, userId, request.Title, request.Description,
                request.AssigneeId, request.DueDate);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, result);
        }
        catch (SharedProjectNotFoundException)
        {
            return NotFound(new { error = "Shared project not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates a task in a shared project.
    /// </summary>
    [HttpPut("{id:guid}/tasks/{taskId:guid}")]
    [ProducesResponseType(typeof(SharedProjectDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedProjectDetailResponse>> UpdateTask(
        Guid id,
        Guid taskId,
        [FromBody] UpdateProjectTaskRequest request)
    {
        var userId = GetCurrentUserId();

        try
        {
            var command = new UpdateProjectTaskCommand(
                id, taskId, userId, request.Title, request.Description,
                request.Status, request.AssigneeId, request.DueDate);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (SharedProjectNotFoundException)
        {
            return NotFound(new { error = "Shared project not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Adds a link to a shared project.
    /// </summary>
    [HttpPost("{id:guid}/links")]
    [ProducesResponseType(typeof(SharedProjectLinkResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedProjectLinkResponse>> AddLink(
        Guid id,
        [FromBody] AddProjectLinkRequest request)
    {
        var userId = GetCurrentUserId();

        try
        {
            var command = new AddProjectLinkCommand(
                id, userId, request.Title, request.Url, request.Description);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, result);
        }
        catch (SharedProjectNotFoundException)
        {
            return NotFound(new { error = "Shared project not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the activity feed for a shared project.
    /// </summary>
    [HttpGet("{id:guid}/activity")]
    [ProducesResponseType(typeof(SharedProjectActivityListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedProjectActivityListResponse>> GetActivity(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        try
        {
            var query = new GetProjectActivityQuery(id, userId, page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (SharedProjectNotFoundException)
        {
            return NotFound(new { error = "Shared project not found." });
        }
        catch (SharedProjectAccessDeniedException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Leaves a shared project.
    /// </summary>
    [HttpPost("{id:guid}/leave")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LeaveProject(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            var command = new LeaveProjectCommand(id, userId);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (SharedProjectNotFoundException)
        {
            return NotFound(new { error = "Shared project not found." });
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
    /// Closes a shared project (owner only).
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseProject(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            var command = new CloseProjectCommand(id, userId);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (SharedProjectNotFoundException)
        {
            return NotFound(new { error = "Shared project not found." });
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
