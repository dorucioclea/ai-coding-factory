using MediatR;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Integrations.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations.Commands.CompleteOAuth;

/// <summary>
/// Handler for CompleteOAuthCommand.
/// Story: ACF-003
/// </summary>
public sealed class CompleteOAuthCommandHandler
    : IRequestHandler<CompleteOAuthCommand, OAuthCompletionResponse>
{
    private readonly IEnumerable<IPlatformOAuthService> _oauthServices;
    private readonly IPlatformConnectionRepository _connectionRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IOAuthStateService _stateService;

    public CompleteOAuthCommandHandler(
        IEnumerable<IPlatformOAuthService> oauthServices,
        IPlatformConnectionRepository connectionRepository,
        IEncryptionService encryptionService,
        IOAuthStateService stateService)
    {
        _oauthServices = oauthServices;
        _connectionRepository = connectionRepository;
        _encryptionService = encryptionService;
        _stateService = stateService;
    }

    public async Task<OAuthCompletionResponse> Handle(
        CompleteOAuthCommand request,
        CancellationToken cancellationToken)
    {
        // Validate HMAC-signed state parameter (CSRF protection + expiration)
        OAuthStateData stateData;
        try
        {
            stateData = _stateService.ValidateState(request.State);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationException("State", ex.Message);
        }

        // Verify state matches request
        if (stateData.UserId != request.UserId || stateData.PlatformType != request.PlatformType)
        {
            throw new ValidationException("State", "OAuth state does not match request parameters.");
        }

        // Find the OAuth service for this platform
        var oauthService = _oauthServices.FirstOrDefault(s => s.PlatformType == request.PlatformType);
        if (oauthService == null)
        {
            throw new ValidationException(
                "PlatformType",
                $"Platform {request.PlatformType} is not supported.");
        }

        // Exchange code for tokens
        var tokenResponse = await oauthService.ExchangeCodeAsync(
            request.Code,
            request.RedirectUri,
            cancellationToken);

        // Get account info
        var accountInfo = await oauthService.GetAccountInfoAsync(
            tokenResponse.AccessToken,
            cancellationToken);

        // Encrypt tokens
        var encryptedAccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken);
        var encryptedRefreshToken = tokenResponse.RefreshToken != null
            ? _encryptionService.Encrypt(tokenResponse.RefreshToken)
            : null;

        // Check for existing connection
        var existing = await _connectionRepository.GetByUserAndPlatformAsync(
            request.UserId,
            request.PlatformType,
            cancellationToken);

        PlatformConnection connection;
        if (existing != null)
        {
            // Update existing connection
            existing.UpdateTokens(encryptedAccessToken, encryptedRefreshToken, tokenResponse.ExpiresAt);
            await _connectionRepository.UpdateAsync(existing, cancellationToken);
            connection = existing;
        }
        else
        {
            // Create new connection
            connection = PlatformConnection.Create(
                request.UserId,
                request.PlatformType,
                encryptedAccessToken,
                encryptedRefreshToken,
                tokenResponse.ExpiresAt,
                accountInfo.AccountId,
                accountInfo.AccountName);

            await _connectionRepository.AddAsync(connection, cancellationToken);
        }

        // Save - repository handles race conditions by throwing ConflictException for duplicates
        await _connectionRepository.SaveChangesAsync(cancellationToken);

        return new OAuthCompletionResponse(
            connection.Id,
            connection.PlatformType.ToString(),
            connection.PlatformAccountId,
            connection.PlatformAccountName,
            connection.Status.ToString());
    }
}
