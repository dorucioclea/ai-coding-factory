using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// AES-256-GCM encryption service for securing OAuth tokens.
/// Story: ACF-003
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const int KeySize = 32; // 256 bits
    private const int NonceSize = 12; // 96 bits for GCM
    private const int TagSize = 16; // 128 bits

    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption key not configured. Set Encryption:Key in configuration.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != KeySize)
        {
            throw new InvalidOperationException($"Encryption key must be {KeySize} bytes (256 bits).");
        }
    }

    /// <summary>
    /// Encrypts plaintext using AES-256-GCM.
    /// Output format: base64(nonce || ciphertext || tag)
    /// </summary>
    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);

        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Encrypt(nonce, plainBytes, ciphertext, tag);

        // Combine: nonce || ciphertext || tag
        var result = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts ciphertext using AES-256-GCM.
    /// </summary>
    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);

        var data = Convert.FromBase64String(cipherText);

        if (data.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Invalid ciphertext format.");
        }

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var cipherBytes = new byte[data.Length - NonceSize - TagSize];
        var plainBytes = new byte[cipherBytes.Length];

        Buffer.BlockCopy(data, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(data, NonceSize, cipherBytes, 0, cipherBytes.Length);
        Buffer.BlockCopy(data, NonceSize + cipherBytes.Length, tag, 0, TagSize);

        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// Generates a new encryption key suitable for AES-256.
    /// Use this to generate a key for the Encryption:Key configuration.
    /// </summary>
    public static string GenerateKey()
    {
        var key = RandomNumberGenerator.GetBytes(KeySize);
        return Convert.ToBase64String(key);
    }
}
