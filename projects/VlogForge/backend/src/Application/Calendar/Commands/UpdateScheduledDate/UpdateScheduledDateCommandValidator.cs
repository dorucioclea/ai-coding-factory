using FluentValidation;

namespace VlogForge.Application.Calendar.Commands.UpdateScheduledDate;

/// <summary>
/// Validator for UpdateScheduledDateCommand.
/// Story: ACF-006
/// </summary>
public sealed class UpdateScheduledDateCommandValidator : AbstractValidator<UpdateScheduledDateCommand>
{
    public UpdateScheduledDateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Content item ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.ScheduledDate)
            .Must(date => !date.HasValue || date.Value > DateTime.UtcNow)
            .When(x => x.ScheduledDate.HasValue)
            .WithMessage("Scheduled date must be in the future.");
    }
}
