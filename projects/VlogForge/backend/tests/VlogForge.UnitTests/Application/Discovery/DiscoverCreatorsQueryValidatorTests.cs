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

    [Fact]
    public void Validate_NicheWithin50Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var niches = new List<string> { new string('a', 50) };
        var query = new DiscoverCreatorsQuery(Niches: niches);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Niches);
    }

    [Fact]
    public void Validate_NicheExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        var niches = new List<string> { new string('a', 51) };
        var query = new DiscoverCreatorsQuery(Niches: niches);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validate_MultipleNichesWithOneExceeding50Characters_ShouldHaveValidationError()
    {
        // Arrange
        var niches = new List<string> { "gaming", new string('a', 51), "tech" };
        var query = new DiscoverCreatorsQuery(Niches: niches);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validate_PageSizeOf0_ShouldHaveCorrectErrorMessage()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 0);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 50.");
    }

    [Fact]
    public void Validate_PageSizeOf51_ShouldHaveCorrectErrorMessage()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(PageSize: 51);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 50.");
    }

    [Fact]
    public void Validate_SearchTermExceedsLimit_ShouldHaveCorrectErrorMessage()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(SearchTerm: new string('a', 101));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
            .WithErrorMessage("Search term cannot exceed 100 characters.");
    }

    [Fact]
    public void Validate_NichesExceedsLimit_ShouldHaveCorrectErrorMessage()
    {
        // Arrange
        var niches = Enumerable.Range(1, 11).Select(i => $"niche{i}").ToList();
        var query = new DiscoverCreatorsQuery(Niches: niches);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Niches)
            .WithErrorMessage("Cannot filter by more than 10 niches at once.");
    }

    [Fact]
    public void Validate_PlatformsExceedsLimit_ShouldHaveCorrectErrorMessage()
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
        result.ShouldHaveValidationErrorFor(x => x.Platforms)
            .WithErrorMessage("Cannot filter by more than 5 platforms at once.");
    }

    [Fact]
    public void Validate_ExactlyMaxPlatforms_ShouldNotHaveValidationError()
    {
        // Arrange
        var platforms = new List<PlatformType>
        {
            PlatformType.YouTube,
            PlatformType.TikTok,
            PlatformType.Instagram,
            PlatformType.Twitter,
            PlatformType.Twitch
        };
        var query = new DiscoverCreatorsQuery(Platforms: platforms);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Platforms);
    }

    [Fact]
    public void Validate_ExactlyMaxNiches_ShouldNotHaveValidationError()
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
    public void Validate_EmptySearchTerm_ShouldNotHaveValidationError()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(SearchTerm: "");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void Validate_EmptyNichesList_ShouldNotHaveValidationError()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(Niches: new List<string>());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Niches);
    }

    [Fact]
    public void Validate_EmptyPlatformsList_ShouldNotHaveValidationError()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(Platforms: new List<PlatformType>());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Platforms);
    }

    [Theory]
    [InlineData(VlogForge.Application.Discovery.DTOs.AudienceSizeRange.Small)]
    [InlineData(VlogForge.Application.Discovery.DTOs.AudienceSizeRange.Medium)]
    [InlineData(VlogForge.Application.Discovery.DTOs.AudienceSizeRange.Large)]
    public void Validate_AllAudienceSizeRanges_ShouldBeValid(VlogForge.Application.Discovery.DTOs.AudienceSizeRange audienceSize)
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(AudienceSize: audienceSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReportAll()
    {
        // Arrange
        var query = new DiscoverCreatorsQuery(
            PageSize: 0,
            SearchTerm: new string('a', 101),
            Niches: Enumerable.Range(1, 11).Select(i => $"niche{i}").ToList());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
        result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
        result.ShouldHaveValidationErrorFor(x => x.Niches);
    }

    [Fact]
    public void Validate_WithCursor_ShouldNotValidateCursorFormat()
    {
        // Arrange - Cursor format is not validated at this level
        var query = new DiscoverCreatorsQuery(Cursor: "any-string-value");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

}
