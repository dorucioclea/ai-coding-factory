using MediatR;
using VlogForge.Application.Auth.DTOs;

namespace VlogForge.Application.Auth.Commands.VerifyEmail;

/// <summary>
/// Command to verify a user's email address.
/// Story: ACF-001
/// </summary>
public sealed record VerifyEmailCommand(
    Guid UserId,
    string Token
) : IRequest<AuthResult>;
