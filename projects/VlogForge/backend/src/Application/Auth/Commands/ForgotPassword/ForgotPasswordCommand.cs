using MediatR;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Command to request a password reset.
/// Story: ACF-001
/// </summary>
public sealed record ForgotPasswordCommand(
    string Email
) : IRequest<AuthResult>;
