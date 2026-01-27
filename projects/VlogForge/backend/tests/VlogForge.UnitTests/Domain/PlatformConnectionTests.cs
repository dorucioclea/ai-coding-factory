using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for PlatformConnection entity.
/// Story: ACF-003
/// </summary>
[Trait("Story", "ACF-003")]
public class PlatformConnectionTests
{
    private readonly Guid _validUserId = Guid.NewGuid();
    private const string ValidAccessToken = "encrypted_access_token_data";
    private const string ValidRefreshToken = "encrypted_refresh_token_data";
    private const string ValidAccountId = "UC1234567890";
    private const string ValidAccountName = "My Channel";

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldSucceed()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var connection = PlatformConnection.Create(
            _validUserId,
            PlatformType.YouTube,
            ValidAccessToken,
            ValidRefreshToken,
            expiresAt,
            ValidAccountId,
            ValidAccountName);

        // Assert
        connection.Should().NotBeNull();
        connection.UserId.Should().Be(_validUserId);
        connection.PlatformType.Should().Be(PlatformType.YouTube);
        connection.EncryptedAccessToken.Should().Be(ValidAccessToken);
        connection.EncryptedRefreshToken.Should().Be(ValidRefreshToken);
        connection.TokenExpiresAt.Should().Be(expiresAt);
        connection.PlatformAccountId.Should().Be(ValidAccountId);
        connection.PlatformAccountName.Should().Be(ValidAccountName);
        connection.Status.Should().Be(ConnectionStatus.Connected);
        connection.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void CreateShouldRaisePlatformConnectedEvent()
    {
        // Act
        var connection = PlatformConnection.Create(
            _validUserId,
            PlatformType.YouTube,
            ValidAccessToken,
            ValidRefreshToken,
            DateTime.UtcNow.AddHours(1),
            ValidAccountId,
            ValidAccountName);

        // Assert
        connection.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PlatformConnectedEvent>()
            .Which.PlatformType.Should().Be(PlatformType.YouTube);
    }

    [Fact]
    public void CreateWithEmptyUserIdShouldThrow()
    {
        // Act
        var act = () => PlatformConnection.Create(
            Guid.Empty,
            PlatformType.YouTube,
            ValidAccessToken,
            ValidRefreshToken,
            DateTime.UtcNow.AddHours(1),
            ValidAccountId,
            ValidAccountName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateWithEmptyAccessTokenShouldThrow(string? accessToken)
    {
        // Act
        var act = () => PlatformConnection.Create(
            _validUserId,
            PlatformType.YouTube,
            accessToken!,
            ValidRefreshToken,
            DateTime.UtcNow.AddHours(1),
            ValidAccountId,
            ValidAccountName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Access token cannot be empty.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateWithEmptyAccountIdShouldThrow(string? accountId)
    {
        // Act
        var act = () => PlatformConnection.Create(
            _validUserId,
            PlatformType.YouTube,
            ValidAccessToken,
            ValidRefreshToken,
            DateTime.UtcNow.AddHours(1),
            accountId!,
            ValidAccountName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Platform account ID cannot be empty.*");
    }

    #endregion

    #region Token Update Tests

    [Fact]
    public void UpdateTokensShouldUpdateTokensAndExpiry()
    {
        // Arrange
        var connection = CreateValidConnection();
        var newAccessToken = "new_encrypted_access";
        var newRefreshToken = "new_encrypted_refresh";
        var newExpiry = DateTime.UtcNow.AddHours(2);

        // Act
        connection.UpdateTokens(newAccessToken, newRefreshToken, newExpiry);

        // Assert
        connection.EncryptedAccessToken.Should().Be(newAccessToken);
        connection.EncryptedRefreshToken.Should().Be(newRefreshToken);
        connection.TokenExpiresAt.Should().Be(newExpiry);
        connection.Status.Should().Be(ConnectionStatus.Connected);
    }

    [Fact]
    public void UpdateTokensWhenStatusWasTokenExpiredShouldResetToConnected()
    {
        // Arrange
        var connection = CreateValidConnection();
        connection.MarkTokenExpired();

        // Act
        connection.UpdateTokens("new_access", "new_refresh", DateTime.UtcNow.AddHours(1));

        // Assert
        connection.Status.Should().Be(ConnectionStatus.Connected);
    }

    #endregion

    #region Status Tests

    [Fact]
    public void MarkTokenExpiredShouldSetStatus()
    {
        // Arrange
        var connection = CreateValidConnection();

        // Act
        connection.MarkTokenExpired();

        // Assert
        connection.Status.Should().Be(ConnectionStatus.TokenExpired);
    }

    [Fact]
    public void MarkErrorShouldSetStatusAndMessage()
    {
        // Arrange
        var connection = CreateValidConnection();
        var errorMessage = "API rate limit exceeded";

        // Act
        connection.MarkError(errorMessage);

        // Assert
        connection.Status.Should().Be(ConnectionStatus.Error);
        connection.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void ClearErrorShouldResetStatusAndMessage()
    {
        // Arrange
        var connection = CreateValidConnection();
        connection.MarkError("Some error");

        // Act
        connection.ClearError();

        // Assert
        connection.Status.Should().Be(ConnectionStatus.Connected);
        connection.ErrorMessage.Should().BeNull();
    }

    #endregion

    #region Disconnect Tests

    [Fact]
    public void DisconnectShouldSetStatusAndClearTokens()
    {
        // Arrange
        var connection = CreateValidConnection();
        connection.ClearDomainEvents();

        // Act
        connection.Disconnect();

        // Assert
        connection.Status.Should().Be(ConnectionStatus.Disconnected);
        connection.EncryptedAccessToken.Should().BeNull();
        connection.EncryptedRefreshToken.Should().BeNull();
        connection.TokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void DisconnectShouldRaisePlatformDisconnectedEvent()
    {
        // Arrange
        var connection = CreateValidConnection();
        connection.ClearDomainEvents();

        // Act
        connection.Disconnect();

        // Assert
        connection.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PlatformDisconnectedEvent>();
    }

    [Fact]
    public void DisconnectWhenAlreadyDisconnectedShouldNotRaiseEvent()
    {
        // Arrange
        var connection = CreateValidConnection();
        connection.Disconnect();
        connection.ClearDomainEvents();

        // Act
        connection.Disconnect();

        // Assert
        connection.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region Sync Tests

    [Fact]
    public void RecordSyncShouldUpdateLastSyncAt()
    {
        // Arrange
        var connection = CreateValidConnection();
        var beforeSync = DateTime.UtcNow;

        // Act
        connection.RecordSync();

        // Assert
        connection.LastSyncAt.Should().NotBeNull();
        connection.LastSyncAt.Should().BeOnOrAfter(beforeSync);
    }

    #endregion

    #region Token Expiry Tests

    [Fact]
    public void IsTokenExpiredWhenExpiredShouldReturnTrue()
    {
        // Arrange
        var connection = PlatformConnection.Create(
            _validUserId,
            PlatformType.YouTube,
            ValidAccessToken,
            ValidRefreshToken,
            DateTime.UtcNow.AddMinutes(-5), // Expired 5 minutes ago
            ValidAccountId,
            ValidAccountName);

        // Act & Assert
        connection.IsTokenExpired.Should().BeTrue();
    }

    [Fact]
    public void IsTokenExpiredWhenNotExpiredShouldReturnFalse()
    {
        // Arrange
        var connection = CreateValidConnection();

        // Act & Assert
        connection.IsTokenExpired.Should().BeFalse();
    }

    [Fact]
    public void IsTokenExpiringSoonWhenExpiringWithinThresholdShouldReturnTrue()
    {
        // Arrange
        var connection = PlatformConnection.Create(
            _validUserId,
            PlatformType.YouTube,
            ValidAccessToken,
            ValidRefreshToken,
            DateTime.UtcNow.AddMinutes(3), // Expires in 3 minutes
            ValidAccountId,
            ValidAccountName);

        // Act & Assert
        connection.IsTokenExpiringSoon(TimeSpan.FromMinutes(5)).Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private PlatformConnection CreateValidConnection()
    {
        return PlatformConnection.Create(
            _validUserId,
            PlatformType.YouTube,
            ValidAccessToken,
            ValidRefreshToken,
            DateTime.UtcNow.AddHours(1),
            ValidAccountId,
            ValidAccountName);
    }

    #endregion
}
