using MediatR;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Handler for ForgotPasswordCommand.
/// Story: ACF-001
/// </summary>
public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<AuthResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Always return success to prevent user enumeration
        const string successMessage = "If an account with that email exists, a password reset link has been sent.";

        if (!Email.TryCreate(request.Email, out var email))
        {
            return AuthResult.Succeeded(successMessage);
        }

        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);
        if (user == null)
        {
            // Don't reveal that user doesn't exist
            return AuthResult.Succeeded(successMessage);
        }

        // Generate password reset token
        var resetToken = user.GeneratePasswordResetToken();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Send password reset email
        await _emailService.SendPasswordResetAsync(
            user.Email.Value,
            user.DisplayName,
            resetToken,
            cancellationToken);

        return AuthResult.Succeeded(successMessage);
    }
}
