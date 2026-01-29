using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.Tasks.Requests;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Tasks.Commands.AddTaskComment;
using VlogForge.Application.Tasks.Commands.AssignTask;
using VlogForge.Application.Tasks.Commands.UpdateTaskStatus;
using VlogForge.Application.Tasks.DTOs;
using VlogForge.Application.Tasks.Queries.GetMyTasks;
using VlogForge.Application.Tasks.Queries.GetTaskById;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Api.Controllers.Tasks;

/// <summary>
/// Task assignment management endpoints.
/// Stories: ACF-008, ACF-014
/// </summary>
[ApiController]
[Route("api/tasks")]
[Authorize]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public TasksController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets tasks assigned to the current user.
    /// ACF-008 AC2: View Assigned Tasks - sorted by due date with overdue highlighting.
    /// ACF-014 AC1-AC3: Task view grouped by status, sorted by due date.
    /// </summary>
    /// <param name="page">Page number (1-based, default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100).</param>
    /// <param name="status">Optional status filter (0=NotStarted, 1=InProgress, 2=Completed).</param>
    /// <param name="sortBy">Sort field: dueDate (default), createdAt, status.</param>
    /// <param name="sortDirection">Sort direction: asc (default) or desc.</param>
    /// <returns>Paginated list of tasks.</returns>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskListResponse>> GetMyTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] AssignmentStatus? status = null,
        [FromQuery] string sortBy = "dueDate",
        [FromQuery] string sortDirection = "asc")
    {
        var userId = GetCurrentUserId();
        var query = new GetMyTasksQuery(userId, page, pageSize, status, sortBy, sortDirection);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a task by ID.
    /// ACF-014 AC5: Task details with comments and history.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="includeComments">Whether to include comments (default: true).</param>
    /// <param name="includeHistory">Whether to include history (default: false).</param>
    /// <returns>The task details.</returns>
    [HttpGet("{id:guid}", Name = "GetTaskById")]
    [ProducesResponseType(typeof(TaskAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskAssignmentResponse>> GetById(
        Guid id,
        [FromQuery] bool includeComments = true,
        [FromQuery] bool includeHistory = false)
    {
        var userId = GetCurrentUserId();
        var query = new GetTaskByIdQuery(id, userId, includeComments, includeHistory);

        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (TaskAssignmentNotFoundException)
        {
            return NotFound(new { error = "Task not found." });
        }
        catch (TeamAccessDeniedException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Updates the status of a task.
    /// AC3: Update Task Status - validates status transitions and notifies team lead on completion.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The status update request.</param>
    /// <returns>The updated task.</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskAssignmentResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateTaskStatusCommand(id, request.Status, userId);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (TaskAssignmentNotFoundException)
        {
            return NotFound(new { error = "Task not found." });
        }
        catch (TaskAccessDeniedException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Adds a comment to a task.
    /// AC4: Task Comments - supports threaded comments visible to all team members.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The comment request.</param>
    /// <returns>The created comment.</returns>
    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(TaskCommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskCommentResponse>> AddComment(
        Guid id,
        [FromBody] AddTaskCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new AddTaskCommentCommand(id, userId, request.Content, request.ParentCommentId);

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, result);
        }
        catch (TaskAssignmentNotFoundException)
        {
            return NotFound(new { error = "Task not found." });
        }
        catch (TeamNotFoundException)
        {
            return NotFound(new { error = "Team not found." });
        }
        catch (TeamAccessDeniedException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
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
        {
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }
        return userId;
    }
}
