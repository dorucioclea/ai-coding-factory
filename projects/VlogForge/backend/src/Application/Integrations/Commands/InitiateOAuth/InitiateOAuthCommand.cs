using MediatR;
using VlogForge.Application.Integrations.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations.Commands.InitiateOAuth;

/// <summary>
/// Command to initiate OAuth flow for a platform.
/// Story: ACF-003
/// </summary>
public sealed record InitiateOAuthCommand(
    Guid UserId,
    PlatformType PlatformType,
    string RedirectUri
) : IRequest<OAuthInitiationResponse>;
