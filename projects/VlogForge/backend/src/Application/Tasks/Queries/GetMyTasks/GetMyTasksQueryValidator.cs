using FluentValidation;

namespace VlogForge.Application.Tasks.Queries.GetMyTasks;

/// <summary>
/// Validator for GetMyTasksQuery.
/// Stories: ACF-008, ACF-014
/// </summary>
public sealed class GetMyTasksQueryValidator : AbstractValidator<GetMyTasksQuery>
{
    private static readonly string[] ValidSortFields = { "dueDate", "createdAt", "status" };
    private static readonly string[] ValidSortDirections = { "asc", "desc" };

    public GetMyTasksQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid assignment status.");

        RuleFor(x => x.SortBy)
            .Must(x => ValidSortFields.Contains(x))
            .WithMessage($"Sort field must be one of: {string.Join(", ", ValidSortFields)}.");

        RuleFor(x => x.SortDirection)
            .Must(x => ValidSortDirections.Contains(x))
            .WithMessage("Sort direction must be 'asc' or 'desc'.");
    }
}
