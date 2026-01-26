using MediatR;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand.
/// Story: ACF-001
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Parse email
        if (!Email.TryCreate(request.Email, out var email))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        // Get user
        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);
        if (user == null)
        {
            // Use same error message to prevent user enumeration
            throw new UnauthorizedException("Invalid email or password.");
        }

        // Check if locked out
        if (user.IsLockedOut())
        {
            throw new UnauthorizedException("Account is locked. Please try again later or reset your password.");
        }

        // Verify password
        if (!_identityService.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("Invalid email or password.");
        }

        // Record successful login
        user.RecordSuccessfulLogin();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Add refresh token to user
        user.AddRefreshToken(refreshToken.TokenHash, refreshToken.ExpiresAt, request.IpAddress, request.UserAgent);

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
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            EmailVerified = user.EmailVerified
        };
    }
}
