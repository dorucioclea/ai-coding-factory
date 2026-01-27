using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.Teams.Requests;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.Commands.AcceptInvitation;
using VlogForge.Application.Teams.Commands.ChangeMemberRole;
using VlogForge.Application.Teams.Commands.CreateTeam;
using VlogForge.Application.Teams.Commands.InviteMember;
using VlogForge.Application.Teams.Commands.RemoveMember;
using VlogForge.Application.Teams.DTOs;
using VlogForge.Application.Teams.Queries.GetTeam;
using VlogForge.Application.Teams.Queries.GetTeamMembers;
using VlogForge.Application.Teams.Queries.GetUserTeams;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Api.Controllers.Teams;

/// <summary>
/// Team management endpoints.
/// Story: ACF-007
/// </summary>
[ApiController]
[Route("api/teams")]
[Authorize]
[Produces("application/json")]
public class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public TeamsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets teams the current user belongs to with pagination.
    /// </summary>
    /// <param name="page">Page number (1-based, default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100).</param>
    /// <returns>Paginated list of teams.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(TeamListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TeamListResponse>> GetUserTeams(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var query = new GetUserTeamsQuery(userId, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a team by ID.
    /// </summary>
    /// <param name="id">The team ID.</param>
    /// <returns>The team with members.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamWithMembersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamWithMembersResponse>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var query = new GetTeamQuery(id, userId);

        try
        {
            var result = await _mediator.Send(query);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Gets the members of a team.
    /// </summary>
    /// <param name="id">The team ID.</param>
    /// <returns>List of team members.</returns>
    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyList<TeamMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TeamMemberResponse>>> GetMembers(Guid id)
    {
        var userId = GetCurrentUserId();
        var query = new GetTeamMembersQuery(id, userId);

        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Creates a new team.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <returns>The created team.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeamResponse>> Create([FromBody] CreateTeamRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new CreateTeamCommand(userId, request.Name, request.Description);

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (TeamNameAlreadyExistsException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Invites a member to a team.
    /// </summary>
    /// <param name="id">The team ID.</param>
    /// <param name="request">The invitation request.</param>
    /// <returns>The invitation details.</returns>
    [HttpPost("{id:guid}/invite")]
    [ProducesResponseType(typeof(TeamInvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeamInvitationResponse>> InviteMember(Guid id, [FromBody] InviteMemberRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new InviteMemberCommand(id, userId, request.Email, request.Role);

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Accepts a team invitation.
    /// </summary>
    /// <param name="request">The acceptance request.</param>
    /// <returns>The new member details.</returns>
    [HttpPost("invitations/accept")]
    [ProducesResponseType(typeof(TeamMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeamMemberResponse>> AcceptInvitation([FromBody] AcceptInvitationRequest request)
    {
        var userId = GetCurrentUserId();
        var userEmail = GetCurrentUserEmail();
        var command = new AcceptInvitationCommand(request.Token, userId, userEmail);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found") || ex.Message.Contains("expired"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already a member"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Changes a team member's role.
    /// </summary>
    /// <param name="id">The team ID.</param>
    /// <param name="userId">The member's user ID.</param>
    /// <param name="request">The role change request.</param>
    /// <returns>The updated member details.</returns>
    [HttpPut("{id:guid}/members/{userId:guid}/role")]
    [ProducesResponseType(typeof(TeamMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamMemberResponse>> ChangeMemberRole(
        Guid id,
        Guid userId,
        [FromBody] ChangeMemberRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var command = new ChangeMemberRoleCommand(id, userId, request.NewRole, currentUserId);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Removes a member from a team.
    /// </summary>
    /// <param name="id">The team ID.</param>
    /// <param name="userId">The member's user ID to remove.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var command = new RemoveMemberCommand(id, userId, currentUserId);

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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

    private string GetCurrentUserEmail()
    {
        var email = _currentUserService.Email;
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException("User email not found in claims.");
        }
        return email;
    }
}
