using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.Commands.SetCollaborationSettings;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Profiles;

/// <summary>
/// Unit tests for SetCollaborationSettingsCommandHandler.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class SetCollaborationSettingsCommandHandlerTests
{
    private readonly Mock<ICreatorProfileRepository> _profileRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<SetCollaborationSettingsCommandHandler>> _loggerMock;
    private readonly SetCollaborationSettingsCommandHandler _handler;

    public SetCollaborationSettingsCommandHandlerTests()
    {
        _profileRepositoryMock = new Mock<ICreatorProfileRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<SetCollaborationSettingsCommandHandler>>();

        _handler = new SetCollaborationSettingsCommandHandler(
            _profileRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidDataShouldUpdateCollaborationSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        var command = new SetCollaborationSettingsCommand(userId, true, "Looking for tech collabs");

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OpenToCollaborations.Should().BeTrue();
        result.CollaborationPreferences.Should().Be("Looking for tech collabs");

        _profileRepositoryMock.Verify(x => x.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _profileRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNonExistentProfileShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SetCollaborationSettingsCommand(userId, true, null);

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleShouldInvalidateCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        var command = new SetCollaborationSettingsCommand(userId, true, null);

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

    [Fact]
    public async Task HandleTogglingOffCollaborationsShouldWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        profile.SetCollaborationSettings(true, "Open to collabs");

        var command = new SetCollaborationSettingsCommand(userId, false, null);

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.OpenToCollaborations.Should().BeFalse();
    }
}
