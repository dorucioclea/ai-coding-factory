using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Entity representing an OAuth-connected platform for API access and data sync.
/// Stores encrypted OAuth tokens for YouTube, Instagram, and TikTok.
/// Story: ACF-003
/// </summary>
public sealed class PlatformConnection : Entity
{
    /// <summary>
    /// Gets the user ID this connection belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the platform type.
    /// </summary>
    public PlatformType PlatformType { get; private set; }

    /// <summary>
    /// Gets the encrypted OAuth access token.
    /// </summary>
    public string? EncryptedAccessToken { get; private set; }

    /// <summary>
    /// Gets the encrypted OAuth refresh token.
    /// </summary>
    public string? EncryptedRefreshToken { get; private set; }

    /// <summary>
    /// Gets the token expiration time.
    /// </summary>
    public DateTime? TokenExpiresAt { get; private set; }

    /// <summary>
    /// Gets the connection status.
    /// </summary>
    public ConnectionStatus Status { get; private set; }

    /// <summary>
    /// Gets the platform-specific account ID.
    /// </summary>
    public string PlatformAccountId { get; private set; }

    /// <summary>
    /// Gets the platform account display name.
    /// </summary>
    public string PlatformAccountName { get; private set; }

    /// <summary>
    /// Gets the last successful data sync timestamp.
    /// </summary>
    public DateTime? LastSyncAt { get; private set; }

    /// <summary>
    /// Gets the error message if status is Error.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets whether the access token is expired.
    /// </summary>
    public bool IsTokenExpired => TokenExpiresAt.HasValue && TokenExpiresAt.Value <= DateTime.UtcNow;

    private PlatformConnection() : base()
    {
        PlatformAccountId = string.Empty;
        PlatformAccountName = string.Empty;
    }

    private PlatformConnection(
        Guid userId,
        PlatformType platformType,
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime tokenExpiresAt,
        string platformAccountId,
        string platformAccountName) : base()
    {
        UserId = userId;
        PlatformType = platformType;
        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        TokenExpiresAt = tokenExpiresAt;
        PlatformAccountId = platformAccountId;
        PlatformAccountName = platformAccountName;
        Status = ConnectionStatus.Connected;
    }

    /// <summary>
    /// Creates a new platform connection with OAuth tokens.
    /// </summary>
    public static PlatformConnection Create(
        Guid userId,
        PlatformType platformType,
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime tokenExpiresAt,
        string platformAccountId,
        string platformAccountName)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            throw new ArgumentException("Access token cannot be empty.", nameof(encryptedAccessToken));

        if (string.IsNullOrWhiteSpace(platformAccountId))
            throw new ArgumentException("Platform account ID cannot be empty.", nameof(platformAccountId));

        if (string.IsNullOrWhiteSpace(platformAccountName))
            throw new ArgumentException("Platform account name cannot be empty.", nameof(platformAccountName));

        var connection = new PlatformConnection(
            userId,
            platformType,
            encryptedAccessToken,
            encryptedRefreshToken,
            tokenExpiresAt,
            platformAccountId.Trim(),
            platformAccountName.Trim());

        connection.RaiseDomainEvent(new PlatformConnectedEvent(
            connection.Id,
            connection.UserId,
            connection.PlatformType,
            connection.PlatformAccountId,
            connection.PlatformAccountName));

        return connection;
    }

    /// <summary>
    /// Updates the OAuth tokens after a refresh.
    /// </summary>
    public void UpdateTokens(string encryptedAccessToken, string? encryptedRefreshToken, DateTime tokenExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            throw new ArgumentException("Access token cannot be empty.", nameof(encryptedAccessToken));

        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        TokenExpiresAt = tokenExpiresAt;
        Status = ConnectionStatus.Connected;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marks the token as expired.
    /// </summary>
    public void MarkTokenExpired()
    {
        Status = ConnectionStatus.TokenExpired;
    }

    /// <summary>
    /// Marks the connection as having an error.
    /// </summary>
    public void MarkError(string errorMessage)
    {
        Status = ConnectionStatus.Error;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Clears the error state and resets to connected.
    /// </summary>
    public void ClearError()
    {
        Status = ConnectionStatus.Connected;
        ErrorMessage = null;
    }

    /// <summary>
    /// Disconnects the platform and clears tokens.
    /// </summary>
    public void Disconnect()
    {
        if (Status == ConnectionStatus.Disconnected)
            return;

        Status = ConnectionStatus.Disconnected;
        EncryptedAccessToken = null;
        EncryptedRefreshToken = null;
        TokenExpiresAt = null;
        ErrorMessage = null;

        RaiseDomainEvent(new PlatformDisconnectedEvent(Id, UserId, PlatformType));
    }

    /// <summary>
    /// Records a successful data sync.
    /// </summary>
    public void RecordSync()
    {
        LastSyncAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the token is expiring within the specified threshold.
    /// </summary>
    public bool IsTokenExpiringSoon(TimeSpan threshold)
    {
        if (!TokenExpiresAt.HasValue)
            return false;

        return TokenExpiresAt.Value <= DateTime.UtcNow.Add(threshold);
    }
}
