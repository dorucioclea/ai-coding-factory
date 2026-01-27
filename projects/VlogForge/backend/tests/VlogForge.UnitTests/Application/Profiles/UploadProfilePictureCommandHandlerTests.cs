using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.Commands.UploadProfilePicture;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Profiles;

/// <summary>
/// Unit tests for UploadProfilePictureCommandHandler.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class UploadProfilePictureCommandHandlerTests
{
    private readonly Mock<ICreatorProfileRepository> _profileRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<UploadProfilePictureCommandHandler>> _loggerMock;
    private readonly UploadProfilePictureCommandHandler _handler;

    public UploadProfilePictureCommandHandlerTests()
    {
        _profileRepositoryMock = new Mock<ICreatorProfileRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<UploadProfilePictureCommandHandler>>();

        _handler = new UploadProfilePictureCommandHandler(
            _profileRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidImageShouldUploadAndUpdateProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        using var imageStream = new MemoryStream(new byte[1024]);

        var command = new UploadProfilePictureCommand(
            userId,
            imageStream,
            "avatar.jpg",
            "image/jpeg");

        var pictureUrls = new ProfilePictureUrls(
            "https://storage.example.com/thumbnails/avatar.jpg",
            "https://storage.example.com/profiles/avatar.jpg");

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _fileStorageServiceMock
            .Setup(x => x.UploadProfilePictureAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pictureUrls);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProfilePictureUrl.Should().Be(pictureUrls.StandardUrl);

        _profileRepositoryMock.Verify(x => x.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _profileRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    public async Task HandleWithAllowedContentTypeShouldSucceed(string contentType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        using var imageStream = new MemoryStream(new byte[1024]);

        var command = new UploadProfilePictureCommand(userId, imageStream, "avatar.jpg", contentType);

        var pictureUrls = new ProfilePictureUrls(
            "https://storage.example.com/thumbnails/thumb.jpg",
            "https://storage.example.com/profiles/standard.jpg");

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _fileStorageServiceMock
            .Setup(x => x.UploadProfilePictureAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pictureUrls);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("video/mp4")]
    public async Task HandleWithDisallowedContentTypeShouldThrowValidationException(string contentType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        using var imageStream = new MemoryStream(new byte[1024]);

        var command = new UploadProfilePictureCommand(userId, imageStream, "file.pdf", contentType);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*not allowed*");
    }

    [Fact]
    public async Task HandleWithFileTooLargeShouldThrowValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        // Create a stream that reports a size larger than 5MB
        var largeStream = new Mock<Stream>();
        largeStream.Setup(s => s.Length).Returns(6 * 1024 * 1024); // 6 MB

        var command = new UploadProfilePictureCommand(userId, largeStream.Object, "large.jpg", "image/jpeg");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*exceeds maximum*");
    }

    [Fact]
    public async Task HandleWithNonExistentProfileShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using var imageStream = new MemoryStream(new byte[1024]);

        var command = new UploadProfilePictureCommand(userId, imageStream, "avatar.jpg", "image/jpeg");

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleShouldDeleteOldPictureWhenReplacingExisting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        var oldPictureUrl = "https://storage.example.com/old-picture.jpg";
        profile.UpdateProfilePicture(oldPictureUrl);

        using var imageStream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(userId, imageStream, "new-avatar.jpg", "image/jpeg");

        var pictureUrls = new ProfilePictureUrls(
            "https://storage.example.com/thumbnails/thumb.jpg",
            "https://storage.example.com/profiles/standard.jpg");

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _fileStorageServiceMock
            .Setup(x => x.UploadProfilePictureAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pictureUrls);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileStorageServiceMock.Verify(
            x => x.DeleteAsync(oldPictureUrl, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldInvalidateCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");
        using var imageStream = new MemoryStream(new byte[1024]);

        var command = new UploadProfilePictureCommand(userId, imageStream, "avatar.jpg", "image/jpeg");
        var pictureUrls = new ProfilePictureUrls(
            "https://storage.example.com/thumbnails/thumb.jpg",
            "https://storage.example.com/profiles/standard.jpg");

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _fileStorageServiceMock
            .Setup(x => x.UploadProfilePictureAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pictureUrls);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(
            x => x.RemoveAsync("profile:username:johndoe", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
