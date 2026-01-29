using VlogForge.Domain.Entities;

namespace VlogForge.Application.SharedProjects.DTOs;

/// <summary>
/// Response DTO for a shared project.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public SharedProjectStatus Status { get; init; }
    public Guid CollaborationRequestId { get; init; }
    public Guid OwnerId { get; init; }
    public int MemberCount { get; init; }
    public int TaskCount { get; init; }
    public int LinkCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public IReadOnlyList<SharedProjectMemberResponse> Members { get; init; } = [];

    public static SharedProjectResponse FromEntity(SharedProject project)
    {
        return new SharedProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            CollaborationRequestId = project.CollaborationRequestId,
            OwnerId = project.OwnerId,
            MemberCount = project.Members.Count,
            TaskCount = project.Tasks.Count,
            LinkCount = project.Links.Count,
            CreatedAt = project.CreatedAt,
            ClosedAt = project.ClosedAt,
            Members = project.Members.Select(SharedProjectMemberResponse.FromEntity).ToList(),
        };
    }
}

/// <summary>
/// Response DTO for a shared project with full details.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectDetailResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public SharedProjectStatus Status { get; init; }
    public Guid CollaborationRequestId { get; init; }
    public Guid OwnerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public IReadOnlyList<SharedProjectMemberResponse> Members { get; init; } = [];
    public IReadOnlyList<SharedProjectTaskResponse> Tasks { get; init; } = [];
    public IReadOnlyList<SharedProjectLinkResponse> Links { get; init; } = [];

    public static SharedProjectDetailResponse FromEntity(SharedProject project)
    {
        return new SharedProjectDetailResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            CollaborationRequestId = project.CollaborationRequestId,
            OwnerId = project.OwnerId,
            CreatedAt = project.CreatedAt,
            ClosedAt = project.ClosedAt,
            Members = project.Members.Select(SharedProjectMemberResponse.FromEntity).ToList(),
            Tasks = project.Tasks.Select(SharedProjectTaskResponse.FromEntity).ToList(),
            Links = project.Links.Select(SharedProjectLinkResponse.FromEntity).ToList(),
        };
    }
}

/// <summary>
/// Response DTO for a shared project member.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectMemberResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public SharedProjectRole Role { get; init; }
    public DateTime JoinedAt { get; init; }

    public static SharedProjectMemberResponse FromEntity(SharedProjectMember member)
    {
        return new SharedProjectMemberResponse
        {
            Id = member.Id,
            UserId = member.UserId,
            Role = member.Role,
            JoinedAt = member.JoinedAt,
        };
    }
}

/// <summary>
/// Response DTO for a shared project task.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectTaskResponse
{
    public Guid Id { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public SharedProjectTaskStatus Status { get; init; }
    public Guid? AssigneeId { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }

    public static SharedProjectTaskResponse FromEntity(SharedProjectTask task)
    {
        return new SharedProjectTaskResponse
        {
            Id = task.Id,
            CreatedByUserId = task.CreatedByUserId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            AssigneeId = task.AssigneeId,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            CreatedAt = task.CreatedAt,
        };
    }
}

/// <summary>
/// Response DTO for a shared project link.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectLinkResponse
{
    public Guid Id { get; init; }
    public Guid AddedByUserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }

    public static SharedProjectLinkResponse FromEntity(SharedProjectLink link)
    {
        return new SharedProjectLinkResponse
        {
            Id = link.Id,
            AddedByUserId = link.AddedByUserId,
            Title = link.Title,
            Url = link.Url,
            Description = link.Description,
            CreatedAt = link.CreatedAt,
        };
    }
}

/// <summary>
/// Response DTO for a shared project activity.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectActivityResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public SharedProjectActivityType ActivityType { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public static SharedProjectActivityResponse FromEntity(SharedProjectActivity activity)
    {
        return new SharedProjectActivityResponse
        {
            Id = activity.Id,
            UserId = activity.UserId,
            ActivityType = activity.ActivityType,
            Message = activity.Message,
            CreatedAt = activity.CreatedAt,
        };
    }
}

/// <summary>
/// Paginated list of shared projects.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectListResponse
{
    public IReadOnlyList<SharedProjectResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Paginated list of shared project activities.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectActivityListResponse
{
    public IReadOnlyList<SharedProjectActivityResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
