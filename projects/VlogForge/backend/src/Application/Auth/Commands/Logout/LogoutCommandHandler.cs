using MediatR;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Auth.Commands.Logout;

/// <summary>
/// Handler for LogoutCommand.
/// Story: ACF-001
/// </summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;

    public LogoutCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
    }

    public async Task<AuthResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return AuthResult.Succeeded("Logged out successfully.");
        }

        // Hash the token to find it
        var tokenHash = _identityService.HashToken(request.RefreshToken);

        var user = await _userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);
        if (user == null)
        {
            // Token doesn't exist or already revoked - still return success
            return AuthResult.Succeeded("Logged out successfully.");
        }

        // Revoke the token
        user.RevokeRefreshToken(tokenHash, request.IpAddress);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return AuthResult.Succeeded("Logged out successfully.");
    }
}
