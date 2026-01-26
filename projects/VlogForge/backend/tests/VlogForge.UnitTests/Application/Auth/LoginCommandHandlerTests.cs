using FluentAssertions;
using Moq;
using VlogForge.Application.Auth.Commands.Login;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for LoginCommandHandler.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _identityServiceMock = new Mock<IIdentityService>();
        _tokenServiceMock = new Mock<ITokenService>();

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _identityServiceMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithValidCredentialsShouldReturnTokens()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");
        user.VerifyEmail(user.GenerateEmailVerificationToken()); // Verify email

        var command = new LoginCommand("test@example.com", "Password123!", "127.0.0.1", "TestAgent");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(x => x.VerifyPassword("Password123!", "hashed-password"))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new TokenResult("access-token", DateTime.UtcNow.AddHours(1)));

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("refresh-token", "hashed-refresh-token", DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWithInvalidEmailShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Password123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task HandleWithInvalidPasswordShouldThrowUnauthorizedExceptionAndRecordFailedLogin()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");

        var command = new LoginCommand("test@example.com", "WrongPassword!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(x => x.VerifyPassword("WrongPassword!", "hashed-password"))
            .Returns(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        user.FailedLoginAttempts.Should().Be(1);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithLockedOutUserShouldThrowUnauthorizedException()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");

        // Lock out the user by recording 5 failed logins
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        var command = new LoginCommand("test@example.com", "Password123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Account is locked. Please try again later or reset your password.");
    }

    [Fact]
    public async Task HandleWithSuccessfulLoginShouldResetFailedAttempts()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");

        // Simulate some failed attempts
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        var command = new LoginCommand("test@example.com", "Password123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(x => x.VerifyPassword("Password123!", "hashed-password"))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new TokenResult("access-token", DateTime.UtcNow.AddHours(1)));

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("refresh-token", "hashed-refresh-token", DateTime.UtcNow.AddDays(7)));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LastLoginAt.Should().NotBeNull();
    }
}
