using FluentAssertions;
using Moq;
using VlogForge.Application.Auth.Commands.RefreshToken;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for RefreshTokenCommandHandler.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _identityServiceMock = new Mock<IIdentityService>();
        _tokenServiceMock = new Mock<ITokenService>();

        _handler = new RefreshTokenCommandHandler(
            _userRepositoryMock.Object,
            _identityServiceMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task HandleWithValidRefreshTokenShouldReturnNewTokens()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");
        var existingRefreshToken = user.AddRefreshToken("existing-token-hash", DateTime.UtcNow.AddDays(7));

        var command = new RefreshTokenCommand("plain-refresh-token", "127.0.0.1", "TestAgent");

        _identityServiceMock
            .Setup(x => x.HashToken("plain-refresh-token"))
            .Returns("existing-token-hash");

        _userRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync("existing-token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new TokenResult("new-access-token", DateTime.UtcNow.AddHours(1)));

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("new-refresh-token", "new-token-hash", DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");

        // Old token should be revoked
        existingRefreshToken.IsRevoked.Should().BeTrue();
        existingRefreshToken.ReplacedByTokenHash.Should().Be("new-token-hash");
    }

    [Fact]
    public async Task HandleWithInvalidRefreshTokenShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new RefreshTokenCommand("invalid-token");

        _identityServiceMock
            .Setup(x => x.HashToken("invalid-token"))
            .Returns("invalid-token-hash");

        _userRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync("invalid-token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid refresh token.");
    }

    [Fact]
    public async Task HandleWithRevokedRefreshTokenShouldThrowUnauthorizedException()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");
        var existingRefreshToken = user.AddRefreshToken("existing-token-hash", DateTime.UtcNow.AddDays(7));
        existingRefreshToken.Revoke(); // Revoke the token

        var command = new RefreshTokenCommand("plain-refresh-token");

        _identityServiceMock
            .Setup(x => x.HashToken("plain-refresh-token"))
            .Returns("existing-token-hash");

        _userRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync("existing-token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Refresh token has been revoked or expired.");
    }

    [Fact]
    public async Task HandleShouldImplementTokenRotation()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");
        user.AddRefreshToken("existing-token-hash", DateTime.UtcNow.AddDays(7));

        var command = new RefreshTokenCommand("plain-refresh-token", "127.0.0.1");

        _identityServiceMock
            .Setup(x => x.HashToken("plain-refresh-token"))
            .Returns("existing-token-hash");

        _userRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync("existing-token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new TokenResult("new-access-token", DateTime.UtcNow.AddHours(1)));

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("new-refresh-token", "new-token-hash", DateTime.UtcNow.AddDays(7)));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - user should have 2 tokens now (1 revoked + 1 new)
        user.RefreshTokens.Should().HaveCount(2);
        user.RefreshTokens.Count(t => t.IsActive).Should().Be(1);
        user.RefreshTokens.Count(t => t.IsRevoked).Should().Be(1);

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
