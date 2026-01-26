using FluentAssertions;
using Moq;
using VlogForge.Application.Auth.Commands.RegisterUser;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for RegisterUserCommandHandler.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _identityServiceMock = new Mock<IIdentityService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _emailServiceMock = new Mock<IEmailService>();

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _identityServiceMock.Object,
            _tokenServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithValidDataShouldCreateUserAndReturnTokens()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "Password123!",
            "Test User",
            "127.0.0.1",
            "TestAgent");

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityServiceMock
            .Setup(x => x.ValidatePassword(It.IsAny<string>()))
            .Returns(PasswordValidationResult.Success());

        _identityServiceMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

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
        result.DisplayName.Should().Be("Test User");
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.EmailVerified.Should().BeFalse();

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithExistingEmailShouldThrowConflictException()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "existing@example.com",
            "Password123!",
            "Test User");

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task HandleWithWeakPasswordShouldThrowValidationException()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "weak",
            "Test User");

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityServiceMock
            .Setup(x => x.ValidatePassword(It.IsAny<string>()))
            .Returns(PasswordValidationResult.Failure(new[] { "Password is too weak" }));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleShouldSendVerificationEmail()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "Password123!",
            "Test User");

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityServiceMock
            .Setup(x => x.ValidatePassword(It.IsAny<string>()))
            .Returns(PasswordValidationResult.Success());

        _identityServiceMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new TokenResult("access-token", DateTime.UtcNow.AddHours(1)));

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("refresh-token", "hashed-refresh-token", DateTime.UtcNow.AddDays(7)));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - verify email service was called (fire and forget, so may not be awaited)
        await Task.Delay(100); // Give time for fire-and-forget task
        _emailServiceMock.Verify(
            x => x.SendEmailVerificationAsync(
                "test@example.com",
                "Test User",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
