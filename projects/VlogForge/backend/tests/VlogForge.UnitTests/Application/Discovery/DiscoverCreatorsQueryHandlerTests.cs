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

    [Fact]
    public async Task Handle_WithInvalidCursor_ShouldPassNullCursorToRepository()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(Cursor: "not-a-valid-guid");
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
                null, // Invalid cursor should result in null
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
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                null,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyCursor_ShouldPassNullCursorToRepository()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(Cursor: "   ");
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
                null,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNullAudienceSize_ShouldPassNullFollowerRangeToRepository()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(AudienceSize: null);
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<IReadOnlyList<PlatformType>?>(),
                null, // minFollowers should be null
                null, // maxFollowers should be null
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
    public async Task Handle_WithWhitespaceOnlyNiches_ShouldFilterOutEmptyNiches()
    {
        // Arrange
        var niches = new List<string> { "gaming", "   ", "", "tech" };
        var query = new DiscoverCreatorsQuery(Niches: niches);
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.Is<IReadOnlyList<string>>(n => n.Count == 2 && n.Contains("gaming") && n.Contains("tech")),
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
    public async Task Handle_WithDuplicateNiches_ShouldDeduplicateNiches()
    {
        // Arrange
        var niches = new List<string> { "Gaming", "gaming", "GAMING" };
        var query = new DiscoverCreatorsQuery(Niches: niches);
        var profiles = CreateTestProfiles(1);

        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscoveryResponse?)null);

        _repositoryMock
            .Setup(x => x.DiscoverCreatorsAsync(
                It.IsAny<Guid?>(),
                It.Is<IReadOnlyList<string>>(n => n.Count == 1 && n.Contains("gaming")),
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
    public async Task Handle_ShouldBuildCorrectCacheKey_WithAllFilters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cursorId = Guid.NewGuid();
        var query = new DiscoverCreatorsQuery(
            ExcludeUserId: userId,
            Niches: new List<string> { "gaming", "tech" },
            Platforms: new List<PlatformType> { PlatformType.YouTube },
            AudienceSize: AudienceSizeRange.Medium,
            SearchTerm: "test",
            OpenToCollaboration: true,
            Cursor: cursorId.ToString(),
            PageSize: 25);
        var profiles = CreateTestProfiles(1);

        string? capturedCacheKey = null;
        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((key, _) => capturedCacheKey = key)
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
            .ReturnsAsync((profiles, false, 1));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedCacheKey.Should().NotBeNullOrEmpty();
        capturedCacheKey.Should().StartWith("discovery");
        capturedCacheKey.Should().Contain($"ex:{userId}");
        capturedCacheKey.Should().Contain("n:gaming,tech");
        capturedCacheKey.Should().Contain("p:YouTube");
        capturedCacheKey.Should().Contain("a:Medium");
        capturedCacheKey.Should().Contain("s:test");
        capturedCacheKey.Should().Contain("c:True");
        capturedCacheKey.Should().Contain($"cur:{cursorId}");
        capturedCacheKey.Should().Contain("ps:25");
    }

    [Fact]
    public async Task Handle_ShouldBuildMinimalCacheKey_WithNoFilters()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 20);
        var profiles = CreateTestProfiles(1);

        string? capturedCacheKey = null;
        _cacheServiceMock
            .Setup(x => x.GetAsync<DiscoveryResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((key, _) => capturedCacheKey = key)
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
            .ReturnsAsync((profiles, false, 1));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedCacheKey.Should().Be("discovery:ps:20");
    }

    [Fact]
    public async Task Handle_ShouldMapProfileToDto_WithAllFields()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 1);
        var profile = CreateTestProfileWithPlatforms();

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
            .ReturnsAsync((new List<CreatorProfile> { profile }.AsReadOnly(), false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var dto = result.Items[0];
        dto.Id.Should().Be(profile.Id);
        dto.Username.Should().Be(profile.Username);
        dto.DisplayName.Should().Be(profile.DisplayName);
        dto.Bio.Should().Be(profile.Bio.Value);
        dto.OpenToCollaborations.Should().Be(profile.OpenToCollaborations);
        dto.NicheTags.Should().HaveCount(2);
        dto.NicheTags.Should().Contain("gaming");
        dto.NicheTags.Should().Contain("tech");
        dto.Platforms.Should().HaveCount(1);
        dto.Platforms[0].PlatformType.Should().Be("YouTube");
        dto.Platforms[0].Handle.Should().Be("testchannel");
        dto.Platforms[0].FollowerCount.Should().Be(5000);
        dto.TotalFollowers.Should().Be(5000);
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalFollowers_FromMultiplePlatforms()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 1);
        var profile = CreateTestProfileWithMultiplePlatforms();

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
            .ReturnsAsync((new List<CreatorProfile> { profile }.AsReadOnly(), false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Items[0];
        dto.TotalFollowers.Should().Be(15000); // 5000 + 10000
        dto.Platforms.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoMoreResults_ShouldNotSetNextCursor()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 10);
        var profiles = CreateTestProfiles(3);

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
            .ReturnsAsync((profiles, false, 3)); // HasMore = false

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.HasMore.Should().BeFalse();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldSetPageSizeInResponse()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 35);
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
                It.IsAny<Guid?>(),
                35,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(35);
    }

    [Fact]
    public async Task Handle_WithOpenToCollaborationFalse_ShouldFilterCorrectly()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(OpenToCollaboration: false);
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
                false,
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((profiles, false, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
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

    private static CreatorProfile CreateTestProfileWithPlatforms()
    {
        var profile = CreatorProfile.Create(
            Guid.NewGuid(),
            "testcreator",
            "Test Creator");
        profile.SetCollaborationSettings(true, "Open to all collaborations");
        profile.SetNicheTags(new[] { NicheTag.Create("gaming"), NicheTag.Create("tech") });
        profile.UpdateBasicInfo("Test Creator", Bio.Create("This is a test bio for the creator profile."));

        var platform = ConnectedPlatform.Create(
            profile.Id,
            PlatformType.YouTube,
            "testchannel",
            "https://youtube.com/@testchannel");
        platform.UpdateFollowerCount(5000);
        profile.AddConnectedPlatform(platform);

        return profile;
    }

    private static CreatorProfile CreateTestProfileWithMultiplePlatforms()
    {
        var profile = CreatorProfile.Create(
            Guid.NewGuid(),
            "multicreator",
            "Multi Platform Creator");
        profile.SetCollaborationSettings(true, "Open to all collaborations");
        profile.SetNicheTags(new[] { NicheTag.Create("gaming"), NicheTag.Create("tech") });

        var youtube = ConnectedPlatform.Create(
            profile.Id,
            PlatformType.YouTube,
            "multichannel",
            "https://youtube.com/@multichannel");
        youtube.UpdateFollowerCount(5000);
        profile.AddConnectedPlatform(youtube);

        var tiktok = ConnectedPlatform.Create(
            profile.Id,
            PlatformType.TikTok,
            "multitiktok",
            "https://tiktok.com/@multitiktok");
        tiktok.UpdateFollowerCount(10000);
        profile.AddConnectedPlatform(tiktok);

        return profile;
    }
}
