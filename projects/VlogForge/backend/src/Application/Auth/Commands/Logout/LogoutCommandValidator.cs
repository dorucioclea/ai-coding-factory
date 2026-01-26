using FluentValidation;

namespace VlogForge.Application.Auth.Commands.Logout;

/// <summary>
/// Validator for LogoutCommand.
/// Story: ACF-001
/// </summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        // RefreshToken is optional for logout - if not provided, just return success
        // No validation rules needed, but validator exists for consistency
    }
}
