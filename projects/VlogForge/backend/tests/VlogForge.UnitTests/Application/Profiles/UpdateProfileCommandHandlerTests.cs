using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.Commands.UpdateProfile;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Profiles;

/// <summary>
/// Unit tests for UpdateProfileCommandHandler.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class UpdateProfileCommandHandlerTests
{
    private static readonly string[] TestNicheTags = ["gaming", "tech"];

    private readonly Mock<ICreatorProfileRepository> _profileRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<UpdateProfileCommandHandler>> _loggerMock;
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests()
    {
        _profileRepositoryMock = new Mock<ICreatorProfileRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<UpdateProfileCommandHandler>>();

        _handler = new UpdateProfileCommandHandler(
            _profileRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidDataShouldUpdateProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        var command = new UpdateProfileCommand(
            userId,
            "Jane Doe",
            "Updated bio",
            TestNicheTags,
            true,
            "Looking for gaming collabs");

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be("Jane Doe");
        result.Bio.Should().Be("Updated bio");
        result.NicheTags.Should().HaveCount(2);
        result.OpenToCollaborations.Should().BeTrue();
        result.CollaborationPreferences.Should().Be("Looking for gaming collabs");

        _profileRepositoryMock.Verify(x => x.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _profileRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync($"profile:username:{profile.Username}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNonExistentProfileShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateProfileCommand(userId, "Jane Doe", null, null, null, null);

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleWithNullNicheTagsShouldNotUpdateTags()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        profile.SetNicheTags(new[] { VlogForge.Domain.ValueObjects.NicheTag.Create("existing") });

        var command = new UpdateProfileCommand(userId, "John Doe", null, null, null, null);

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NicheTags.Should().ContainSingle().Which.Should().Be("existing");
    }

    [Fact]
    public async Task HandleWithEmptyNicheTagsShouldClearTags()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        profile.SetNicheTags(new[] { VlogForge.Domain.ValueObjects.NicheTag.Create("existing") });

        var command = new UpdateProfileCommand(userId, "John Doe", null, Array.Empty<string>(), null, null);

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NicheTags.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleShouldInvalidateCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        var command = new UpdateProfileCommand(userId, "Jane Doe", null, null, null, null);

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(
            x => x.RemoveAsync("profile:username:johndoe", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
