using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.Commands.CreateProfile;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Profiles;

/// <summary>
/// Unit tests for CreateProfileCommandHandler.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class CreateProfileCommandHandlerTests
{
    private readonly Mock<ICreatorProfileRepository> _profileRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<CreateProfileCommandHandler>> _loggerMock;
    private readonly CreateProfileCommandHandler _handler;

    public CreateProfileCommandHandlerTests()
    {
        _profileRepositoryMock = new Mock<ICreatorProfileRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<CreateProfileCommandHandler>>();

        _handler = new CreateProfileCommandHandler(
            _profileRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidDataShouldCreateProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, "johndoe", "John Doe", "I create tech content");

        var user = CreateTestUser(userId);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UserHasProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _profileRepositoryMock
            .Setup(x => x.UsernameExistsAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("johndoe");
        result.DisplayName.Should().Be("John Doe");
        result.Bio.Should().Be("I create tech content");
        result.UserId.Should().Be(userId);

        _profileRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreatorProfile>(), It.IsAny<CancellationToken>()), Times.Once);
        _profileRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithNonExistentUserShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, "johndoe", "John Doe");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleWhenUserAlreadyHasProfileShouldThrowConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, "johndoe", "John Doe");

        var user = CreateTestUser(userId);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UserHasProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already has a profile*");
    }

    [Fact]
    public async Task HandleWhenUsernameExistsShouldThrowConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, "johndoe", "John Doe");

        var user = CreateTestUser(userId);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UserHasProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _profileRepositoryMock
            .Setup(x => x.UsernameExistsAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*username is already taken*");
    }

    [Fact]
    public async Task HandleWithNoBioShouldCreateProfileWithEmptyBio()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, "johndoe", "John Doe");

        var user = CreateTestUser(userId);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _profileRepositoryMock
            .Setup(x => x.UserHasProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _profileRepositoryMock
            .Setup(x => x.UsernameExistsAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Bio.Should().BeEmpty();
    }

    private static User CreateTestUser(Guid userId)
    {
        var email = Email.Create("test@example.com");
        var (user, _) = User.Create(email, "Test User", "hashed-password");

        // Use reflection to set the Id since it's generated in the constructor
        var idProperty = typeof(User).GetProperty("Id");
        idProperty!.SetValue(user, userId);

        return user;
    }
}
