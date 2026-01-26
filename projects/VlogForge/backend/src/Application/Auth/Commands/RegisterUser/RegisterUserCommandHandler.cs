using MediatR;
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
public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _tokenService = tokenService;
        _emailService = emailService;
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

        // Send verification email (fire and forget, don't block registration)
        _ = _emailService.SendEmailVerificationAsync(
            user.Email.Value,
            user.DisplayName,
            verificationToken,
            CancellationToken.None);

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
