using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using VlogForge.Application.Profiles.Commands.UploadProfilePicture;
using Xunit;

namespace VlogForge.UnitTests.Application.Profiles;

/// <summary>
/// Unit tests for UploadProfilePictureCommandValidator.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class UploadProfilePictureCommandValidatorTests
{
    private readonly UploadProfilePictureCommandValidator _validator = new();

    [Fact]
    public void ValidCommandShouldPassValidation()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            stream,
            "avatar.jpg",
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyUserIdShouldFailValidation()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(
            Guid.Empty,
            stream,
            "avatar.jpg",
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void NullImageStreamShouldFailValidation()
    {
        // Arrange
        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            null!,
            "avatar.jpg",
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageStream)
            .WithErrorMessage("Image stream is required.");
    }

    [Fact]
    public void FileSizeExceeding5MBShouldFailValidation()
    {
        // Arrange
        var mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.Length).Returns(6 * 1024 * 1024); // 6 MB
        mockStream.Setup(s => s.CanRead).Returns(true);

        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            mockStream.Object,
            "large.jpg",
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageStream)
            .WithErrorMessage("Image file size cannot exceed 5 MB.");
    }

    [Fact]
    public void UnreadableStreamShouldFailValidation()
    {
        // Arrange
        var mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.Length).Returns(1024);
        mockStream.Setup(s => s.CanRead).Returns(false);

        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            mockStream.Object,
            "avatar.jpg",
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageStream)
            .WithErrorMessage("Image stream must be readable.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyFileNameShouldFailValidation(string? fileName)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            stream,
            fileName!,
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".gif")]
    [InlineData(".webp")]
    public void ValidFileExtensionShouldPassValidation(string extension)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            stream,
            $"avatar{extension}",
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("video.mp4")]
    [InlineData("archive.zip")]
    [InlineData("text.txt")]
    [InlineData("noextension")]
    public void InvalidFileExtensionShouldFailValidation(string fileName)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            stream,
            fileName,
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("Invalid file extension. Allowed extensions: .jpg, .jpeg, .png, .gif, .webp");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyContentTypeShouldFailValidation(string? contentType)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            stream,
            "avatar.jpg",
            contentType!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    public void ValidContentTypeShouldPassValidation(string contentType)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            stream,
            "avatar.jpg",
            contentType);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ContentType);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("video/mp4")]
    [InlineData("application/octet-stream")]
    public void InvalidContentTypeShouldFailValidation(string contentType)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            stream,
            "avatar.jpg",
            contentType);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentType)
            .WithErrorMessage("Invalid content type. Allowed types: image/jpeg, image/png, image/gif, image/webp");
    }

    [Fact]
    public void FileSizeExactly5MBShouldPassValidation()
    {
        // Arrange
        var mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.Length).Returns(5 * 1024 * 1024); // Exactly 5 MB
        mockStream.Setup(s => s.CanRead).Returns(true);

        var command = new UploadProfilePictureCommand(
            Guid.NewGuid(),
            mockStream.Object,
            "avatar.jpg",
            "image/jpeg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageStream);
    }
}
