using MediatR;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Application.Auth.Commands.Logout;

/// <summary>
/// Command to log out a user by revoking their refresh token.
/// Story: ACF-001
/// </summary>
public sealed record LogoutCommand(
    string RefreshToken,
    string? IpAddress = null
) : IRequest<AuthResult>;
