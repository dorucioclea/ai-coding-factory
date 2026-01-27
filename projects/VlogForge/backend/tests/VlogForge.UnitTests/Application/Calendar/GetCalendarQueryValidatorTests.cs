using FluentAssertions;
using FluentValidation.TestHelper;
using VlogForge.Application.Calendar.Queries.GetCalendar;
using Xunit;

namespace VlogForge.UnitTests.Application.Calendar;

/// <summary>
/// Unit tests for GetCalendarQueryValidator.
/// Story: ACF-006
/// </summary>
[Trait("Story", "ACF-006")]
public class GetCalendarQueryValidatorTests
{
    private readonly GetCalendarQueryValidator _validator = new();

    [Fact]
    public void ShouldFailWhenUserIdIsEmpty()
    {
        // Arrange
        var query = new GetCalendarQuery(Guid.Empty, 2026, 1);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Theory]
    [InlineData(2019)]
    [InlineData(2101)]
    public void ShouldFailWhenYearIsOutOfRange(int year)
    {
        // Arrange
        var query = new GetCalendarQuery(Guid.NewGuid(), year, 1);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Year)
            .WithErrorMessage("Year must be between 2020 and 2100.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void ShouldFailWhenMonthIsOutOfRange(int month)
    {
        // Arrange
        var query = new GetCalendarQuery(Guid.NewGuid(), 2026, month);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Month)
            .WithErrorMessage("Month must be between 1 and 12.");
    }

    [Theory]
    [InlineData(2020, 1)]
    [InlineData(2026, 6)]
    [InlineData(2100, 12)]
    public void ShouldPassWithValidInput(int year, int month)
    {
        // Arrange
        var query = new GetCalendarQuery(Guid.NewGuid(), year, month);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
