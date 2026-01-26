using MediatR;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Auth.Commands.ResetPassword;

/// <summary>
/// Handler for ResetPasswordCommand.
/// Story: ACF-001
/// </summary>
public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task<AuthResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (!Email.TryCreate(request.Email, out var email))
        {
            return AuthResult.Failed("Invalid email address.");
        }

        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);
        if (user == null)
        {
            // Use generic error to prevent user enumeration
            return AuthResult.Failed("Invalid or expired reset token.");
        }

        // Validate new password strength
        var passwordValidation = _identityService.ValidatePassword(request.NewPassword);
        if (!passwordValidation.IsValid)
        {
            return AuthResult.Failed(passwordValidation.Errors);
        }

        // Hash new password
        var newPasswordHash = _identityService.HashPassword(request.NewPassword);

        // Reset password
        if (!user.ResetPassword(request.Token, newPasswordHash))
        {
            return AuthResult.Failed("Invalid or expired reset token.");
        }

        // Revoke all refresh tokens for security
        user.RevokeAllRefreshTokens("Password reset");

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Send password changed notification
        await _emailService.SendPasswordChangedNotificationAsync(
            user.Email.Value,
            user.DisplayName,
            cancellationToken);

        return AuthResult.Succeeded("Password reset successfully. Please log in with your new password.");
    }
}
