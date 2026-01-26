using VlogForge.Domain.Common;

namespace VlogForge.Domain.Events;

/// <summary>
/// Event raised when a new user registers.
/// </summary>
public sealed class UserRegisteredEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string DisplayName { get; }

    public UserRegisteredEvent(Guid userId, string email, string displayName)
    {
        UserId = userId;
        Email = email;
        DisplayName = displayName;
    }
}

/// <summary>
/// Event raised when a user's email is verified.
/// </summary>
public sealed class UserEmailVerifiedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserEmailVerifiedEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}

/// <summary>
/// Event raised when a user successfully logs in.
/// </summary>
public sealed class UserLoggedInEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserLoggedInEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}

/// <summary>
/// Event raised when a user account is locked out due to failed login attempts.
/// </summary>
public sealed class UserLockedOutEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public DateTime LockoutEnd { get; }

    public UserLockedOutEvent(Guid userId, string email, DateTime lockoutEnd)
    {
        UserId = userId;
        Email = email;
        LockoutEnd = lockoutEnd;
    }
}

/// <summary>
/// Event raised when a password reset is requested.
/// </summary>
public sealed class PasswordResetRequestedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public PasswordResetRequestedEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}

/// <summary>
/// Event raised when a password reset is completed.
/// </summary>
public sealed class PasswordResetCompletedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public PasswordResetCompletedEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
