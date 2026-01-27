using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Represents a member of a team with their assigned role.
/// Story: ACF-007
/// </summary>
public sealed class TeamMember : Entity
{
    /// <summary>
    /// Gets the team ID this member belongs to.
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the user ID of the team member.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the member's role in the team.
    /// </summary>
    public TeamRole Role { get; private set; }

    /// <summary>
    /// Gets when the member joined the team.
    /// </summary>
    public DateTime JoinedAt { get; private set; }

    private TeamMember() : base()
    {
    }

    internal TeamMember(Guid teamId, Guid userId, TeamRole role) : base()
    {
        TeamId = teamId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the member's role.
    /// </summary>
    /// <param name="newRole">The new role to assign.</param>
    internal void UpdateRole(TeamRole newRole)
    {
        Role = newRole;
    }
}
