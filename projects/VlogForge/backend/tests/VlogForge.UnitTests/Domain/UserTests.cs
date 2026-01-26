using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for the User aggregate root.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class UserTests
{
    private static Email CreateValidEmail() => Email.Create("test@example.com");
    private const string ValidDisplayName = "Test User";
    private const string ValidPasswordHash = "hashedpassword123";

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldCreateUser()
    {
        // Arrange
        var email = CreateValidEmail();

        // Act
        var user = User.Create(email, ValidDisplayName, ValidPasswordHash);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(ValidDisplayName, user.DisplayName);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
        Assert.False(user.EmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationTokenExpiry);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public void CreateShouldRaiseUserRegisteredEvent()
    {
        // Arrange
        var email = CreateValidEmail();

        // Act
        var user = User.Create(email, ValidDisplayName, ValidPasswordHash);

        // Assert
        var domainEvent = user.DomainEvents.OfType<UserRegisteredEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(user.Id, domainEvent.UserId);
        Assert.Equal(email.Value, domainEvent.Email);
        Assert.Equal(ValidDisplayName, domainEvent.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithEmptyDisplayNameShouldThrow(string displayName)
    {
        // Arrange
        var email = CreateValidEmail();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create(email, displayName, ValidPasswordHash));
        Assert.Contains("Display name", exception.Message);
    }

    [Fact]
    public void CreateWithTooLongDisplayNameShouldThrow()
    {
        // Arrange
        var email = CreateValidEmail();
        var longName = new string('a', 101);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create(email, longName, ValidPasswordHash));
        Assert.Contains("100 characters", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithEmptyPasswordHashShouldThrow(string passwordHash)
    {
        // Arrange
        var email = CreateValidEmail();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create(email, ValidDisplayName, passwordHash));
        Assert.Contains("Password hash", exception.Message);
    }

    #endregion

    #region Email Verification Tests

    [Fact]
    public void VerifyEmailWithValidTokenShouldVerifyAndClearToken()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        var token = user.EmailVerificationToken!;

        // Act
        var result = user.VerifyEmail(token);

        // Assert
        Assert.True(result);
        Assert.True(user.EmailVerified);
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationTokenExpiry);
    }

    [Fact]
    public void VerifyEmailShouldRaiseUserEmailVerifiedEvent()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.ClearDomainEvents();
        var token = user.EmailVerificationToken!;

        // Act
        user.VerifyEmail(token);

        // Assert
        var domainEvent = user.DomainEvents.OfType<UserEmailVerifiedEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(user.Id, domainEvent.UserId);
    }

    [Fact]
    public void VerifyEmailWithInvalidTokenShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);

        // Act
        var result = user.VerifyEmail("invalid-token");

        // Assert
        Assert.False(result);
        Assert.False(user.EmailVerified);
    }

    [Fact]
    public void VerifyEmailWhenAlreadyVerifiedShouldReturnTrue()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.VerifyEmail(user.EmailVerificationToken!);

        // Act
        var result = user.VerifyEmail("any-token");

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyEmailWithEmptyTokenShouldReturnFalse(string token)
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);

        // Act
        var result = user.VerifyEmail(token);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Login Tests

    [Fact]
    public void RecordSuccessfulLoginShouldResetAttemptsAndSetLastLogin()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.RecordFailedLogin();
        user.RecordFailedLogin();
        user.ClearDomainEvents();

        // Act
        user.RecordSuccessfulLogin();

        // Assert
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public void RecordSuccessfulLoginShouldRaiseUserLoggedInEvent()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.ClearDomainEvents();

        // Act
        user.RecordSuccessfulLogin();

        // Assert
        var domainEvent = user.DomainEvents.OfType<UserLoggedInEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(user.Id, domainEvent.UserId);
    }

    [Fact]
    public void RecordFailedLoginShouldIncrementAttempts()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);

        // Act
        user.RecordFailedLogin();

        // Assert
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public void RecordFailedLoginAtMaxAttemptsShouldLockOut()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.ClearDomainEvents();

        // Act - 5 failed attempts (default max)
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        // Assert
        Assert.Equal(5, user.FailedLoginAttempts);
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.IsLockedOut());
    }

    [Fact]
    public void RecordFailedLoginAtMaxAttemptsShouldRaiseUserLockedOutEvent()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.ClearDomainEvents();

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        // Assert
        var domainEvent = user.DomainEvents.OfType<UserLockedOutEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(user.Id, domainEvent.UserId);
    }

    [Fact]
    public void IsLockedOutWhenNotLockedShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);

        // Act & Assert
        Assert.False(user.IsLockedOut());
    }

    #endregion

    #region Password Reset Tests

    [Fact]
    public void GeneratePasswordResetTokenShouldSetTokenAndExpiry()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.ClearDomainEvents();

        // Act
        var token = user.GeneratePasswordResetToken();

        // Assert
        Assert.NotNull(token);
        Assert.Equal(token, user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetTokenExpiry);
        Assert.True(user.PasswordResetTokenExpiry > DateTime.UtcNow);
    }

    [Fact]
    public void GeneratePasswordResetTokenShouldRaisePasswordResetRequestedEvent()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.ClearDomainEvents();

        // Act
        user.GeneratePasswordResetToken();

        // Assert
        var domainEvent = user.DomainEvents.OfType<PasswordResetRequestedEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(user.Id, domainEvent.UserId);
    }

    [Fact]
    public void ResetPasswordWithValidTokenShouldResetAndClearLockout()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        for (int i = 0; i < 5; i++) user.RecordFailedLogin();
        var token = user.GeneratePasswordResetToken();
        var newHash = "newhash123";
        user.ClearDomainEvents();

        // Act
        var result = user.ResetPassword(token, newHash);

        // Assert
        Assert.True(result);
        Assert.Equal(newHash, user.PasswordHash);
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetTokenExpiry);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public void ResetPasswordShouldRaisePasswordResetCompletedEvent()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        var token = user.GeneratePasswordResetToken();
        user.ClearDomainEvents();

        // Act
        user.ResetPassword(token, "newhash123");

        // Assert
        var domainEvent = user.DomainEvents.OfType<PasswordResetCompletedEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(user.Id, domainEvent.UserId);
    }

    [Fact]
    public void ResetPasswordWithInvalidTokenShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.GeneratePasswordResetToken();

        // Act
        var result = user.ResetPassword("invalid-token", "newhash123");

        // Assert
        Assert.False(result);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ResetPasswordWithEmptyNewHashShouldReturnFalse(string newHash)
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        var token = user.GeneratePasswordResetToken();

        // Act
        var result = user.ResetPassword(token, newHash);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public void AddRefreshTokenShouldAddToCollection()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        var tokenValue = "refresh-token-123";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = user.AddRefreshToken(tokenValue, expiresAt, "127.0.0.1", "TestAgent");

        // Assert
        Assert.Single(user.RefreshTokens);
        Assert.Equal(tokenValue, token.Token);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.Equal("127.0.0.1", token.CreatedByIp);
        Assert.Equal("TestAgent", token.UserAgent);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void RevokeRefreshTokenWithValidTokenShouldRevoke()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        var tokenValue = "refresh-token-123";
        user.AddRefreshToken(tokenValue, DateTime.UtcNow.AddDays(7));

        // Act
        var result = user.RevokeRefreshToken(tokenValue, "192.168.1.1");

        // Assert
        Assert.True(result);
        var token = user.RefreshTokens.First();
        Assert.True(token.IsRevoked);
        Assert.Equal("192.168.1.1", token.RevokedByIp);
    }

    [Fact]
    public void RevokeRefreshTokenWithInvalidTokenShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.AddRefreshToken("token-1", DateTime.UtcNow.AddDays(7));

        // Act
        var result = user.RevokeRefreshToken("non-existent-token");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RevokeAllRefreshTokensShouldRevokeAll()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.AddRefreshToken("token-1", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken("token-2", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken("token-3", DateTime.UtcNow.AddDays(7));

        // Act
        user.RevokeAllRefreshTokens("logout-ip");

        // Assert
        Assert.All(user.RefreshTokens, t => Assert.True(t.IsRevoked));
    }

    [Fact]
    public void GetActiveRefreshTokenShouldReturnActiveToken()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.AddRefreshToken("token-1", DateTime.UtcNow.AddDays(7));
        var activeToken = user.AddRefreshToken("token-2", DateTime.UtcNow.AddDays(7));
        user.RevokeRefreshToken("token-1");

        // Act
        var result = user.GetActiveRefreshToken("token-2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(activeToken.Id, result.Id);
    }

    [Fact]
    public void GetActiveRefreshTokenWithRevokedTokenShouldReturnNull()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        user.AddRefreshToken("token-1", DateTime.UtcNow.AddDays(7));
        user.RevokeRefreshToken("token-1");

        // Act
        var result = user.GetActiveRefreshToken("token-1");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void UpdateDisplayNameWithValidNameShouldUpdate()
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);
        var newName = "New Display Name";

        // Act
        user.UpdateDisplayName(newName);

        // Assert
        Assert.Equal(newName, user.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDisplayNameWithEmptyNameShouldThrow(string name)
    {
        // Arrange
        var user = User.Create(CreateValidEmail(), ValidDisplayName, ValidPasswordHash);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.UpdateDisplayName(name));
    }

    #endregion
}
