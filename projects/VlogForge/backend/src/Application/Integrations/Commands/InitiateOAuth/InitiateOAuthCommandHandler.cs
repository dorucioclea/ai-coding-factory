using MediatR;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Integrations.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations.Commands.InitiateOAuth;

/// <summary>
/// Handler for InitiateOAuthCommand.
/// Story: ACF-003
/// </summary>
public sealed class InitiateOAuthCommandHandler
    : IRequestHandler<InitiateOAuthCommand, OAuthInitiationResponse>
{
    private readonly IEnumerable<IPlatformOAuthService> _oauthServices;
    private readonly IPlatformConnectionRepository _connectionRepository;
    private readonly IOAuthStateService _stateService;

    public InitiateOAuthCommandHandler(
        IEnumerable<IPlatformOAuthService> oauthServices,
        IPlatformConnectionRepository connectionRepository,
        IOAuthStateService stateService)
    {
        _oauthServices = oauthServices;
        _connectionRepository = connectionRepository;
        _stateService = stateService;
    }

    public async Task<OAuthInitiationResponse> Handle(
        InitiateOAuthCommand request,
        CancellationToken cancellationToken)
    {
        // Check if already connected
        var existing = await _connectionRepository.GetByUserAndPlatformAsync(
            request.UserId,
            request.PlatformType,
            cancellationToken);

        if (existing != null && existing.Status == ConnectionStatus.Connected)
        {
            throw new ConflictException(
                "PlatformConnection",
                request.PlatformType.ToString(),
                $"Platform {request.PlatformType} is already connected. Disconnect first to reconnect.");
        }

        // Find the OAuth service for this platform
        var oauthService = _oauthServices.FirstOrDefault(s => s.PlatformType == request.PlatformType);
        if (oauthService == null)
        {
            throw new ValidationException(
                "PlatformType",
                $"Platform {request.PlatformType} is not supported for OAuth integration.");
        }

        // Generate HMAC-signed state for CSRF protection
        var state = _stateService.GenerateState(request.UserId, request.PlatformType);

        // Get authorization URL
        var authorizationUrl = oauthService.GetAuthorizationUrl(state, request.RedirectUri);

        return new OAuthInitiationResponse(authorizationUrl, state);
    }
}
