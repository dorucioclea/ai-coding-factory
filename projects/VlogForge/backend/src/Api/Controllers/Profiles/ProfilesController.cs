using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.Profiles.Requests;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.Commands.CreateProfile;
using VlogForge.Application.Profiles.Commands.SetCollaborationSettings;
using VlogForge.Application.Profiles.Commands.UpdateProfile;
using VlogForge.Application.Profiles.Commands.UploadProfilePicture;
using VlogForge.Application.Profiles.DTOs;
using VlogForge.Application.Profiles.Queries.GetMyProfile;
using VlogForge.Application.Profiles.Queries.GetProfileByUsername;

namespace VlogForge.Api.Controllers.Profiles;

/// <summary>
/// Creator profile management endpoints.
/// Story: ACF-002
/// </summary>
[ApiController]
[Route("api/profiles")]
[Produces("application/json")]
public class ProfilesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ProfilesController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets a public profile by username.
    /// </summary>
    /// <param name="username">The username to look up.</param>
    /// <returns>The public profile information.</returns>
    [HttpGet("{username}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicProfileResponse>> GetByUsername(string username)
    {
        var query = new GetProfileByUsernameQuery(username);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <returns>The user's profile or 404 if no profile exists.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CreatorProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatorProfileResponse>> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var query = new GetMyProfileQuery(userId);
        var result = await _mediator.Send(query);

        if (result is null)
        {
            return NotFound("Profile not found. Please create a profile first.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new profile for the current user.
    /// </summary>
    /// <param name="request">Profile creation details.</param>
    /// <returns>The created profile.</returns>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(CreatorProfileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatorProfileResponse>> CreateProfile([FromBody] CreateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new CreateProfileCommand(
            userId,
            request.Username,
            request.DisplayName,
            request.Bio);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMyProfile), result);
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="request">Profile update details.</param>
    /// <returns>The updated profile.</returns>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(CreatorProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatorProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateProfileCommand(
            userId,
            request.DisplayName,
            request.Bio,
            request.NicheTags,
            request.OpenToCollaborations,
            request.CollaborationPreferences);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Uploads a profile picture for the current user.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <returns>The updated profile.</returns>
    [HttpPost("me/avatar")]
    [Authorize]
    [ProducesResponseType(typeof(CreatorProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CreatorProfileResponse>> UploadProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var userId = GetCurrentUserId();
        await using var stream = file.OpenReadStream();

        var command = new UploadProfilePictureCommand(
            userId,
            stream,
            file.FileName,
            file.ContentType);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Sets collaboration availability for the current user.
    /// </summary>
    /// <param name="request">Collaboration settings.</param>
    /// <returns>The updated profile.</returns>
    [HttpPut("me/collaboration")]
    [Authorize]
    [ProducesResponseType(typeof(CreatorProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatorProfileResponse>> SetCollaborationSettings([FromBody] SetCollaborationSettingsRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new SetCollaborationSettingsCommand(
            userId,
            request.OpenToCollaborations,
            request.CollaborationPreferences);

        var result = await _mediator.Send(command);
        return Ok(result);
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
