using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for CreatorProfile entity.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class CreatorProfileTests
{
    private readonly Guid _validUserId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldSucceed()
    {
        // Act
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");

        // Assert
        profile.Should().NotBeNull();
        profile.UserId.Should().Be(_validUserId);
        profile.Username.Should().Be("johndoe");
        profile.DisplayName.Should().Be("John Doe");
        profile.Bio.Should().Be(Bio.Empty);
        profile.OpenToCollaborations.Should().BeFalse();
    }

    [Fact]
    public void CreateShouldNormalizeUsername()
    {
        // Act
        var profile = CreatorProfile.Create(_validUserId, "  JohnDoe  ", "John Doe");

        // Assert
        profile.Username.Should().Be("johndoe");
    }

    [Fact]
    public void CreateShouldRaiseProfileCreatedEvent()
    {
        // Act
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");

        // Assert
        profile.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CreatorProfileCreatedEvent>()
            .Which.Username.Should().Be("johndoe");
    }

    [Fact]
    public void CreateWithEmptyUserIdShouldThrow()
    {
        // Act
        var act = () => CreatorProfile.Create(Guid.Empty, "johndoe", "John Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateWithEmptyUsernameShouldThrow(string? username)
    {
        // Act
        var act = () => CreatorProfile.Create(_validUserId, username!, "John Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be empty.*");
    }

    [Fact]
    public void CreateWithTooShortUsernameShouldThrow()
    {
        // Act
        var act = () => CreatorProfile.Create(_validUserId, "ab", "John Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Username must be at least {CreatorProfile.MinUsernameLength} characters.*");
    }

    [Fact]
    public void CreateWithTooLongUsernameShouldThrow()
    {
        // Arrange
        var longUsername = new string('a', CreatorProfile.MaxUsernameLength + 1);

        // Act
        var act = () => CreatorProfile.Create(_validUserId, longUsername, "John Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Username cannot exceed {CreatorProfile.MaxUsernameLength} characters.*");
    }

    [Theory]
    [InlineData("john doe")]
    [InlineData("john-doe")]
    [InlineData("john.doe")]
    [InlineData("john@doe")]
    public void CreateWithInvalidUsernameCharactersShouldThrow(string username)
    {
        // Act
        var act = () => CreatorProfile.Create(_validUserId, username, "John Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username can only contain letters, numbers, and underscores.*");
    }

    [Fact]
    public void CreateWithValidUsernameCharactersShouldSucceed()
    {
        // Act
        var profile = CreatorProfile.Create(_validUserId, "john_doe_123", "John Doe");

        // Assert
        profile.Username.Should().Be("john_doe_123");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void UpdateBasicInfoShouldUpdateDisplayNameAndBio()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        profile.ClearDomainEvents();
        var newBio = Bio.Create("I create tech content");

        // Act
        profile.UpdateBasicInfo("Jane Doe", newBio);

        // Assert
        profile.DisplayName.Should().Be("Jane Doe");
        profile.Bio.Should().Be(newBio);
    }

    [Fact]
    public void UpdateBasicInfoShouldRaiseProfileUpdatedEvent()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        profile.ClearDomainEvents();

        // Act
        profile.UpdateBasicInfo("Jane Doe", Bio.Empty);

        // Assert
        profile.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CreatorProfileUpdatedEvent>();
    }

    [Fact]
    public void UpdateProfilePictureWithValidUrlShouldSucceed()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var url = "https://example.com/image.jpg";

        // Act
        profile.UpdateProfilePicture(url);

        // Assert
        profile.ProfilePictureUrl.Should().Be(url);
    }

    [Fact]
    public void UpdateProfilePictureWithNullShouldClearPicture()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        profile.UpdateProfilePicture("https://example.com/image.jpg");

        // Act
        profile.UpdateProfilePicture(null);

        // Assert
        profile.ProfilePictureUrl.Should().BeNull();
    }

    [Fact]
    public void UpdateProfilePictureWithInvalidUrlShouldThrow()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");

        // Act
        var act = () => profile.UpdateProfilePicture("not-a-valid-url");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Profile picture URL must be a valid absolute URL.*");
    }

    #endregion

    #region Collaboration Settings Tests

    [Fact]
    public void SetCollaborationSettingsShouldUpdateValues()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");

        // Act
        profile.SetCollaborationSettings(true, "Looking for gaming collabs");

        // Assert
        profile.OpenToCollaborations.Should().BeTrue();
        profile.CollaborationPreferences.Should().Be("Looking for gaming collabs");
    }

    [Fact]
    public void SetCollaborationSettingsChangingAvailabilityShouldRaiseEvent()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        profile.ClearDomainEvents();

        // Act
        profile.SetCollaborationSettings(true, null);

        // Assert
        profile.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CollaborationAvailabilityChangedEvent>()
            .Which.IsOpenToCollaborations.Should().BeTrue();
    }

    [Fact]
    public void SetCollaborationSettingsWithTooLongPreferencesShouldThrow()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var longPreferences = new string('a', 501);

        // Act
        var act = () => profile.SetCollaborationSettings(true, longPreferences);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Collaboration preferences cannot exceed 500 characters.*");
    }

    #endregion

    #region Niche Tags Tests

    [Fact]
    public void AddNicheTagShouldAddTag()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var tag = NicheTag.Create("gaming");

        // Act
        var result = profile.AddNicheTag(tag);

        // Assert
        result.Should().BeTrue();
        profile.NicheTags.Should().ContainSingle().Which.Should().Be(tag);
    }

    [Fact]
    public void AddNicheTagBeyondMaxShouldReturnFalse()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        for (var i = 0; i < CreatorProfile.MaxNicheTags; i++)
        {
            profile.AddNicheTag(NicheTag.Create($"tag{i}"));
        }

        // Act
        var result = profile.AddNicheTag(NicheTag.Create("extra-tag"));

        // Assert
        result.Should().BeFalse();
        profile.NicheTags.Should().HaveCount(CreatorProfile.MaxNicheTags);
    }

    [Fact]
    public void AddDuplicateNicheTagShouldReturnFalse()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var tag = NicheTag.Create("gaming");
        profile.AddNicheTag(tag);

        // Act
        var result = profile.AddNicheTag(NicheTag.Create("gaming"));

        // Assert
        result.Should().BeFalse();
        profile.NicheTags.Should().ContainSingle();
    }

    [Fact]
    public void RemoveNicheTagShouldRemoveTag()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var tag = NicheTag.Create("gaming");
        profile.AddNicheTag(tag);

        // Act
        var result = profile.RemoveNicheTag(tag);

        // Assert
        result.Should().BeTrue();
        profile.NicheTags.Should().BeEmpty();
    }

    [Fact]
    public void SetNicheTagsShouldReplaceAllTags()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        profile.AddNicheTag(NicheTag.Create("old-tag"));
        var newTags = new[] { NicheTag.Create("gaming"), NicheTag.Create("tech") };

        // Act
        profile.SetNicheTags(newTags);

        // Assert
        profile.NicheTags.Should().HaveCount(2);
        profile.NicheTags.Should().Contain(NicheTag.Create("gaming"));
        profile.NicheTags.Should().Contain(NicheTag.Create("tech"));
    }

    [Fact]
    public void SetNicheTagsShouldLimitToMax()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var tags = Enumerable.Range(0, 10).Select(i => NicheTag.Create($"tag{i}")).ToList();

        // Act
        profile.SetNicheTags(tags);

        // Assert
        profile.NicheTags.Should().HaveCount(CreatorProfile.MaxNicheTags);
    }

    #endregion

    #region Connected Platforms Tests

    [Fact]
    public void AddConnectedPlatformShouldAddPlatform()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var platform = ConnectedPlatform.Create(profile.Id, PlatformType.YouTube, "johndoe", "https://youtube.com/@johndoe");

        // Act
        profile.AddConnectedPlatform(platform);

        // Assert
        profile.ConnectedPlatforms.Should().ContainSingle().Which.Should().Be(platform);
    }

    [Fact]
    public void AddConnectedPlatformOfSameTypeShouldReplace()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var platform1 = ConnectedPlatform.Create(profile.Id, PlatformType.YouTube, "oldhandle", "https://youtube.com/@oldhandle");
        var platform2 = ConnectedPlatform.Create(profile.Id, PlatformType.YouTube, "newhandle", "https://youtube.com/@newhandle");
        profile.AddConnectedPlatform(platform1);

        // Act
        profile.AddConnectedPlatform(platform2);

        // Assert
        profile.ConnectedPlatforms.Should().ContainSingle()
            .Which.Handle.Should().Be("newhandle");
    }

    [Fact]
    public void RemoveConnectedPlatformShouldRemovePlatform()
    {
        // Arrange
        var profile = CreatorProfile.Create(_validUserId, "johndoe", "John Doe");
        var platform = ConnectedPlatform.Create(profile.Id, PlatformType.YouTube, "johndoe", "https://youtube.com/@johndoe");
        profile.AddConnectedPlatform(platform);

        // Act
        var result = profile.RemoveConnectedPlatform(PlatformType.YouTube);

        // Assert
        result.Should().BeTrue();
        profile.ConnectedPlatforms.Should().BeEmpty();
    }

    #endregion
}
