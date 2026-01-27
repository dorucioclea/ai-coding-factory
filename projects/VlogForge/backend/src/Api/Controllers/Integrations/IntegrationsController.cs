using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.Integrations.Requests;
using VlogForge.Application.Integrations.Commands.CompleteOAuth;
using VlogForge.Application.Integrations.Commands.DisconnectPlatform;
using VlogForge.Application.Integrations.Commands.InitiateOAuth;
using VlogForge.Application.Integrations.DTOs;
using VlogForge.Application.Integrations.Queries.GetConnectionStatus;
using VlogForge.Domain.Entities;

namespace VlogForge.Api.Controllers.Integrations;

/// <summary>
/// Platform integration endpoints for connecting YouTube, Instagram, and TikTok.
/// Story: ACF-003
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class IntegrationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IntegrationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the connection status for all platforms.
    /// </summary>
    /// <returns>List of platform connections and available platforms.</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ConnectionStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ConnectionStatusResponse>> GetStatus()
    {
        var userId = GetCurrentUserId();
        var query = new GetConnectionStatusQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Initiates OAuth flow for connecting a platform.
    /// </summary>
    /// <param name="platform">Platform to connect (youtube, instagram, tiktok).</param>
    /// <param name="request">OAuth initiation details.</param>
    /// <returns>OAuth authorization URL and state.</returns>
    [HttpPost("{platform}/connect")]
    [ProducesResponseType(typeof(OAuthInitiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OAuthInitiationResponse>> InitiateOAuth(
        [FromRoute] string platform,
        [FromBody] InitiateOAuthRequest request)
    {
        if (!TryParsePlatform(platform, out var platformType))
        {
            return BadRequest($"Invalid platform: {platform}. Must be youtube, instagram, or tiktok.");
        }

        var userId = GetCurrentUserId();
        var command = new InitiateOAuthCommand(userId, platformType, request.RedirectUri);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Completes OAuth flow after user authorization.
    /// </summary>
    /// <param name="platform">Platform being connected.</param>
    /// <param name="request">OAuth callback data.</param>
    /// <returns>Connection result.</returns>
    [HttpPost("{platform}/callback")]
    [ProducesResponseType(typeof(OAuthCompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OAuthCompletionResponse>> CompleteOAuth(
        [FromRoute] string platform,
        [FromBody] CompleteOAuthRequest request)
    {
        if (!TryParsePlatform(platform, out var platformType))
        {
            return BadRequest($"Invalid platform: {platform}. Must be youtube, instagram, or tiktok.");
        }

        var userId = GetCurrentUserId();
        var command = new CompleteOAuthCommand(
            userId,
            platformType,
            request.Code,
            request.State,
            request.RedirectUri);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Disconnects a platform and revokes OAuth tokens.
    /// </summary>
    /// <param name="platform">Platform to disconnect.</param>
    /// <returns>Disconnect result.</returns>
    [HttpDelete("{platform}")]
    [ProducesResponseType(typeof(DisconnectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisconnectResponse>> Disconnect([FromRoute] string platform)
    {
        if (!TryParsePlatform(platform, out var platformType))
        {
            return BadRequest($"Invalid platform: {platform}. Must be youtube, instagram, or tiktok.");
        }

        var userId = GetCurrentUserId();
        var command = new DisconnectPlatformCommand(userId, platformType);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }
        return userId;
    }

    private static bool TryParsePlatform(string platform, out PlatformType platformType)
    {
        platformType = platform.ToLowerInvariant() switch
        {
            "youtube" => PlatformType.YouTube,
            "instagram" => PlatformType.Instagram,
            "tiktok" => PlatformType.TikTok,
            _ => (PlatformType)(-1)
        };

        return (int)platformType >= 0;
    }
}
