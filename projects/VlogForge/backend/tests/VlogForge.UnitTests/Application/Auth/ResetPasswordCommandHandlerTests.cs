using FluentAssertions;
using Moq;
using VlogForge.Application.Auth.Commands.ResetPassword;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for ResetPasswordCommandHandler.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _identityServiceMock = new Mock<IIdentityService>();
        _emailServiceMock = new Mock<IEmailService>();

        _handler = new ResetPasswordCommandHandler(
            _userRepositoryMock.Object,
            _identityServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithValidTokenShouldResetPassword()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "old-hashed-password");
        var resetToken = user.GeneratePasswordResetToken();

        var command = new ResetPasswordCommand("test@example.com", resetToken, "NewPassword123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(x => x.ValidatePassword("NewPassword123!"))
            .Returns(PasswordValidationResult.Success());

        _identityServiceMock
            .Setup(x => x.HashPassword("NewPassword123!"))
            .Returns("new-hashed-password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Password reset successfully");
        user.PasswordHash.Should().Be("new-hashed-password");

        _emailServiceMock.Verify(
            x => x.SendPasswordChangedNotificationAsync(
                "test@example.com",
                "Test User",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleWithInvalidTokenShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");
        user.GeneratePasswordResetToken(); // Generate token but use different one

        var command = new ResetPasswordCommand("test@example.com", "wrong-token", "NewPassword123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(x => x.ValidatePassword("NewPassword123!"))
            .Returns(PasswordValidationResult.Success());

        _identityServiceMock
            .Setup(x => x.HashPassword("NewPassword123!"))
            .Returns("new-hashed-password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid or expired reset token.");
    }

    [Fact]
    public async Task HandleWithWeakPasswordShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");
        var resetToken = user.GeneratePasswordResetToken();

        var command = new ResetPasswordCommand("test@example.com", resetToken, "weak");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(x => x.ValidatePassword("weak"))
            .Returns(PasswordValidationResult.Failure(new[] { "Password is too weak" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Password is too weak");
    }

    [Fact]
    public async Task HandleShouldRevokeAllRefreshTokensAfterPasswordReset()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "old-hashed-password");
        user.AddRefreshToken("token-hash-1", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken("token-hash-2", DateTime.UtcNow.AddDays(7));
        var resetToken = user.GeneratePasswordResetToken();

        var command = new ResetPasswordCommand("test@example.com", resetToken, "NewPassword123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(x => x.ValidatePassword("NewPassword123!"))
            .Returns(PasswordValidationResult.Success());

        _identityServiceMock
            .Setup(x => x.HashPassword("NewPassword123!"))
            .Returns("new-hashed-password");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - all refresh tokens should be revoked
        user.RefreshTokens.Should().HaveCount(2);
        user.RefreshTokens.All(t => t.IsRevoked).Should().BeTrue();
    }

    [Fact]
    public async Task HandleWithNonExistentUserShouldReturnGenericError()
    {
        // Arrange
        var command = new ResetPasswordCommand("nonexistent@example.com", "some-token", "NewPassword123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - should return generic error to prevent user enumeration
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid or expired reset token.");
    }
}
