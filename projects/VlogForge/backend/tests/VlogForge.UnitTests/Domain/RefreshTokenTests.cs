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
    private const string ValidTokenHash = "hashed-refresh-token-abc123";
    private static DateTime ValidExpiresAt => DateTime.UtcNow.AddDays(7);

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldCreateToken()
    {
        // Arrange & Act
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, ValidExpiresAt, "127.0.0.1", "TestAgent");

        // Assert
        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(ValidUserId, token.UserId);
        Assert.Equal(ValidTokenHash, token.TokenHash);
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
            RefreshToken.Create(Guid.Empty, ValidTokenHash, ValidExpiresAt));
        Assert.Contains("User ID", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithEmptyTokenHashShouldThrow(string tokenHash)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(ValidUserId, tokenHash, ValidExpiresAt));
        Assert.Contains("Token hash", exception.Message);
    }

    [Fact]
    public void CreateWithPastExpiryShouldThrow()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(ValidUserId, ValidTokenHash, pastDate));
        Assert.Contains("future", exception.Message);
    }

    [Fact]
    public void CreateWithInvalidIpShouldSetNull()
    {
        // Act
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, ValidExpiresAt, "not-an-ip", "TestAgent");

        // Assert
        Assert.Null(token.CreatedByIp);
    }

    [Fact]
    public void CreateWithValidIpShouldNormalize()
    {
        // Act
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, ValidExpiresAt, "  192.168.1.1  ", "TestAgent");

        // Assert
        Assert.Equal("192.168.1.1", token.CreatedByIp);
    }

    [Fact]
    public void CreateWithLongUserAgentShouldTruncate()
    {
        // Arrange
        var longUserAgent = new string('a', 600);

        // Act
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, ValidExpiresAt, null, longUserAgent);

        // Assert
        Assert.NotNull(token.UserAgent);
        Assert.Equal(500, token.UserAgent.Length);
    }

    #endregion

    #region Expiry Tests

    [Fact]
    public void IsExpiredWhenNotExpiredShouldReturnFalse()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, DateTime.UtcNow.AddDays(7));

        // Assert
        Assert.False(token.IsExpired);
    }

    [Fact]
    public void IsActiveWhenNotExpiredAndNotRevokedShouldReturnTrue()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, DateTime.UtcNow.AddDays(7));

        // Assert
        Assert.True(token.IsActive);
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void RevokeShouldSetRevokedAtAndRevokedBy()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, ValidExpiresAt);

        // Act
        token.Revoke("192.168.1.1", "replacement-token-hash");

        // Assert
        Assert.True(token.IsRevoked);
        Assert.NotNull(token.RevokedAt);
        Assert.Equal("192.168.1.1", token.RevokedBy);
        Assert.Equal("replacement-token-hash", token.ReplacedByTokenHash);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void RevokeWithoutReplacementTokenShouldStillRevoke()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, ValidExpiresAt);

        // Act
        token.Revoke("192.168.1.1");

        // Assert
        Assert.True(token.IsRevoked);
        Assert.Null(token.ReplacedByTokenHash);
    }

    [Fact]
    public void RevokeWhenAlreadyRevokedShouldNotUpdate()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, ValidExpiresAt);
        token.Revoke("first-revoker");
        var firstRevokedAt = token.RevokedAt;

        // Act
        token.Revoke("second-revoker");

        // Assert
        Assert.Equal(firstRevokedAt, token.RevokedAt);
        Assert.Equal("first-revoker", token.RevokedBy);
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActiveWhenRevokedShouldReturnFalse()
    {
        // Arrange
        var token = RefreshToken.Create(ValidUserId, ValidTokenHash, ValidExpiresAt);
        token.Revoke();

        // Assert
        Assert.False(token.IsActive);
    }

    #endregion
}
