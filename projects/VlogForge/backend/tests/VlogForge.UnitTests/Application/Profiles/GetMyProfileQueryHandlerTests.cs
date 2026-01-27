using FluentAssertions;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.Queries.GetMyProfile;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Profiles;

/// <summary>
/// Unit tests for GetMyProfileQueryHandler.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
public class GetMyProfileQueryHandlerTests
{
    private readonly Mock<ICreatorProfileRepository> _profileRepositoryMock;
    private readonly GetMyProfileQueryHandler _handler;

    public GetMyProfileQueryHandlerTests()
    {
        _profileRepositoryMock = new Mock<ICreatorProfileRepository>();
        _handler = new GetMyProfileQueryHandler(_profileRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleWhenProfileExistsShouldReturnProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyProfileQuery(userId);
        var profile = CreatorProfile.Create(userId, "johndoe", "John Doe");

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("johndoe");
        result.DisplayName.Should().Be("John Doe");
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task HandleWhenNoProfileExistsShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyProfileQuery(userId);

        _profileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatorProfile?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
