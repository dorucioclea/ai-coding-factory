using FluentAssertions;
using Moq;
using VlogForge.Application.Auth.Commands.ResendVerification;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for ResendVerificationCommandHandler.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class ResendVerificationCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ResendVerificationCommandHandler _handler;

    public ResendVerificationCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailServiceMock = new Mock<IEmailService>();

        _handler = new ResendVerificationCommandHandler(
            _userRepositoryMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithUnverifiedUserShouldSendNewVerificationEmail()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");

        var command = new ResendVerificationCommand(user.Id);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Verification email sent.");

        _emailServiceMock.Verify(
            x => x.SendEmailVerificationAsync(
                "test@example.com",
                "Test User",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithAlreadyVerifiedUserShouldReturnSuccessMessage()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, verificationToken) = User.Create(email, "Test User", "hashed-password");
        user.VerifyEmail(verificationToken); // Already verified

        var command = new ResendVerificationCommand(user.Id);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Email is already verified.");

        // Should not send email or update user
        _emailServiceMock.Verify(
            x => x.SendEmailVerificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleWithNonExistentUserShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ResendVerificationCommand(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleShouldGenerateNewVerificationToken()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, originalToken) = User.Create(email, "Test User", "hashed-password");
        var originalTokenHash = user.EmailVerificationTokenHash;

        var command = new ResendVerificationCommand(user.Id);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - new token should be generated
        user.EmailVerificationTokenHash.Should().NotBe(originalTokenHash);
    }
}
