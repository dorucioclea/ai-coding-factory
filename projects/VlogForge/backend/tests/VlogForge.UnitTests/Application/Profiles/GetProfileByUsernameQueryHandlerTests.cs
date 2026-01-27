using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.DTOs;
using VlogForge.Application.Profiles.Queries.GetProfileByUsername;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Profiles;

/// <summary>
/// Unit tests for GetProfileByUsernameQueryHandler.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class GetProfileByUsernameQueryHandlerTests
{
    private readonly Mock<ICreatorProfileRepository> _profileRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<GetProfileByUsernameQueryHandler>> _loggerMock;
    private readonly GetProfileByUsernameQueryHandler _handler;

    public GetProfileByUsernameQueryHandlerTests()
    {
        _profileRepositoryMock = new Mock<ICreatorProfileRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<GetProfileByUsernameQueryHandler>>();

        _handler = new GetProfileByUsernameQueryHandler(
            _profileRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWhenCachedShouldReturnFromCache()
    {
        // Arrange
        var query = new GetProfileByUsernameQuery("johndoe");
        var cachedProfile = new PublicProfileResponse
        {
            Username = "johndoe",
            DisplayName = "John Doe",
            Bio = "Cached bio"
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<PublicProfileResponse>("profile:username:johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedProfile);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(cachedProfile);
        _profileRepositoryMock.Verify(x => x.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleWhenNotCachedShouldFetchFromDatabaseAndCache()
    {
        // Arrange
        var query = new GetProfileByUsernameQuery("johndoe");
        var profile = CreatorProfile.Create(Guid.NewGuid(), "johndoe", "John Doe");

        _cacheServiceMock
            .Setup(x => x.GetAsync<PublicProfileResponse>("profile:username:johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublicProfileResponse?)null);

        _profileRepositoryMock
            .Setup(x => x.GetByUsernameAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("johndoe");
        result.DisplayName.Should().Be("John Doe");

        _cacheServiceMock.Verify(
            x => x.SetAsync(
                "profile:username:johndoe",
                It.IsAny<PublicProfileResponse>(),
                TimeSpan.FromMinutes(5),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleWithNonExistentUsernameShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetProfileByUsernameQuery("nonexistent");

        _cacheServiceMock
            .Setup(x => x.GetAsync<PublicProfileResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublicProfileResponse?)null);

        _profileRepositoryMock
            .Setup(x => x.GetByUsernameAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleShouldNormalizeUsername()
    {
        // Arrange
        var query = new GetProfileByUsernameQuery("  JohnDoe  ");
        var profile = CreatorProfile.Create(Guid.NewGuid(), "johndoe", "John Doe");

        _cacheServiceMock
            .Setup(x => x.GetAsync<PublicProfileResponse>("profile:username:johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublicProfileResponse?)null);

        _profileRepositoryMock
            .Setup(x => x.GetByUsernameAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _profileRepositoryMock.Verify(x => x.GetByUsernameAsync("johndoe", It.IsAny<CancellationToken>()), Times.Once);
    }
}
