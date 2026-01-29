using VlogForge.Domain.Entities;

namespace VlogForge.Api.Controllers.SharedProjects.Requests;

/// <summary>
/// Request to add a task to a shared project.
/// Story: ACF-013
/// </summary>
public sealed class AddProjectTaskRequest
{
    /// <summary>The task title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Optional task description.</summary>
    public string? Description { get; init; }

    /// <summary>Optional assignee user ID.</summary>
    public Guid? AssigneeId { get; init; }

    /// <summary>Optional due date.</summary>
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// Request to update a task in a shared project.
/// Story: ACF-013
/// </summary>
public sealed class UpdateProjectTaskRequest
{
    /// <summary>Updated title (optional).</summary>
    public string? Title { get; init; }

    /// <summary>Updated description (optional).</summary>
    public string? Description { get; init; }

    /// <summary>Updated status (optional).</summary>
    public SharedProjectTaskStatus? Status { get; init; }

    /// <summary>Updated assignee (optional).</summary>
    public Guid? AssigneeId { get; init; }

    /// <summary>Updated due date (optional).</summary>
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// Request to add a link to a shared project.
/// Story: ACF-013
/// </summary>
public sealed class AddProjectLinkRequest
{
    /// <summary>The link title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The link URL.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Optional link description.</summary>
    public string? Description { get; init; }
}
