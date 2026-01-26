using MediatR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;

namespace VlogForge.Application.Auth.Commands.RegisterUser;

/// <summary>
/// Handler for RegisterUserCommand.
/// Story: ACF-001
/// </summary>
public sealed partial class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        ITokenService tokenService,
        IEmailService emailService,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Parse and validate email
        var email = Email.Create(request.Email);

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Email", request.Email, "A user with this email already exists.");
        }

        // Validate password strength
        var passwordValidation = _identityService.ValidatePassword(request.Password);
        if (!passwordValidation.IsValid)
        {
            throw new ValidationException(passwordValidation.Errors
                .Select(e => new FluentValidation.Results.ValidationFailure("Password", e)));
        }

        // Hash password
        var passwordHash = _identityService.HashPassword(request.Password);

        // Create user (returns user and plain verification token)
        var (user, verificationToken) = User.Create(email, request.DisplayName, passwordHash);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Add refresh token to user
        user.AddRefreshToken(refreshToken.TokenHash, refreshToken.ExpiresAt, request.IpAddress, request.UserAgent);

        // Persist user
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Send verification email asynchronously (don't block registration)
        _ = SendVerificationEmailSafelyAsync(user.Email.Value, user.DisplayName, verificationToken);

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

    private async Task SendVerificationEmailSafelyAsync(string email, string displayName, string verificationToken)
    {
        try
        {
            await _emailService.SendEmailVerificationAsync(email, displayName, verificationToken, CancellationToken.None);
            LogVerificationEmailSent(_logger, email);
        }
        catch (Exception ex)
        {
            LogVerificationEmailFailed(_logger, ex, email);
            // Don't rethrow - registration should succeed even if email fails
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Verification email sent to {Email}")]
    private static partial void LogVerificationEmailSent(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send verification email to {Email}")]
    private static partial void LogVerificationEmailFailed(ILogger logger, Exception ex, string email);
}
