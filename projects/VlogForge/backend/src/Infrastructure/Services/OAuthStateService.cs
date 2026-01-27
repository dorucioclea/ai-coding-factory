using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Integrations;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// OAuth state service with HMAC signing for integrity verification.
/// Story: ACF-003
/// </summary>
public class OAuthStateService : IOAuthStateService
{
    private readonly byte[] _signingKey;

    public OAuthStateService(IConfiguration configuration)
    {
        // Use the encryption key for HMAC signing (or a separate key if configured)
        var keyBase64 = configuration["OAuth:StateSigningKey"]
            ?? configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "OAuth state signing key not configured. Set OAuth:StateSigningKey or Encryption:Key.");

        _signingKey = Convert.FromBase64String(keyBase64);
    }

    public string GenerateState(Guid userId, PlatformType platformType)
    {
        var timestamp = DateTime.UtcNow.Ticks;
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

        // State data format: userId|platformType|timestamp|nonce
        var stateData = $"{userId}|{platformType}|{timestamp}|{nonce}";

        // Compute HMAC signature
        var signature = ComputeHmac(stateData);

        // Final state: base64(stateData|signature)
        var fullState = $"{stateData}|{signature}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(fullState));
    }

    public OAuthStateData ValidateState(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new InvalidOperationException("OAuth state is required.");
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(state));
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Invalid OAuth state format.");
        }

        var parts = decoded.Split('|');
        if (parts.Length != 5)
        {
            throw new InvalidOperationException("Invalid OAuth state structure.");
        }

        var stateData = string.Join("|", parts[..4]);
        var providedSignature = parts[4];

        // Verify HMAC signature
        var expectedSignature = ComputeHmac(stateData);
        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedSignature),
            Encoding.UTF8.GetBytes(expectedSignature)))
        {
            throw new InvalidOperationException("OAuth state signature is invalid.");
        }

        // Parse state data
        if (!Guid.TryParse(parts[0], out var userId))
        {
            throw new InvalidOperationException("Invalid user ID in OAuth state.");
        }

        if (!Enum.TryParse<PlatformType>(parts[1], out var platformType))
        {
            throw new InvalidOperationException("Invalid platform type in OAuth state.");
        }

        if (!long.TryParse(parts[2], out var timestampTicks))
        {
            throw new InvalidOperationException("Invalid timestamp in OAuth state.");
        }

        var createdAt = new DateTime(timestampTicks, DateTimeKind.Utc);
        var age = DateTime.UtcNow - createdAt;

        // Check expiration
        if (age > IntegrationConstants.StateExpiration)
        {
            throw new InvalidOperationException(
                $"OAuth state has expired. State was created {age.TotalMinutes:F1} minutes ago.");
        }

        // Check for future timestamps (clock skew protection)
        if (age < TimeSpan.FromMinutes(-1))
        {
            throw new InvalidOperationException("OAuth state has invalid timestamp.");
        }

        return new OAuthStateData(userId, platformType, createdAt);
    }

    private string ComputeHmac(string data)
    {
        using var hmac = new HMACSHA256(_signingKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
