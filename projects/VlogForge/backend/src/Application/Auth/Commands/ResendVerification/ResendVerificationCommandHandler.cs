using MediatR;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Handler for ResendVerificationCommand.
/// Story: ACF-001
/// </summary>
public sealed class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public ResendVerificationCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<AuthResult> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        if (user.EmailVerified)
        {
            return AuthResult.Succeeded("Email is already verified.");
        }

        // Generate new verification token
        var verificationToken = user.GenerateEmailVerificationToken();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Send verification email
        await _emailService.SendEmailVerificationAsync(
            user.Email.Value,
            user.DisplayName,
            verificationToken,
            cancellationToken);

        return AuthResult.Succeeded("Verification email sent.");
    }
}
