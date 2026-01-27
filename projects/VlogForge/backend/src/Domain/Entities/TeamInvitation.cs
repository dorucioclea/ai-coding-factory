using VlogForge.Domain.Common;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Represents a pending invitation to join a team.
/// Story: ACF-007
/// </summary>
public sealed class TeamInvitation : Entity
{
    /// <summary>
    /// Gets the team ID this invitation is for.
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the email address of the invitee.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Gets the role to be assigned upon acceptance.
    /// </summary>
    public TeamRole Role { get; private set; }

    /// <summary>
    /// Gets the invitation token.
    /// </summary>
    public string Token { get; private set; }

    /// <summary>
    /// Gets when the invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the user ID of the person who sent the invitation.
    /// </summary>
    public Guid InvitedByUserId { get; private set; }

    /// <summary>
    /// Gets when the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; private set; }

    /// <summary>
    /// Gets the user ID who accepted the invitation.
    /// </summary>
    public Guid? AcceptedByUserId { get; private set; }

    /// <summary>
    /// Gets whether the invitation has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Gets whether the invitation has been accepted.
    /// </summary>
    public bool IsAccepted => AcceptedAt.HasValue;

    private TeamInvitation() : base()
    {
        Email = string.Empty;
        Token = string.Empty;
    }

    internal TeamInvitation(
        Guid teamId,
        string email,
        TeamRole role,
        string token,
        DateTime expiresAt,
        Guid invitedByUserId) : base()
    {
        TeamId = teamId;
        Email = email;
        Role = role;
        Token = token;
        ExpiresAt = expiresAt;
        InvitedByUserId = invitedByUserId;
    }

    /// <summary>
    /// Marks the invitation as accepted.
    /// </summary>
    /// <param name="acceptedByUserId">The user ID accepting the invitation.</param>
    internal void Accept(Guid acceptedByUserId)
    {
        AcceptedAt = DateTime.UtcNow;
        AcceptedByUserId = acceptedByUserId;
    }
}
