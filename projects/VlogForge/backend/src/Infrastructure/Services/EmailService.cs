using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// Email service implementation.
/// In development, this logs emails instead of sending them.
/// In production, replace with actual email provider (SendGrid, SES, etc.)
/// Story: ACF-001
/// </summary>
public sealed partial class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SendEmailVerificationAsync(string email, string displayName, string verificationToken, CancellationToken cancellationToken = default)
    {
        // In development, just log the email
        LogVerificationEmail(_logger, email, displayName, verificationToken);

        // TODO: In production, send actual email via SendGrid, SES, etc.
        // Example verification URL: https://yourdomain.com/verify-email?token={verificationToken}

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendPasswordResetAsync(string email, string displayName, string resetToken, CancellationToken cancellationToken = default)
    {
        // In development, just log the email
        LogPasswordResetEmail(_logger, email, displayName, resetToken);

        // TODO: In production, send actual email
        // Example reset URL: https://yourdomain.com/reset-password?token={resetToken}

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendPasswordChangedNotificationAsync(string email, string displayName, CancellationToken cancellationToken = default)
    {
        // In development, just log the email
        LogPasswordChangedEmail(_logger, email, displayName);

        // TODO: In production, send actual email

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// Story: ACF-007
    public Task SendTeamInvitationAsync(string email, string teamName, string invitationToken, CancellationToken cancellationToken = default)
    {
        // In development, just log the email
        LogTeamInvitationEmail(_logger, email, teamName, invitationToken);

        // TODO: In production, send actual email
        // Example invite URL: https://yourdomain.com/teams/invite?token={invitationToken}

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEV] Email verification for {Email} ({DisplayName}): Token = {Token}")]
    private static partial void LogVerificationEmail(ILogger logger, string email, string displayName, string token);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEV] Password reset for {Email} ({DisplayName}): Token = {Token}")]
    private static partial void LogPasswordResetEmail(ILogger logger, string email, string displayName, string token);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEV] Password changed notification for {Email} ({DisplayName})")]
    private static partial void LogPasswordChangedEmail(ILogger logger, string email, string displayName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEV] Team invitation for {Email} to join team '{TeamName}': Token = {Token}")]
    private static partial void LogTeamInvitationEmail(ILogger logger, string email, string teamName, string token);
}
