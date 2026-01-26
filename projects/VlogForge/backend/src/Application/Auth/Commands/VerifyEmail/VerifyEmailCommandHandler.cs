using MediatR;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Application.Auth.Commands.VerifyEmail;

/// <summary>
/// Handler for VerifyEmailCommand.
/// Story: ACF-001
/// </summary>
public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;

    public VerifyEmailCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            // Return generic error to prevent user enumeration
            return AuthResult.Failed("Invalid or expired verification token.");
        }

        if (user.EmailVerified)
        {
            return AuthResult.Succeeded("Email is already verified.");
        }

        if (!user.VerifyEmail(request.Token))
        {
            return AuthResult.Failed("Invalid or expired verification token.");
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return AuthResult.Succeeded("Email verified successfully.");
    }
}
