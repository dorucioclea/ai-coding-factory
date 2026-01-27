using FluentAssertions;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for NicheTag value object.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class NicheTagTests
{
    [Theory]
    [InlineData("gaming", "gaming")]
    [InlineData("GAMING", "gaming")]
    [InlineData("tech-reviews", "tech-reviews")]
    [InlineData("vlog", "vlog")]
    [InlineData("beauty", "beauty")]
    public void CreateWithValidTagShouldSucceedAndNormalize(string input, string expected)
    {
        // Act
        var tag = NicheTag.Create(input);

        // Assert
        tag.Value.Should().Be(expected);
    }

    [Fact]
    public void CreateWithMinLengthShouldSucceed()
    {
        // Arrange
        var tagText = new string('a', NicheTag.MinLength);

        // Act
        var tag = NicheTag.Create(tagText);

        // Assert
        tag.Value.Should().HaveLength(NicheTag.MinLength);
    }

    [Fact]
    public void CreateWithMaxLengthShouldSucceed()
    {
        // Arrange
        var tagText = new string('a', NicheTag.MaxLength);

        // Act
        var tag = NicheTag.Create(tagText);

        // Assert
        tag.Value.Should().HaveLength(NicheTag.MaxLength);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateWithEmptyOrNullShouldThrow(string? value)
    {
        // Act
        var act = () => NicheTag.Create(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Niche tag cannot be empty.*");
    }

    [Fact]
    public void CreateWithTooShortTagShouldThrow()
    {
        // Arrange
        var tagText = new string('a', NicheTag.MinLength - 1);

        // Act
        var act = () => NicheTag.Create(tagText);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Niche tag must be at least {NicheTag.MinLength} characters.*");
    }

    [Fact]
    public void CreateExceedingMaxLengthShouldThrow()
    {
        // Arrange
        var tagText = new string('a', NicheTag.MaxLength + 1);

        // Act
        var act = () => NicheTag.Create(tagText);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Niche tag cannot exceed {NicheTag.MaxLength} characters.*");
    }

    [Theory]
    [InlineData("hello world")]
    [InlineData("hello_world")]
    [InlineData("hello.world")]
    [InlineData("hello@world")]
    [InlineData("-invalid")]
    [InlineData("invalid-")]
    [InlineData("invalid--double")]
    public void CreateWithInvalidCharactersShouldThrow(string value)
    {
        // Act
        var act = () => NicheTag.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Niche tag can only contain letters, numbers, and hyphens.*");
    }

    [Fact]
    public void TryCreateWithValidTagShouldReturnTrue()
    {
        // Act
        var result = NicheTag.TryCreate("gaming", out var tag);

        // Assert
        result.Should().BeTrue();
        tag.Should().NotBeNull();
        tag!.Value.Should().Be("gaming");
    }

    [Fact]
    public void TryCreateWithInvalidTagShouldReturnFalse()
    {
        // Act
        var result = NicheTag.TryCreate("", out var tag);

        // Assert
        result.Should().BeFalse();
        tag.Should().BeNull();
    }

    [Fact]
    public void EqualTagsShouldBeEqual()
    {
        // Arrange
        var tag1 = NicheTag.Create("gaming");
        var tag2 = NicheTag.Create("GAMING"); // Should normalize to same

        // Assert
        tag1.Should().Be(tag2);
        (tag1 == tag2).Should().BeTrue();
    }

    [Fact]
    public void DifferentTagsShouldNotBeEqual()
    {
        // Arrange
        var tag1 = NicheTag.Create("gaming");
        var tag2 = NicheTag.Create("tech");

        // Assert
        tag1.Should().NotBe(tag2);
        (tag1 != tag2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitStringConversionShouldWork()
    {
        // Arrange
        var tag = NicheTag.Create("gaming");

        // Act
        string tagString = tag;

        // Assert
        tagString.Should().Be("gaming");
    }
}
