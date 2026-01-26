using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.Auth.Requests;
using VlogForge.Application.Auth.Commands.ForgotPassword;
using VlogForge.Application.Auth.Commands.Login;
using VlogForge.Application.Auth.Commands.Logout;
using VlogForge.Application.Auth.Commands.RefreshToken;
using VlogForge.Application.Auth.Commands.RegisterUser;
using VlogForge.Application.Auth.Commands.ResendVerification;
using VlogForge.Application.Auth.Commands.ResetPassword;
using VlogForge.Application.Auth.Commands.VerifyEmail;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Api.Controllers.Auth;

/// <summary>
/// Authentication endpoints.
/// Story: ACF-001
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <returns>Authentication tokens.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.DisplayName,
            GetIpAddress(),
            GetUserAgent()
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>Authentication tokens.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            GetIpAddress(),
            GetUserAgent()
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes authentication tokens.
    /// </summary>
    /// <param name="request">Refresh token.</param>
    /// <returns>New authentication tokens.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(
            request.RefreshToken,
            GetIpAddress(),
            GetUserAgent()
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Verifies a user's email address.
    /// </summary>
    /// <param name="request">Verification details.</param>
    /// <returns>Operation result.</returns>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResult>> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var command = new VerifyEmailCommand(request.UserId, request.Token);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Resends the email verification.
    /// </summary>
    /// <param name="request">User ID.</param>
    /// <returns>Operation result.</returns>
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResult>> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        var command = new ResendVerificationCommand(request.UserId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Requests a password reset.
    /// </summary>
    /// <param name="request">Email address.</param>
    /// <returns>Operation result.</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResult>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">Reset details.</param>
    /// <returns>Operation result.</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResult>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(
            request.Email,
            request.Token,
            request.NewPassword
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <param name="request">Optional refresh token to revoke.</param>
    /// <returns>Operation result.</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResult>> Logout([FromBody] LogoutRequest? request)
    {
        var command = new LogoutCommand(
            request?.RefreshToken ?? string.Empty,
            GetIpAddress()
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    private string? GetIpAddress()
    {
        // Check for forwarded IP (when behind proxy/load balancer)
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ips.Length > 0)
            {
                return ips[0];
            }
        }

        // Check for real IP header
        if (Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            return realIp.ToString();
        }

        // Fall back to remote IP
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return Request.Headers.UserAgent.ToString();
    }
}
