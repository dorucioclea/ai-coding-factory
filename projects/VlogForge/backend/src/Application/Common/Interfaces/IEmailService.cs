namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Service for sending emails.
/// Story: ACF-001
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email verification message.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="verificationToken">The verification token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendEmailVerificationAsync(string email, string displayName, string verificationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="resetToken">The password reset token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendPasswordResetAsync(string email, string displayName, string resetToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification that the password was changed.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendPasswordChangedNotificationAsync(string email, string displayName, CancellationToken cancellationToken = default);
}
