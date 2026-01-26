using MediatR;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh authentication tokens.
/// Story: ACF-001
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<AuthResponse>;
