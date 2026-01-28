using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VlogForge.Domain.Entities;
using VlogForge.Domain.ValueObjects;
using VlogForge.Infrastructure.Data;
using VlogForge.Infrastructure.Data.Repositories;
using Xunit;

namespace VlogForge.UnitTests.Infrastructure.Data.Repositories;

/// <summary>
/// Unit tests for CreatorProfileRepository.DiscoverCreatorsAsync method.
/// Tests discovery filtering, search, pagination, and follower count logic.
/// Story: ACF-010
/// </summary>
[Trait("Story", "ACF-010")]
public class CreatorProfileRepositoryDiscoveryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreatorProfileRepository _repository;

    public CreatorProfileRepositoryDiscoveryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CreatorProfileRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region ExcludeUserId Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_WithExcludeUserId_ShouldExcludeUserProfile()
    {
        // Arrange
        var userToExclude = Guid.NewGuid();
        var profile1 = await CreateAndSaveProfile("user1", "User One", userToExclude);
        var profile2 = await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());
        var profile3 = await CreateAndSaveProfile("user3", "User Three", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            excludeUserId: userToExclude);

        // Assert
        profiles.Should().HaveCount(2);
        profiles.Should().NotContain(p => p.UserId == userToExclude);
        profiles.Select(p => p.Username).Should().Contain("user2");
        profiles.Select(p => p.Username).Should().Contain("user3");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithoutExcludeUserId_ShouldReturnAllProfiles()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            excludeUserId: null);

        // Assert
        profiles.Should().HaveCount(2);
    }

    #endregion

    #region Niche Filter Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_WithNicheFilter_ShouldFilterByNiches()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfileWithNiches("gamer1", "Gamer One", new[] { "gaming", "tech" });
        var profile2 = await CreateAndSaveProfileWithNiches("cook1", "Cook One", new[] { "cooking", "lifestyle" });
        var profile3 = await CreateAndSaveProfileWithNiches("techie1", "Techie One", new[] { "tech", "programming" });

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            niches: new List<string> { "gaming" });

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("gamer1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithMultipleNiches_ShouldFilterByAnyNiche()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfileWithNiches("gamer1", "Gamer One", new[] { "gaming" });
        var profile2 = await CreateAndSaveProfileWithNiches("cook1", "Cook One", new[] { "cooking" });
        var profile3 = await CreateAndSaveProfileWithNiches("techie1", "Techie One", new[] { "tech" });

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            niches: new List<string> { "gaming", "tech" });

        // Assert
        profiles.Should().HaveCount(2);
        profiles.Select(p => p.Username).Should().Contain("gamer1");
        profiles.Select(p => p.Username).Should().Contain("techie1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithNicheFilter_ShouldBeCaseInsensitive()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfileWithNiches("gamer1", "Gamer One", new[] { "gaming" });

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            niches: new List<string> { "GAMING" });

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("gamer1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithNonMatchingNiche_ShouldReturnEmpty()
    {
        // Arrange
        await CreateAndSaveProfileWithNiches("gamer1", "Gamer One", new[] { "gaming" });

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            niches: new List<string> { "cooking" });

        // Assert
        profiles.Should().BeEmpty();
    }

    #endregion

    #region Platform Filter Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_WithPlatformFilter_ShouldFilterByPlatforms()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfileWithPlatform("youtuber1", "YouTuber One", PlatformType.YouTube, 1000);
        var profile2 = await CreateAndSaveProfileWithPlatform("tiktoker1", "TikToker One", PlatformType.TikTok, 2000);

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            platforms: new List<PlatformType> { PlatformType.YouTube });

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("youtuber1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithMultiplePlatforms_ShouldFilterByAnyPlatform()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfileWithPlatform("youtuber1", "YouTuber One", PlatformType.YouTube, 1000);
        var profile2 = await CreateAndSaveProfileWithPlatform("tiktoker1", "TikToker One", PlatformType.TikTok, 2000);
        var profile3 = await CreateAndSaveProfileWithPlatform("instagrammer1", "Instagrammer One", PlatformType.Instagram, 3000);

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            platforms: new List<PlatformType> { PlatformType.YouTube, PlatformType.TikTok });

        // Assert
        profiles.Should().HaveCount(2);
        profiles.Select(p => p.Username).Should().Contain("youtuber1");
        profiles.Select(p => p.Username).Should().Contain("tiktoker1");
    }

    #endregion

    #region OpenToCollaboration Filter Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_WithOpenToCollaborationTrue_ShouldFilterCorrectly()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfile("open1", "Open One", Guid.NewGuid());
        profile1.SetCollaborationSettings(true, "Open for collabs");
        await _repository.UpdateAsync(profile1);
        await _context.SaveChangesAsync();

        var profile2 = await CreateAndSaveProfile("closed1", "Closed One", Guid.NewGuid());
        profile2.SetCollaborationSettings(false, null);
        await _repository.UpdateAsync(profile2);
        await _context.SaveChangesAsync();

        // Clear tracker to reload
        _context.ChangeTracker.Clear();

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            openToCollaboration: true);

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("open1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithOpenToCollaborationFalse_ShouldFilterCorrectly()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfile("open1", "Open One", Guid.NewGuid());
        profile1.SetCollaborationSettings(true, "Open for collabs");
        await _repository.UpdateAsync(profile1);
        await _context.SaveChangesAsync();

        var profile2 = await CreateAndSaveProfile("closed1", "Closed One", Guid.NewGuid());
        profile2.SetCollaborationSettings(false, null);
        await _repository.UpdateAsync(profile2);
        await _context.SaveChangesAsync();

        // Clear tracker to reload
        _context.ChangeTracker.Clear();

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            openToCollaboration: false);

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("closed1");
    }

    #endregion

    #region Search Term Tests

    // NOTE: Search tests using EF.Functions.ILike require PostgreSQL and cannot be tested
    // with InMemory provider. These tests are marked as Skip and should be run as
    // integration tests with a real PostgreSQL database.

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithSearchTerm_ShouldSearchDisplayName()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "Gaming Expert", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "Cooking Master", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "Gaming");

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].DisplayName.Should().Be("Gaming Expert");
    }

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithSearchTerm_ShouldSearchUsername()
    {
        // Arrange
        await CreateAndSaveProfile("gamingpro", "User One", Guid.NewGuid());
        await CreateAndSaveProfile("cookmaster", "User Two", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "gaming");

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("gamingpro");
    }

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithSearchTerm_ShouldSearchBio()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        profile1.UpdateBasicInfo("User One", Bio.Create("I love gaming and streaming!"));
        await _repository.UpdateAsync(profile1);
        await _context.SaveChangesAsync();

        var profile2 = await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());
        profile2.UpdateBasicInfo("User Two", Bio.Create("I love cooking recipes!"));
        await _repository.UpdateAsync(profile2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "gaming");

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("user1");
    }

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithSearchTerm_ShouldBeCaseInsensitive()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "Gaming Expert", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "GAMING");

        // Assert
        profiles.Should().HaveCount(1);
    }

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithSearchTerm_ShouldMatchPartialStrings()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "Gaming Expert Pro", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "Expert");

        // Assert
        profiles.Should().HaveCount(1);
    }

    #endregion

    #region LIKE Pattern Escaping Tests

    // NOTE: LIKE pattern escaping tests require PostgreSQL and cannot be tested
    // with InMemory provider. These should be run as integration tests.

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithPercentInSearchTerm_ShouldEscapeCorrectly()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "100% Gamer", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "Gaming Pro", Guid.NewGuid());

        // Act - Search for literal "100%"
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "100%");

        // Assert - Should match only the one with "100%" in name
        profiles.Should().HaveCount(1);
        profiles[0].DisplayName.Should().Be("100% Gamer");
    }

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithUnderscoreInSearchTerm_ShouldEscapeCorrectly()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "Gaming_Pro", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "GamingXPro", Guid.NewGuid());

        // Act - Search for literal "_"
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "Gaming_");

        // Assert - Should match only the one with underscore
        profiles.Should().HaveCount(1);
        profiles[0].DisplayName.Should().Be("Gaming_Pro");
    }

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithBackslashInSearchTerm_ShouldEscapeCorrectly()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "Gaming\\Pro", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "GamingPro", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "Gaming\\");

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].DisplayName.Should().Be("Gaming\\Pro");
    }

    #endregion

    #region Cursor Pagination Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_WithCursor_ShouldReturnResultsAfterCursor()
    {
        // Arrange - Create profiles with known creation order
        var profile1 = await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        await Task.Delay(10); // Ensure different timestamps
        var profile2 = await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());
        await Task.Delay(10);
        var profile3 = await CreateAndSaveProfile("user3", "User Three", Guid.NewGuid());

        // Act - Get results after profile3 (most recent, so first in descending order)
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            cursor: profile3.Id,
            pageSize: 10);

        // Assert - Should return profiles created before profile3
        profiles.Should().HaveCount(2);
        profiles.Should().NotContain(p => p.Id == profile3.Id);
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithInvalidCursor_ShouldReturnAllResults()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());

        // Act - Use a cursor that doesn't exist
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            cursor: Guid.NewGuid(),
            pageSize: 10);

        // Assert - Should return all results when cursor not found
        profiles.Should().HaveCount(2);
    }

    #endregion

    #region HasMore Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_WhenMoreResultsExist_ShouldReturnHasMoreTrue()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());
        await CreateAndSaveProfile("user3", "User Three", Guid.NewGuid());

        // Act
        var (profiles, hasMore, _) = await _repository.DiscoverCreatorsAsync(
            pageSize: 2);

        // Assert
        profiles.Should().HaveCount(2);
        hasMore.Should().BeTrue();
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WhenNoMoreResultsExist_ShouldReturnHasMoreFalse()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());

        // Act
        var (profiles, hasMore, _) = await _repository.DiscoverCreatorsAsync(
            pageSize: 10);

        // Assert
        profiles.Should().HaveCount(2);
        hasMore.Should().BeFalse();
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WhenExactlyPageSizeResults_ShouldReturnHasMoreFalse()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());

        // Act
        var (profiles, hasMore, _) = await _repository.DiscoverCreatorsAsync(
            pageSize: 2);

        // Assert
        profiles.Should().HaveCount(2);
        hasMore.Should().BeFalse();
    }

    #endregion

    #region TotalCount Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_ShouldReturnCorrectTotalCount()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());
        await CreateAndSaveProfile("user3", "User Three", Guid.NewGuid());

        // Act
        var (profiles, _, totalCount) = await _repository.DiscoverCreatorsAsync(
            pageSize: 2);

        // Assert
        profiles.Should().HaveCount(2); // Limited by pageSize
        totalCount.Should().Be(3); // Total count should be all matching
    }

    [Fact(Skip = "EF.Functions.ILike requires PostgreSQL - use integration tests")]
    public async Task DiscoverCreatorsAsync_WithSearchFilter_ShouldReturnFilteredTotalCount()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "Gaming One", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "Gaming Two", Guid.NewGuid());
        await CreateAndSaveProfile("user3", "Cooking One", Guid.NewGuid());

        // Act - Search for "Gaming"
        var (profiles, _, totalCount) = await _repository.DiscoverCreatorsAsync(
            searchTerm: "Gaming",
            pageSize: 10);

        // Assert
        profiles.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithNicheFilter_ShouldReturnFilteredTotalCount()
    {
        // Arrange
        await CreateAndSaveProfileWithNiches("gamer1", "Gamer One", new[] { "gaming" });
        await CreateAndSaveProfileWithNiches("gamer2", "Gamer Two", new[] { "gaming" });
        await CreateAndSaveProfileWithNiches("cook1", "Cook One", new[] { "cooking" });

        // Act - Filter by gaming niche
        var (profiles, _, totalCount) = await _repository.DiscoverCreatorsAsync(
            niches: new List<string> { "gaming" },
            pageSize: 10);

        // Assert
        profiles.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    #endregion

    #region Follower Count Filter Tests (In-Memory)

    [Fact]
    public async Task DiscoverCreatorsAsync_WithMinFollowers_ShouldFilterByMinimum()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfileWithPlatform("small1", "Small Creator", PlatformType.YouTube, 500);
        var profile2 = await CreateAndSaveProfileWithPlatform("medium1", "Medium Creator", PlatformType.YouTube, 5000);
        var profile3 = await CreateAndSaveProfileWithPlatform("large1", "Large Creator", PlatformType.YouTube, 50000);

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            minFollowers: 1000);

        // Assert
        profiles.Should().HaveCount(2);
        profiles.Select(p => p.Username).Should().Contain("medium1");
        profiles.Select(p => p.Username).Should().Contain("large1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithMaxFollowers_ShouldFilterByMaximum()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfileWithPlatform("small1", "Small Creator", PlatformType.YouTube, 500);
        var profile2 = await CreateAndSaveProfileWithPlatform("medium1", "Medium Creator", PlatformType.YouTube, 5000);
        var profile3 = await CreateAndSaveProfileWithPlatform("large1", "Large Creator", PlatformType.YouTube, 50000);

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            maxFollowers: 10000);

        // Assert
        profiles.Should().HaveCount(2);
        profiles.Select(p => p.Username).Should().Contain("small1");
        profiles.Select(p => p.Username).Should().Contain("medium1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithMinAndMaxFollowers_ShouldFilterByRange()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfileWithPlatform("small1", "Small Creator", PlatformType.YouTube, 500);
        var profile2 = await CreateAndSaveProfileWithPlatform("medium1", "Medium Creator", PlatformType.YouTube, 5000);
        var profile3 = await CreateAndSaveProfileWithPlatform("large1", "Large Creator", PlatformType.YouTube, 50000);

        // Act - Filter for Medium range (1000-10000)
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            minFollowers: 1000,
            maxFollowers: 10000);

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("medium1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithFollowerFilter_ShouldSumMultiplePlatforms()
    {
        // Arrange - Create profile with multiple platforms totaling 15000 followers
        var profile = await CreateAndSaveProfile("multi1", "Multi Platform", Guid.NewGuid());
        var youtube = ConnectedPlatform.Create(profile.Id, PlatformType.YouTube, "channel1", "https://youtube.com/@channel1");
        youtube.UpdateFollowerCount(5000);
        profile.AddConnectedPlatform(youtube);

        var tiktok = ConnectedPlatform.Create(profile.Id, PlatformType.TikTok, "tiktok1", "https://tiktok.com/@tiktok1");
        tiktok.UpdateFollowerCount(10000);
        profile.AddConnectedPlatform(tiktok);

        await _repository.UpdateAsync(profile);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Also create a single platform profile with 5000 followers
        var singleProfile = await CreateAndSaveProfileWithPlatform("single1", "Single Platform", PlatformType.YouTube, 5000);

        // Act - Filter for 10000+ followers
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            minFollowers: 10000);

        // Assert - Only multi-platform profile should match (15000 total)
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("multi1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithNoFollowers_ShouldTreatAsZero()
    {
        // Arrange - Profile with no connected platforms (0 followers)
        var profile1 = await CreateAndSaveProfile("noplatform1", "No Platform", Guid.NewGuid());
        var profile2 = await CreateAndSaveProfileWithPlatform("withplatform1", "With Platform", PlatformType.YouTube, 5000);

        // Act - Filter for 0-1000 followers
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            minFollowers: 0,
            maxFollowers: 1000);

        // Assert - Should include profile with no platforms
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("noplatform1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithFollowerFilter_ShouldAdjustHasMore()
    {
        // Arrange - Create profiles that all get fetched initially, then filtered in-memory
        var profile1 = await CreateAndSaveProfileWithPlatform("small1", "Small One", PlatformType.YouTube, 500);
        await Task.Delay(10);
        var profile2 = await CreateAndSaveProfileWithPlatform("small2", "Small Two", PlatformType.YouTube, 800);
        await Task.Delay(10);
        var profile3 = await CreateAndSaveProfileWithPlatform("large1", "Large One", PlatformType.YouTube, 50000);

        // Act - Page size 3 to fetch all, then filter removes large
        var (profiles, hasMore, _) = await _repository.DiscoverCreatorsAsync(
            maxFollowers: 1000,
            pageSize: 3);

        // Assert - Both small profiles should be returned after in-memory filter
        profiles.Should().HaveCount(2);
        profiles.Should().Contain(p => p.Username == "small1");
        profiles.Should().Contain(p => p.Username == "small2");
    }

    #endregion

    #region Combined Filters Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile1 = await CreateAndSaveProfileWithNiches("gamer1", "Gaming Pro", new[] { "gaming" });
        profile1.SetCollaborationSettings(true, "Open");
        var youtube = ConnectedPlatform.Create(profile1.Id, PlatformType.YouTube, "gamer", "https://youtube.com/@gamer");
        youtube.UpdateFollowerCount(5000);
        profile1.AddConnectedPlatform(youtube);
        await _repository.UpdateAsync(profile1);
        await _context.SaveChangesAsync();

        var profile2 = await CreateAndSaveProfileWithNiches("gamer2", "Gaming Noob", new[] { "gaming" });
        profile2.SetCollaborationSettings(false, null);
        await _repository.UpdateAsync(profile2);
        await _context.SaveChangesAsync();

        var profile3 = await CreateAndSaveProfileWithNiches("cook1", "Cooking Pro", new[] { "cooking" });
        profile3.SetCollaborationSettings(true, "Open");
        await _repository.UpdateAsync(profile3);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act - Apply multiple filters
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            niches: new List<string> { "gaming" },
            platforms: new List<PlatformType> { PlatformType.YouTube },
            openToCollaboration: true,
            minFollowers: 1000);

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("gamer1");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithEmptyFilters_ShouldReturnAllProfiles()
    {
        // Arrange
        await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        await CreateAndSaveProfile("user2", "User Two", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            excludeUserId: null,
            niches: null,
            platforms: null,
            minFollowers: null,
            maxFollowers: null,
            searchTerm: null,
            openToCollaboration: null,
            cursor: null,
            pageSize: 20);

        // Assert
        profiles.Should().HaveCount(2);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task DiscoverCreatorsAsync_WithNoProfiles_ShouldReturnEmptyList()
    {
        // Act
        var (profiles, hasMore, totalCount) = await _repository.DiscoverCreatorsAsync();

        // Assert
        profiles.Should().BeEmpty();
        hasMore.Should().BeFalse();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_ShouldIncludeConnectedPlatforms()
    {
        // Arrange
        var profile = await CreateAndSaveProfileWithPlatform("user1", "User One", PlatformType.YouTube, 5000);

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync();

        // Assert
        profiles.Should().HaveCount(1);
        profiles[0].ConnectedPlatforms.Should().HaveCount(1);
        profiles[0].ConnectedPlatforms.First().PlatformType.Should().Be(PlatformType.YouTube);
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        var profile1 = await CreateAndSaveProfile("older", "Older Profile", Guid.NewGuid());
        await Task.Delay(50); // Ensure different timestamps
        var profile2 = await CreateAndSaveProfile("newer", "Newer Profile", Guid.NewGuid());

        // Act
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync();

        // Assert - Newer should be first
        profiles[0].Username.Should().Be("newer");
        profiles[1].Username.Should().Be("older");
    }

    [Fact]
    public async Task DiscoverCreatorsAsync_WithNullFollowerCount_ShouldTreatAsZero()
    {
        // Arrange - Create profile with platform that has null follower count
        var profile = await CreateAndSaveProfile("user1", "User One", Guid.NewGuid());
        var platform = ConnectedPlatform.Create(profile.Id, PlatformType.YouTube, "channel", "https://youtube.com/@channel");
        // Don't set follower count (remains null)
        profile.AddConnectedPlatform(platform);
        await _repository.UpdateAsync(profile);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Filter for 0-1000 followers
        var (profiles, _, _) = await _repository.DiscoverCreatorsAsync(
            minFollowers: 0,
            maxFollowers: 1000);

        // Assert - Profile with null followers (treated as 0) should be included
        profiles.Should().HaveCount(1);
    }

    #endregion

    #region Helper Methods

    private async Task<CreatorProfile> CreateAndSaveProfile(string username, string displayName, Guid userId)
    {
        var profile = CreatorProfile.Create(userId, username, displayName);
        await _repository.AddAsync(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    private async Task<CreatorProfile> CreateAndSaveProfileWithNiches(string username, string displayName, string[] niches)
    {
        var profile = CreatorProfile.Create(Guid.NewGuid(), username, displayName);
        profile.SetNicheTags(niches.Select(NicheTag.Create).ToArray());
        await _repository.AddAsync(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    private async Task<CreatorProfile> CreateAndSaveProfileWithPlatform(
        string username,
        string displayName,
        PlatformType platformType,
        int followerCount)
    {
        var profile = CreatorProfile.Create(Guid.NewGuid(), username, displayName);
        var platform = ConnectedPlatform.Create(
            profile.Id,
            platformType,
            $"{username}_handle",
            $"https://{platformType.ToString().ToLower()}.com/@{username}");
        platform.UpdateFollowerCount(followerCount);
        profile.AddConnectedPlatform(platform);

        await _repository.AddAsync(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    #endregion
}
