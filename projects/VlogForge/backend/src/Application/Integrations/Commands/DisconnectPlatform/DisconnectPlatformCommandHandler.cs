using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Integrations.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations.Commands.DisconnectPlatform;

/// <summary>
/// Handler for DisconnectPlatformCommand.
/// Story: ACF-003
/// </summary>
public sealed partial class DisconnectPlatformCommandHandler
    : IRequestHandler<DisconnectPlatformCommand, DisconnectResponse>
{
    private readonly IEnumerable<IPlatformOAuthService> _oauthServices;
    private readonly IPlatformConnectionRepository _connectionRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<DisconnectPlatformCommandHandler> _logger;

    public DisconnectPlatformCommandHandler(
        IEnumerable<IPlatformOAuthService> oauthServices,
        IPlatformConnectionRepository connectionRepository,
        IEncryptionService encryptionService,
        ILogger<DisconnectPlatformCommandHandler> logger)
    {
        _oauthServices = oauthServices;
        _connectionRepository = connectionRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<DisconnectResponse> Handle(
        DisconnectPlatformCommand request,
        CancellationToken cancellationToken)
    {
        // Find existing connection
        var connection = await _connectionRepository.GetByUserAndPlatformAsync(
            request.UserId,
            request.PlatformType,
            cancellationToken);

        if (connection == null)
        {
            throw new NotFoundException("PlatformConnection", request.PlatformType.ToString());
        }

        if (connection.Status == ConnectionStatus.Disconnected)
        {
            LogAlreadyDisconnected(request.UserId, request.PlatformType);
            return new DisconnectResponse(
                request.PlatformType.ToString(),
                true,
                "Platform was already disconnected.");
        }

        // Try to revoke tokens (best effort - don't fail if revocation fails)
        if (!string.IsNullOrEmpty(connection.EncryptedAccessToken))
        {
            var oauthService = _oauthServices.FirstOrDefault(s => s.PlatformType == request.PlatformType);
            if (oauthService != null)
            {
                try
                {
                    var accessToken = _encryptionService.Decrypt(connection.EncryptedAccessToken);
                    await oauthService.RevokeTokenAsync(accessToken, cancellationToken);
                    LogTokenRevoked(request.UserId, request.PlatformType);
                }
                catch (Exception ex)
                {
                    // Log but don't fail - tokens will expire anyway
                    LogTokenRevocationFailed(request.UserId, request.PlatformType, ex);
                }
            }
        }

        // Mark as disconnected (clears tokens)
        connection.Disconnect();
        await _connectionRepository.UpdateAsync(connection, cancellationToken);
        await _connectionRepository.SaveChangesAsync(cancellationToken);

        LogDisconnected(request.UserId, request.PlatformType);
        return new DisconnectResponse(
            request.PlatformType.ToString(),
            true,
            $"Successfully disconnected from {request.PlatformType}.");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Platform {PlatformType} already disconnected for user {UserId}")]
    private partial void LogAlreadyDisconnected(Guid userId, PlatformType platformType);

    [LoggerMessage(Level = LogLevel.Information, Message = "Token revoked for platform {PlatformType} for user {UserId}")]
    private partial void LogTokenRevoked(Guid userId, PlatformType platformType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Token revocation failed for platform {PlatformType} for user {UserId}")]
    private partial void LogTokenRevocationFailed(Guid userId, PlatformType platformType, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Platform {PlatformType} disconnected for user {UserId}")]
    private partial void LogDisconnected(Guid userId, PlatformType platformType);
}
