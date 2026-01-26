using MediatR;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Application.Auth.Commands.ResetPassword;

/// <summary>
/// Command to reset a user's password.
/// Story: ACF-001
/// </summary>
public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<AuthResult>;
