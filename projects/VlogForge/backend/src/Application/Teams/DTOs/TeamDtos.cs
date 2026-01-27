using VlogForge.Domain.Entities;

namespace VlogForge.Application.Teams.DTOs;

/// <summary>
/// Response DTO for team information.
/// Story: ACF-007
/// </summary>
public sealed class TeamResponse
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MemberCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static TeamResponse FromEntity(Team team)
    {
        return new TeamResponse
        {
            Id = team.Id,
            OwnerId = team.OwnerId,
            Name = team.Name,
            Description = team.Description,
            MemberCount = team.Members.Count,
            CreatedAt = team.CreatedAt
        };
    }
}

/// <summary>
/// Response DTO for team with members.
/// Story: ACF-007
/// </summary>
public sealed class TeamWithMembersResponse
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<TeamMemberResponse> Members { get; init; } = new List<TeamMemberResponse>();

    public static TeamWithMembersResponse FromEntity(Team team)
    {
        return new TeamWithMembersResponse
        {
            Id = team.Id,
            OwnerId = team.OwnerId,
            Name = team.Name,
            Description = team.Description,
            CreatedAt = team.CreatedAt,
            Members = team.Members.Select(TeamMemberResponse.FromEntity).ToList()
        };
    }
}

/// <summary>
/// Response DTO for team member information.
/// Story: ACF-007
/// </summary>
public sealed class TeamMemberResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public TeamRole Role { get; init; }
    public DateTime JoinedAt { get; init; }

    public static TeamMemberResponse FromEntity(TeamMember member)
    {
        return new TeamMemberResponse
        {
            Id = member.Id,
            UserId = member.UserId,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }
}

/// <summary>
/// Response DTO for team invitation.
/// Note: Token is intentionally NOT included for security - it's only sent via email.
/// Story: ACF-007
/// </summary>
public sealed class TeamInvitationResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public TeamRole Role { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsExpired { get; init; }
    public bool IsAccepted { get; init; }

    public static TeamInvitationResponse FromEntity(TeamInvitation invitation)
    {
        return new TeamInvitationResponse
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role,
            ExpiresAt = invitation.ExpiresAt,
            IsExpired = invitation.IsExpired,
            IsAccepted = invitation.IsAccepted
        };
    }
}

/// <summary>
/// Response for team list with pagination.
/// Story: ACF-007
/// </summary>
public sealed class TeamListResponse
{
    /// <summary>
    /// The list of teams for the current page.
    /// </summary>
    public IReadOnlyList<TeamResponse> Items { get; init; } = new List<TeamResponse>();

    /// <summary>
    /// Total number of teams across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
