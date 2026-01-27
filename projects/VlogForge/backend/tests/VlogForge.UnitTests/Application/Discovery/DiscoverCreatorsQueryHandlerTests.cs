using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Discovery.DTOs;
using VlogForge.Application.Discovery.Queries.DiscoverCreators;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using Xunit;

namespace VlogForge.UnitTests.Application.Discovery;

/// <summary>
/// Unit tests for DiscoverCreatorsQueryHandler.
/// Story: ACF-010
/// </summary>
[Trait("Story", "ACF-010")]
public class DiscoverCreatorsQueryHandlerTests
{
    private readonly Mock<ICreatorProfileRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<DiscoverCreatorsQueryHandler>> _loggerMock;
    private readonly DiscoverCreatorsQueryHandler _handler;

    public DiscoverCreatorsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ICreatorProfileRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<DiscoverCreatorsQueryHandler>>();

        _handler = new DiscoverCreatorsQueryHandler(
            _repositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCached_ShouldReturnFromCache()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 20);
        var cachedResponse = new DiscoveryResponse
        {
            Items = new List<DiscoveryCreatorDto>
            {
                new() { Username = "creator1", DisplayName = "Creator One" }
            },
            TotalCount = 1,
            HasMore = false
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(cachedResponse);
        _repositoryMock.Verify(
            x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotCached_ShouldFetchFromRepositoryAndCache()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 20);
        var profiles = CreateTestProfiles(3);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                null, null, null, null, null, null, null, null, 20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, profiles.Count));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.HasMore.Should().BeFalse();

        _cacheServiceMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<DiscoveryResponse>(),
                TimeSpan.FromMinutes(5),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNicheFilter_ShouldPassNormalizedNichesToRepository()
    {
        // Arrange
        var niches = new List<string> { "Gaming", "TECH", "  lifestyle  " };
        var query = new DiscoverCreatorsQuery(Niches: niches);
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.Is<IReadOnlyList<string>>(n =>
                    n.Contains("gaming") &&
                    n.Contains("tech") &&
                    n.Contains("lifestyle")),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(
            x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.Is<IReadOnlyList<string>>(n => n.Count == 3),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPlatformFilter_ShouldPassPlatformsToRepository()
    {
        // Arrange
        var platforms = new List<PlatformType> { PlatformType.YouTube, PlatformType.TikTok };
        var query = new DiscoverCreatorsQuery(Platforms: platforms);
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.Is<IReadOnlyList<PlatformType>>(p =>
                    p.Contains(PlatformType.YouTube) &&
                    p.Contains(PlatformType.TikTok)),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(AudienceSizeRange.Small, 1000, 10000)]
    [InlineData(AudienceSizeRange.Medium, 10000, 100000)]
    [InlineData(AudienceSizeRange.Large, 100000, null)]
    public async Task Handle_WithAudienceSizeFilter_ShouldConvertToFollowerRange(
        AudienceSizeRange audienceSize,
        int expectedMin,
        int? expectedMax)
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(AudienceSize: audienceSize);
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                expectedMin,
                expectedMax,
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchTermToRepository()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(SearchTerm: "gaming creator");
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                "gaming creator",
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithOpenToCollaborationFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(OpenToCollaboration: true);
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                true,
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithCursor_ShouldParseCursorAndPassToRepository()
    {
        // Arrange
        var cursorId = Guid.NewGuid();
        var query = new DiscoverCreatorsQuery(Cursor: cursorId.ToString());
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                cursorId,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithMoreResults_ShouldSetNextCursor()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 2);
        var profiles = CreateTestProfiles(2);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, true, 10)); // HasMore = true

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.HasMore.Should().BeTrue();
        result.NextCursor.Should().NotBeNullOrEmpty();
        result.NextCursor.Should().Be(profiles[^1].Id.ToString());
    }

    [Fact]
    public async Task Handle_WithExcludeUserId_ShouldExcludeUserFromResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new DiscoverCreatorsQuery(ExcludeUserId: userId);
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                userId,
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_EmptyResults_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery();
        var emptyProfiles = new List<CreatorProfile>();

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyProfiles.AsReadOnly(), false, 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.HasMore.Should().BeFalse();
        result.NextCursor.Should().BeNull();
    }

    private static IReadOnlyList<CreatorProfile> CreateTestProfiles(int count)
    {
        var profiles = new List<CreatorProfile>();
        for (var i = 1; i <= count; i++)
        {
            var profile = CreatorProfile.Create(
                Guid.NewGuid(),
                $"creator{i}",
                $"Creator {i}");
            profile.SetCollaborationSettings(true, "Open to all collaborations");
            profile.SetNicheTags(new[] { NicheTag.Create("gaming"), NicheTag.Create("tech") });
            profiles.Add(profile);
        }
        return profiles.AsReadOnly();
    }
}
