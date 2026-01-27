using FluentAssertions;
using FluentValidation.TestHelper;
using VlogForge.Application.Calendar.Commands.UpdateScheduledDate;
using Xunit;

namespace VlogForge.UnitTests.Application.Calendar;

/// <summary>
/// Unit tests for UpdateScheduledDateCommandValidator.
/// Story: ACF-006
/// </summary>
[Trait("Story", "ACF-006")]
public class UpdateScheduledDateCommandValidatorTests
{
    private readonly UpdateScheduledDateCommandValidator _validator = new();

    [Fact]
    public void ShouldFailWhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateScheduledDateCommand(Guid.Empty, Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Content item ID is required.");
    }

    [Fact]
    public void ShouldFailWhenUserIdIsEmpty()
    {
        // Arrange
        var command = new UpdateScheduledDateCommand(Guid.NewGuid(), Guid.Empty, DateTime.UtcNow.AddDays(1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void ShouldFailWhenScheduledDateIsInThePast()
    {
        // Arrange
        var command = new UpdateScheduledDateCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ScheduledDate)
            .WithErrorMessage("Scheduled date must be in the future.");
    }

    [Fact]
    public void ShouldPassWhenScheduledDateIsInTheFuture()
    {
        // Arrange
        var command = new UpdateScheduledDateCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldPassWhenScheduledDateIsNull()
    {
        // Arrange - Null is valid (clearing the scheduled date)
        var command = new UpdateScheduledDateCommand(Guid.NewGuid(), Guid.NewGuid(), null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
