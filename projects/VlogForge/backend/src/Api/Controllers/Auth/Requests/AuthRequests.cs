namespace VlogForge.Api.Controllers.Auth.Requests;

/// <summary>
/// Request for user registration.
/// Story: ACF-001
/// </summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName
);

/// <summary>
/// Request for user login.
/// Story: ACF-001
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password
);

/// <summary>
/// Request for token refresh.
/// Story: ACF-001
/// </summary>
public sealed record RefreshTokenRequest(
    string RefreshToken
);

/// <summary>
/// Request for email verification.
/// Story: ACF-001
/// </summary>
public sealed record VerifyEmailRequest(
    Guid UserId,
    string Token
);

/// <summary>
/// Request for forgot password.
/// Story: ACF-001
/// </summary>
public sealed record ForgotPasswordRequest(
    string Email
);

/// <summary>
/// Request for password reset.
/// Story: ACF-001
/// </summary>
public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword
);

/// <summary>
/// Request for resending verification email.
/// Story: ACF-001
/// </summary>
public sealed record ResendVerificationRequest(
    Guid UserId
);

/// <summary>
/// Request for logout.
/// Story: ACF-001
/// </summary>
public sealed record LogoutRequest(
    string? RefreshToken
);
