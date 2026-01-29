using FluentAssertions;
using FluentValidation.TestHelper;
using VlogForge.Application.Tasks.Queries.GetMyTasks;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Tasks;

/// <summary>
/// Validator tests for GetMyTasksQuery with new ACF-014 parameters.
/// Story: ACF-014
/// </summary>
[Trait("Story", "ACF-014")]
public class GetMyTasksQueryValidatorTests
{
    private readonly GetMyTasksQueryValidator _validator = new();

    [Fact]
    public void ValidQueryShouldPassValidation()
    {
        var query = new GetMyTasksQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyUserIdShouldFailValidation()
    {
        var query = new GetMyTasksQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidPageShouldFailValidation(int page)
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), Page: page);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void InvalidPageSizeShouldFailValidation(int pageSize)
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), PageSize: pageSize);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(AssignmentStatus.NotStarted)]
    [InlineData(AssignmentStatus.InProgress)]
    [InlineData(AssignmentStatus.Completed)]
    public void ValidStatusFilterShouldPassValidation(AssignmentStatus status)
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), Status: status);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void InvalidStatusFilterShouldFailValidation()
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), Status: (AssignmentStatus)99);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void NullStatusFilterShouldPassValidation()
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), Status: null);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData("dueDate")]
    [InlineData("createdAt")]
    [InlineData("status")]
    public void ValidSortByShouldPassValidation(string sortBy)
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), SortBy: sortBy);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void InvalidSortByShouldFailValidation()
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), SortBy: "invalid");

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    public void ValidSortDirectionShouldPassValidation(string sortDirection)
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), SortDirection: sortDirection);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.SortDirection);
    }

    [Fact]
    public void InvalidSortDirectionShouldFailValidation()
    {
        var query = new GetMyTasksQuery(Guid.NewGuid(), SortDirection: "invalid");

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.SortDirection);
    }
}
