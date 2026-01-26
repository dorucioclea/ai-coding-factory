using FluentAssertions;
using Moq;
using VlogForge.Application.Auth.Commands.Logout;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for LogoutCommandHandler.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class LogoutCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _identityServiceMock = new Mock<IIdentityService>();

        _handler = new LogoutCommandHandler(
            _userRepositoryMock.Object,
            _identityServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithValidRefreshTokenShouldRevokeIt()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");
        var refreshToken = user.AddRefreshToken("token-hash", DateTime.UtcNow.AddDays(7));

        var command = new LogoutCommand("plain-refresh-token", "127.0.0.1");

        _identityServiceMock
            .Setup(x => x.HashToken("plain-refresh-token"))
            .Returns("token-hash");

        _userRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync("token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Logged out successfully.");
        refreshToken.IsRevoked.Should().BeTrue();

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithInvalidRefreshTokenShouldStillReturnSuccess()
    {
        // Arrange
        var command = new LogoutCommand("invalid-token", "127.0.0.1");

        _identityServiceMock
            .Setup(x => x.HashToken("invalid-token"))
            .Returns("invalid-hash");

        _userRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync("invalid-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - should still return success (idempotent logout)
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Logged out successfully.");
    }

    [Fact]
    public async Task HandleWithEmptyRefreshTokenShouldReturnSuccess()
    {
        // Arrange
        var command = new LogoutCommand("", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        // Should not try to look up or revoke anything
        _userRepositoryMock.Verify(
            x => x.GetByRefreshTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleWithAlreadyRevokedTokenShouldReturnSuccess()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");
        var refreshToken = user.AddRefreshToken("token-hash", DateTime.UtcNow.AddDays(7));
        refreshToken.Revoke(); // Already revoked

        var command = new LogoutCommand("plain-refresh-token", "127.0.0.1");

        _identityServiceMock
            .Setup(x => x.HashToken("plain-refresh-token"))
            .Returns("token-hash");

        _userRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync("token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - should still return success
        result.Success.Should().BeTrue();
    }
}
