using FluentAssertions;
using Microsoft.Extensions.Configuration;
using VlogForge.Infrastructure.Services;
using Xunit;

namespace VlogForge.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for EncryptionService.
/// Story: ACF-003
/// </summary>
[Trait("Story", "ACF-003")]
public class EncryptionServiceTests
{
    private readonly EncryptionService _sut;

    public EncryptionServiceTests()
    {
        // Generate a valid test key
        var testKey = EncryptionService.GenerateKey();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Encryption:Key"] = testKey
            })
            .Build();

        _sut = new EncryptionService(configuration);
    }

    [Fact]
    public void EncryptAndDecryptShouldRoundTrip()
    {
        // Arrange
        var plainText = "This is a secret OAuth token!";

        // Act
        var encrypted = _sut.Encrypt(plainText);
        var decrypted = _sut.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void EncryptShouldProduceDifferentOutputsForSameInput()
    {
        // Arrange
        var plainText = "Same input";

        // Act
        var encrypted1 = _sut.Encrypt(plainText);
        var encrypted2 = _sut.Encrypt(plainText);

        // Assert (different nonces should produce different ciphertexts)
        encrypted1.Should().NotBe(encrypted2);
    }

    [Fact]
    public void DecryptShouldWorkForBothEncryptions()
    {
        // Arrange
        var plainText = "Same input";
        var encrypted1 = _sut.Encrypt(plainText);
        var encrypted2 = _sut.Encrypt(plainText);

        // Act
        var decrypted1 = _sut.Decrypt(encrypted1);
        var decrypted2 = _sut.Decrypt(encrypted2);

        // Assert
        decrypted1.Should().Be(plainText);
        decrypted2.Should().Be(plainText);
    }

    [Fact]
    public void EncryptWithEmptyStringShouldThrow()
    {
        // Act
        var act = () => _sut.Encrypt(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DecryptWithEmptyStringShouldThrow()
    {
        // Act
        var act = () => _sut.Decrypt(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DecryptWithInvalidDataShouldThrow()
    {
        // Arrange
        var invalidCiphertext = Convert.ToBase64String(new byte[] { 1, 2, 3 });

        // Act
        var act = () => _sut.Decrypt(invalidCiphertext);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void DecryptWithTamperedDataShouldThrow()
    {
        // Arrange
        var plainText = "Secret data";
        var encrypted = _sut.Encrypt(plainText);
        var encryptedBytes = Convert.FromBase64String(encrypted);

        // Tamper with the ciphertext
        encryptedBytes[20]++;
        var tampered = Convert.ToBase64String(encryptedBytes);

        // Act
        var act = () => _sut.Decrypt(tampered);

        // Assert (AES-GCM should detect tampering)
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void EncryptShouldHandleLongStrings()
    {
        // Arrange
        var longText = new string('A', 10000);

        // Act
        var encrypted = _sut.Encrypt(longText);
        var decrypted = _sut.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(longText);
    }

    [Fact]
    public void EncryptShouldHandleUnicodeStrings()
    {
        // Arrange
        var unicodeText = "Hello 世界! 🔐 Привет!";

        // Act
        var encrypted = _sut.Encrypt(unicodeText);
        var decrypted = _sut.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(unicodeText);
    }

    [Fact]
    public void GenerateKeyShouldProduceValidBase64()
    {
        // Act
        var key = EncryptionService.GenerateKey();

        // Assert
        key.Should().NotBeNullOrEmpty();
        var keyBytes = Convert.FromBase64String(key);
        keyBytes.Length.Should().Be(32); // 256 bits
    }

    [Fact]
    public void ConstructorWithMissingKeyShouldThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        var act = () => new EncryptionService(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Encryption key not configured*");
    }

    [Fact]
    public void ConstructorWithInvalidKeySizeShouldThrow()
    {
        // Arrange
        var shortKey = Convert.ToBase64String(new byte[16]); // 128 bits, not 256

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Encryption:Key"] = shortKey
            })
            .Build();

        // Act
        var act = () => new EncryptionService(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*32 bytes*");
    }
}
