using FluentAssertions;
using Moq;
using VlogForge.Application.Auth.Commands.ForgotPassword;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for ForgotPasswordCommandHandler.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailServiceMock = new Mock<IEmailService>();

        _handler = new ForgotPasswordCommandHandler(
            _userRepositoryMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithExistingUserShouldSendResetEmail()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");

        var command = new ForgotPasswordCommand("test@example.com");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("If an account with that email exists");

        _emailServiceMock.Verify(
            x => x.SendPasswordResetAsync(
                "test@example.com",
                "Test User",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNonExistentUserShouldReturnSuccessToPreventEnumeration()
    {
        // Arrange
        var command = new ForgotPasswordCommand("nonexistent@example.com");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - should still return success to prevent user enumeration
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("If an account with that email exists");

        // Should not send any email
        _emailServiceMock.Verify(
            x => x.SendPasswordResetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleWithInvalidEmailFormatShouldReturnSuccess()
    {
        // Arrange
        var command = new ForgotPasswordCommand("invalid-email");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - should still return success to prevent information leakage
        result.Success.Should().BeTrue();

        // Should not even try to look up user
        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
