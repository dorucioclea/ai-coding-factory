using FluentAssertions;
using FluentValidation.TestHelper;
using VlogForge.Application.Discovery.Queries.DiscoverCreators;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Discovery;

/// <summary>
/// Unit tests for DiscoverCreatorsQueryValidator.
/// Story: ACF-010
/// </summary>
[Trait("Story", "ACF-010")]
public class DiscoverCreatorsQueryValidatorTests
{
    private readonly DiscoverCreatorsQueryValidator _validator;

    public DiscoverCreatorsQueryValidatorTests()
    {
        _validator = new DiscoverCreatorsQueryValidator();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(50)]
    public void Validate_ValidPageSize_ShouldNotHaveValidationError(int pageSize)
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: pageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]
    [InlineData(100)]
    public void Validate_InvalidPageSize_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: pageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_SearchTermWithinLimit_ShouldNotHaveValidationError()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(SearchTerm: new string('a', 100));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void Validate_SearchTermExceedsLimit_ShouldHaveValidationError()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(SearchTerm: new string('a', 101));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void Validate_NullSearchTerm_ShouldNotHaveValidationError()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(SearchTerm: null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void Validate_NichesWithinLimit_ShouldNotHaveValidationError()
    {
        // Arrange
        var niches = Enumerable.Range(1, 10).Select(i => $"niche{i}").ToList();
        var query = new DiscoverCreatorsQuery(Niches: niches);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Niches);
    }

    [Fact]
    public void Validate_NichesExceedsLimit_ShouldHaveValidationError()
    {
        // Arrange
        var niches = Enumerable.Range(1, 11).Select(i => $"niche{i}").ToList();
        var query = new DiscoverCreatorsQuery(Niches: niches);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Niches);
    }

    [Fact]
    public void Validate_PlatformsWithinLimit_ShouldNotHaveValidationError()
    {
        // Arrange
        var platforms = new List<PlatformType> { PlatformType.YouTube, PlatformType.TikTok, PlatformType.Instagram };
        var query = new DiscoverCreatorsQuery(Platforms: platforms);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Platforms);
    }

    [Fact]
    public void Validate_PlatformsExceedsLimit_ShouldHaveValidationError()
    {
        // Arrange
        var platforms = new List<PlatformType>
        {
            PlatformType.YouTube,
            PlatformType.TikTok,
            PlatformType.Instagram,
            PlatformType.Twitter,
            PlatformType.Twitch,
            PlatformType.LinkedIn
        };
        var query = new DiscoverCreatorsQuery(Platforms: platforms);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Platforms);
    }

    [Fact]
    public void Validate_EmptyQuery_ShouldBeValid()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FullyPopulatedValidQuery_ShouldBeValid()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(
            ExcludeUserId: Guid.NewGuid(),
            Niches: new List<string> { "gaming", "tech" },
            Platforms: new List<PlatformType> { PlatformType.YouTube },
            AudienceSize: VlogForge.Application.Discovery.DTOs.AudienceSizeRange.Medium,
            SearchTerm: "content creator",
            OpenToCollaboration: true,
            Cursor: Guid.NewGuid().ToString(),
            PageSize: 25);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
