using MediatR;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Command to resend the email verification message.
/// Story: ACF-001
/// </summary>
public sealed record ResendVerificationCommand(
    Guid UserId
) : IRequest<AuthResult>;
