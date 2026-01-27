using FluentAssertions;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for Bio value object.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class BioTests
{
    [Fact]
    public void CreateWithValidBioShouldSucceed()
    {
        // Arrange
        var bioText = "I'm a content creator focused on tech reviews.";

        // Act
        var bio = Bio.Create(bioText);

        // Assert
        bio.Value.Should().Be(bioText);
    }

    [Fact]
    public void CreateWithEmptyStringShouldReturnEmptyBio()
    {
        // Act
        var bio = Bio.Create("");

        // Assert
        bio.Value.Should().BeEmpty();
    }

    [Fact]
    public void CreateWithNullShouldReturnEmptyBio()
    {
        // Act
        var bio = Bio.Create(null);

        // Assert
        bio.Value.Should().BeEmpty();
    }

    [Fact]
    public void CreateWithWhitespaceShouldTrim()
    {
        // Act
        var bio = Bio.Create("  Hello World  ");

        // Assert
        bio.Value.Should().Be("Hello World");
    }

    [Fact]
    public void CreateWithMaxLengthShouldSucceed()
    {
        // Arrange
        var bioText = new string('a', Bio.MaxLength);

        // Act
        var bio = Bio.Create(bioText);

        // Assert
        bio.Value.Should().HaveLength(Bio.MaxLength);
    }

    [Fact]
    public void CreateExceedingMaxLengthShouldThrow()
    {
        // Arrange
        var bioText = new string('a', Bio.MaxLength + 1);

        // Act
        var act = () => Bio.Create(bioText);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Bio cannot exceed {Bio.MaxLength} characters.*");
    }

    [Fact]
    public void EmptyPropertyShouldReturnEmptyBio()
    {
        // Act
        var bio = Bio.Empty;

        // Assert
        bio.Value.Should().BeEmpty();
    }

    [Fact]
    public void EqualBiosShouldBeEqual()
    {
        // Arrange
        var bio1 = Bio.Create("Test bio");
        var bio2 = Bio.Create("Test bio");

        // Assert
        bio1.Should().Be(bio2);
        (bio1 == bio2).Should().BeTrue();
    }

    [Fact]
    public void DifferentBiosShouldNotBeEqual()
    {
        // Arrange
        var bio1 = Bio.Create("Test bio 1");
        var bio2 = Bio.Create("Test bio 2");

        // Assert
        bio1.Should().NotBe(bio2);
        (bio1 != bio2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitStringConversionShouldWork()
    {
        // Arrange
        var bio = Bio.Create("My bio");

        // Act
        string bioString = bio;

        // Assert
        bioString.Should().Be("My bio");
    }
}
