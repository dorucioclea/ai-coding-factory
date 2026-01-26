using FluentAssertions;
using Moq;
using VlogForge.Application.Auth.Commands.VerifyEmail;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for VerifyEmailCommandHandler.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly VerifyEmailCommandHandler _handler;

    public VerifyEmailCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new VerifyEmailCommandHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleWithValidTokenShouldVerifyEmail()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, verificationToken) = User.Create(email, "Test User", "hashed-password");

        var command = new VerifyEmailCommand(user.Id, verificationToken);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Email verified successfully.");
        user.EmailVerified.Should().BeTrue();

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithInvalidTokenShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");

        var command = new VerifyEmailCommand(user.Id, "invalid-token");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid or expired verification token.");
        user.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task HandleWithNonExistentUserShouldReturnFailureToPreventEnumeration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new VerifyEmailCommand(userId, "some-token");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - should return generic error to prevent user enumeration
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid or expired verification token.");
    }

    [Fact]
    public async Task HandleWithAlreadyVerifiedEmailShouldReturnSuccess()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, verificationToken) = User.Create(email, "Test User", "hashed-password");
        user.VerifyEmail(verificationToken); // Already verify

        var command = new VerifyEmailCommand(user.Id, "any-token");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Email is already verified.");

        // Should not try to update
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
