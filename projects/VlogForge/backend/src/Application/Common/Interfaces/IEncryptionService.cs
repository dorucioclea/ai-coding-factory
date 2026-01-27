namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Service interface for encrypting and decrypting sensitive data.
/// Story: ACF-003
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts plaintext using AES-256-GCM.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <returns>Base64-encoded encrypted data with IV and auth tag.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts ciphertext using AES-256-GCM.
    /// </summary>
    /// <param name="cipherText">Base64-encoded encrypted data.</param>
    /// <returns>The decrypted plaintext.</returns>
    string Decrypt(string cipherText);
}
