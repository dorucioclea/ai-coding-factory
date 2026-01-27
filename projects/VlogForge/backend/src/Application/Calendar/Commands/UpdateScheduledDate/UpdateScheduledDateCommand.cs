using MediatR;
using VlogForge.Application.ContentIdeas.DTOs;

namespace VlogForge.Application.Calendar.Commands.UpdateScheduledDate;

/// <summary>
/// Command to update the scheduled date of a content item.
/// Story: ACF-006
/// </summary>
public sealed record UpdateScheduledDateCommand(
    Guid Id,
    Guid UserId,
    DateTime? ScheduledDate
) : IRequest<ContentIdeaResponse>;
