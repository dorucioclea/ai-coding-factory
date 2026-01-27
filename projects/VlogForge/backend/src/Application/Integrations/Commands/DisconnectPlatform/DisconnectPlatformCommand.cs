using MediatR;
using VlogForge.Application.Integrations.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations.Commands.DisconnectPlatform;

/// <summary>
/// Command to disconnect a platform and revoke OAuth tokens.
/// Story: ACF-003
/// </summary>
public sealed record DisconnectPlatformCommand(
    Guid UserId,
    PlatformType PlatformType
) : IRequest<DisconnectResponse>;
