using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Represents a member of a shared project.
/// Story: ACF-013
/// </summary>
public sealed class SharedProjectMember : Entity
{
    /// <summary>
    /// Gets the shared project ID.
    /// </summary>
    public Guid SharedProjectId { get; private set; }

    /// <summary>
    /// Gets the user ID of the member.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the member's role in the project.
    /// </summary>
    public SharedProjectRole Role { get; private set; }

    /// <summary>
    /// Gets when the member joined the project.
    /// </summary>
    public DateTime JoinedAt { get; private set; }

    private SharedProjectMember() : base()
    {
    }

    internal SharedProjectMember(Guid sharedProjectId, Guid userId, SharedProjectRole role) : base()
    {
        SharedProjectId = sharedProjectId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Promotes this member to owner.
    /// </summary>
    public void PromoteToOwner()
    {
        Role = SharedProjectRole.Owner;
    }
}
