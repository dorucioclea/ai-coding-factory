using MediatR;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Application.Auth.Commands.RegisterUser;

/// <summary>
/// Command to register a new user.
/// Story: ACF-001
/// </summary>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<AuthResponse>;
