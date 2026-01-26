using MediatR;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Application.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user.
/// Story: ACF-001
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<AuthResponse>;
