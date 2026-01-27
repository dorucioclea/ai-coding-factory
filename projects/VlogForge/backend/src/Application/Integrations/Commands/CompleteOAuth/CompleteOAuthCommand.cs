using MediatR;
using VlogForge.Application.Integrations.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations.Commands.CompleteOAuth;

/// <summary>
/// Command to complete OAuth flow and store tokens.
/// Story: ACF-003
/// </summary>
public sealed record CompleteOAuthCommand(
    Guid UserId,
    PlatformType PlatformType,
    string Code,
    string State,
    string RedirectUri
) : IRequest<OAuthCompletionResponse>;
