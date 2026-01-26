using MediatR;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand.
/// Story: ACF-001
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Hash the incoming token to find it in the database
        var tokenHash = _identityService.HashToken(request.RefreshToken);

        // Find user by refresh token hash
        var user = await _userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        // Get the active refresh token
        var existingToken = user.GetActiveRefreshToken(tokenHash);
        if (existingToken == null)
        {
            // Token was revoked or expired
            throw new UnauthorizedException("Refresh token has been revoked or expired.");
        }

        // Generate new tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Revoke old token and add new one (token rotation)
        existingToken.Revoke(request.IpAddress, newRefreshToken.TokenHash);
        user.AddRefreshToken(newRefreshToken.TokenHash, newRefreshToken.ExpiresAt, request.IpAddress, request.UserAgent);

        // Persist changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email.Value,
            DisplayName = user.DisplayName,
            AccessToken = accessToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshToken = newRefreshToken.Token,
            RefreshTokenExpiresAt = newRefreshToken.ExpiresAt,
            EmailVerified = user.EmailVerified
        };
    }
}
