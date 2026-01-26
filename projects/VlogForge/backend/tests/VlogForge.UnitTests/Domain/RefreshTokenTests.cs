using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for the RefreshToken entity.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
public class RefreshTokenTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidToken = "refresh-token-abc123";
    private static DateTime ValidExpiresAt => DateTime.UtcNow.AddDays(7);

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldCreateToken()
    {
        // Arrange & Act
        var token = RefreshToken.Create(ValidUserId, ValidToken, ValidExpiresAt, "127.0.0.1", "TestAgent");

        // Assert
        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(ValidUserId, token.UserId);
        Assert.Equal(ValidToken, token.Token);
        Assert.Equal("127.0.0.1", token.CreatedByIp);
        Assert.Equal("TestAgent", token.UserAgent);
        Assert.True(token.IsActive);
        Assert.False(token.IsExpired);
        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void CreateWithEmptyUserIdShouldThrow()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(Guid.Empty, ValidToken, ValidExpiresAt));
        Assert.Contains("User ID", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithEmptyTokenShouldThrow(string token)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(ValidUserId, token, ValidExpiresAt));
        Assert.Contains("Token", exception.Message);
    }

    [Fact]
    public void CreateWithPastExpiryShouldThrow()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(ValidUserId, ValidToken, pastDate));
        Assert.Contains("future", exception.Message);
    }

    #endregion

    #region Expiry Tests

    [Fact]
    public void IsExpiredWhenNotExpiredShouldReturnFalse()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidToken, DateTime.UtcNow.AddDays(7));

        // Assert
        Assert.False(token.IsExpired);
    }

    [Fact]
    public void IsActiveWhenNotExpiredAndNotRevokedShouldReturnTrue()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidToken, DateTime.UtcNow.AddDays(7));

        // Assert
        Assert.True(token.IsActive);
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void RevokeShouldSetRevokedAtAndIp()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidToken, ValidExpiresAt);

        // Act
        token.Revoke("192.168.1.1", "replacement-token");

        // Assert
        Assert.True(token.IsRevoked);
        Assert.NotNull(token.RevokedAt);
        Assert.Equal("192.168.1.1", token.RevokedByIp);
        Assert.Equal("replacement-token", token.ReplacedByToken);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void RevokeWithoutReplacementTokenShouldStillRevoke()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidToken, ValidExpiresAt);

        // Act
        token.Revoke("192.168.1.1");

        // Assert
        Assert.True(token.IsRevoked);
        Assert.Null(token.ReplacedByToken);
    }

    [Fact]
    public void RevokeWhenAlreadyRevokedShouldNotUpdate()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidToken, ValidExpiresAt);
        token.Revoke("first-ip");
        var firstRevokedAt = token.RevokedAt;

        // Act
        token.Revoke("second-ip");

        // Assert
        Assert.Equal(firstRevokedAt, token.RevokedAt);
        Assert.Equal("first-ip", token.RevokedByIp);
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActiveWhenRevokedShouldReturnFalse()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidToken, ValidExpiresAt);
        token.Revoke();

        // Assert
        Assert.False(token.IsActive);
    }

    #endregion
}
