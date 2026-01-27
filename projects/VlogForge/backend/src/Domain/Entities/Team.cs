using System.Security.Cryptography;
using VlogForge.Domain.Common;
using VlogForge.Domain.Events;

namespace VlogForge.Domain.Entities;

/// <summary>
/// Team role hierarchy for permission checks.
/// Story: ACF-007
/// </summary>
public enum TeamRole
{
    /// <summary>View-only access to team resources.</summary>
    Viewer = 0,

    /// <summary>Can edit content and assign tasks.</summary>
    Editor = 1,

    /// <summary>Can manage team settings and members.</summary>
    Admin = 2,

    /// <summary>Full control over the team.</summary>
    Owner = 3
}

/// <summary>
/// Team aggregate root for managing creator teams.
/// Story: ACF-007
/// </summary>
public sealed class Team : AggregateRoot
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;
    public const int MaxMembersPerTeam = 50;
    public const int InvitationExpirationDays = 7;

    private readonly List<TeamMember> _members = new();
    private readonly List<TeamInvitation> _invitations = new();
    private readonly List<Guid> _approverIds = new();

    /// <summary>
    /// Gets the user ID of the team owner.
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Gets the team name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the team description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the team members.
    /// </summary>
    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    /// <summary>
    /// Gets the pending team invitations.
    /// </summary>
    public IReadOnlyCollection<TeamInvitation> Invitations => _invitations.AsReadOnly();

    /// <summary>
    /// Gets whether this team requires approval for content before scheduling.
    /// Story: ACF-009
    /// </summary>
    public bool RequiresApproval { get; private set; }

    /// <summary>
    /// Gets the user IDs of members who can approve content.
    /// If empty and RequiresApproval is true, only Admins and Owner can approve.
    /// Story: ACF-009
    /// </summary>
    public IReadOnlyCollection<Guid> ApproverIds => _approverIds.AsReadOnly();

    private Team() : base()
    {
        Name = string.Empty;
    }

    private Team(Guid ownerId, string name, string? description) : base()
    {
        OwnerId = ownerId;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Creates a new team with the specified owner.
    /// </summary>
    /// <param name="ownerId">The owner's user ID.</param>
    /// <param name="name">The team name.</param>
    /// <param name="description">Optional team description.</param>
    /// <returns>A new Team instance.</returns>
    public static Team Create(Guid ownerId, string name, string? description = null)
    {
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty.", nameof(ownerId));

        ValidateName(name);
        ValidateDescription(description);

        var team = new Team(ownerId, name.Trim(), description?.Trim());

        // Add owner as first member
        var ownerMember = new TeamMember(team.Id, ownerId, TeamRole.Owner);
        team._members.Add(ownerMember);

        team.RaiseDomainEvent(new TeamCreatedEvent(team.Id, ownerId, team.Name));

        return team;
    }

    /// <summary>
    /// Updates the team name and description.
    /// </summary>
    /// <param name="name">The new team name.</param>
    /// <param name="description">The new team description.</param>
    public void Update(string name, string? description)
    {
        ValidateName(name);
        ValidateDescription(description);

        Name = name.Trim();
        Description = description?.Trim();
        IncrementVersion();

        RaiseDomainEvent(new TeamUpdatedEvent(Id, OwnerId));
    }

    /// <summary>
    /// Creates an invitation for a user to join the team.
    /// </summary>
    /// <param name="email">The invitee's email address.</param>
    /// <param name="role">The role to assign upon acceptance.</param>
    /// <param name="invitedByUserId">The user ID of the person inviting.</param>
    /// <returns>The invitation token.</returns>
    public string InviteMember(string email, TeamRole role, Guid invitedByUserId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (role == TeamRole.Owner)
            throw new InvalidOperationException("Cannot invite someone as Owner. Use TransferOwnership instead.");

        if (!HasPermission(invitedByUserId, TeamAccessRight.InviteMembers))
            throw new UnauthorizedAccessException("You do not have permission to invite members.");

        if (_members.Count >= MaxMembersPerTeam)
            throw new InvalidOperationException($"Team cannot have more than {MaxMembersPerTeam} members.");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // Check if invitation already exists
        var existingInvitation = _invitations.FirstOrDefault(i =>
            i.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            !i.IsExpired &&
            i.AcceptedAt is null);

        if (existingInvitation is not null)
            throw new InvalidOperationException("An active invitation already exists for this email.");

        var token = GenerateInvitationToken();
        var expiresAt = DateTime.UtcNow.AddDays(InvitationExpirationDays);
        var invitation = new TeamInvitation(Id, normalizedEmail, role, token, expiresAt, invitedByUserId);

        _invitations.Add(invitation);
        IncrementVersion();

        RaiseDomainEvent(new TeamMemberInvitedEvent(Id, invitation.Id, role, invitedByUserId));

        return token;
    }

    /// <summary>
    /// Accepts an invitation and adds the user to the team.
    /// </summary>
    /// <param name="token">The invitation token.</param>
    /// <param name="userId">The user ID accepting the invitation.</param>
    /// <param name="userEmail">The email of the user accepting (for verification).</param>
    public void AcceptInvitation(string token, Guid userId, string userEmail)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(userEmail))
            throw new ArgumentException("User email cannot be empty.", nameof(userEmail));

        // Use constant-time comparison to prevent timing attacks
        var invitation = _invitations.FirstOrDefault(i =>
            i.AcceptedAt is null &&
            ConstantTimeEquals(i.Token, token));

        if (invitation is null)
            throw new InvalidOperationException("Invitation not found.");

        if (invitation.IsExpired)
            throw new InvalidOperationException("Invitation has expired.");

        // Verify the invitation was sent to this user's email
        if (!invitation.Email.Equals(userEmail.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("This invitation was not sent to your email address.");

        // Check if user is already a member
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User is already a member of this team.");

        if (_members.Count >= MaxMembersPerTeam)
            throw new InvalidOperationException($"Team cannot have more than {MaxMembersPerTeam} members.");

        invitation.Accept(userId);
        var member = new TeamMember(Id, userId, invitation.Role);
        _members.Add(member);
        IncrementVersion();

        RaiseDomainEvent(new TeamMemberJoinedEvent(Id, userId, invitation.Role));
    }

    /// <summary>
    /// Changes a member's role.
    /// </summary>
    /// <param name="userId">The member's user ID.</param>
    /// <param name="newRole">The new role to assign.</param>
    /// <param name="changedByUserId">The user ID making the change.</param>
    public void ChangeMemberRole(Guid userId, TeamRole newRole, Guid changedByUserId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (!HasPermission(changedByUserId, TeamAccessRight.ManageTeamSettings))
            throw new UnauthorizedAccessException("You do not have permission to change member roles.");

        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            throw new InvalidOperationException("Member not found.");

        if (member.Role == TeamRole.Owner)
            throw new InvalidOperationException("Cannot change the owner's role. Use TransferOwnership instead.");

        if (newRole == TeamRole.Owner)
            throw new InvalidOperationException("Cannot assign Owner role. Use TransferOwnership instead.");

        var oldRole = member.Role;
        if (oldRole == newRole)
            return;

        member.UpdateRole(newRole);
        IncrementVersion();

        RaiseDomainEvent(new TeamMemberRoleChangedEvent(Id, userId, oldRole, newRole, changedByUserId));
    }

    /// <summary>
    /// Removes a member from the team.
    /// </summary>
    /// <param name="userId">The member's user ID.</param>
    /// <param name="removedByUserId">The user ID removing the member.</param>
    public void RemoveMember(Guid userId, Guid removedByUserId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            throw new InvalidOperationException("Member not found.");

        if (member.Role == TeamRole.Owner)
            throw new InvalidOperationException("Cannot remove the team owner.");

        // Members can remove themselves
        if (userId != removedByUserId && !HasPermission(removedByUserId, TeamAccessRight.InviteMembers))
            throw new UnauthorizedAccessException("You do not have permission to remove members.");

        _members.Remove(member);
        IncrementVersion();

        RaiseDomainEvent(new TeamMemberRemovedEvent(Id, userId, removedByUserId));
    }

    /// <summary>
    /// Transfers team ownership to another member.
    /// </summary>
    /// <param name="newOwnerId">The user ID of the new owner.</param>
    /// <param name="currentOwnerId">The current owner's user ID.</param>
    public void TransferOwnership(Guid newOwnerId, Guid currentOwnerId)
    {
        if (newOwnerId == Guid.Empty)
            throw new ArgumentException("New owner ID cannot be empty.", nameof(newOwnerId));

        if (currentOwnerId != OwnerId)
            throw new UnauthorizedAccessException("Only the current owner can transfer ownership.");

        if (newOwnerId == OwnerId)
            return;

        var newOwnerMember = _members.FirstOrDefault(m => m.UserId == newOwnerId);
        if (newOwnerMember is null)
            throw new InvalidOperationException("New owner must be an existing member.");

        var currentOwnerMember = _members.FirstOrDefault(m => m.UserId == OwnerId);

        // Update roles
        newOwnerMember.UpdateRole(TeamRole.Owner);
        currentOwnerMember?.UpdateRole(TeamRole.Admin);

        var oldOwnerId = OwnerId;
        OwnerId = newOwnerId;
        IncrementVersion();

        RaiseDomainEvent(new TeamOwnershipTransferredEvent(Id, oldOwnerId, newOwnerId));
    }

    /// <summary>
    /// Configures the approval workflow settings.
    /// Story: ACF-009
    /// </summary>
    /// <param name="requiresApproval">Whether approval is required before scheduling.</param>
    /// <param name="approverIds">User IDs of designated approvers. If empty, Admins and Owner can approve.</param>
    /// <param name="configuredByUserId">The user ID making the configuration change.</param>
    public void ConfigureWorkflow(bool requiresApproval, IEnumerable<Guid>? approverIds, Guid configuredByUserId)
    {
        if (!HasPermission(configuredByUserId, TeamAccessRight.ManageTeamSettings))
            throw new UnauthorizedAccessException("You do not have permission to configure workflow settings.");

        RequiresApproval = requiresApproval;

        _approverIds.Clear();

        if (approverIds is not null)
        {
            foreach (var approverId in approverIds.Distinct())
            {
                if (approverId == Guid.Empty)
                    continue;

                // Verify approver is a member
                if (!IsMember(approverId))
                    throw new InvalidOperationException($"Approver must be a team member.");

                _approverIds.Add(approverId);
            }
        }

        IncrementVersion();

        RaiseDomainEvent(new TeamWorkflowConfiguredEvent(
            Id,
            configuredByUserId,
            RequiresApproval,
            _approverIds.ToList()));
    }

    /// <summary>
    /// Checks if a user can approve content for this team.
    /// Story: ACF-009
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user can approve content.</returns>
    public bool CanApproveContent(Guid userId)
    {
        if (!RequiresApproval)
            return false;

        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return false;

        // If specific approvers are designated, only they can approve
        if (_approverIds.Count > 0)
            return _approverIds.Contains(userId);

        // Otherwise, Admins and Owner can approve
        return member.Role >= TeamRole.Admin;
    }

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the permission.</returns>
    public bool HasPermission(Guid userId, TeamAccessRight permission)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return false;

        return permission switch
        {
            TeamAccessRight.ViewContent => true, // All members can view
            TeamAccessRight.EditContent => member.Role >= TeamRole.Editor,
            TeamAccessRight.AssignTasks => member.Role >= TeamRole.Editor,
            TeamAccessRight.InviteMembers => member.Role >= TeamRole.Admin,
            TeamAccessRight.ManageTeamSettings => member.Role >= TeamRole.Admin,
            TeamAccessRight.ApproveContent => CanApproveContent(userId), // ACF-009
            _ => false
        };
    }

    /// <summary>
    /// Gets a member by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The team member or null.</returns>
    public TeamMember? GetMember(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId);
    }

    /// <summary>
    /// Checks if a user is a member of this team.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user is a member.</returns>
    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    private const int InvitationTokenSizeBytes = 32;

    private static string GenerateInvitationToken()
    {
        var tokenBytes = new byte[InvitationTokenSizeBytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    /// <summary>
    /// Performs constant-time string comparison to prevent timing attacks.
    /// </summary>
    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a is null || b is null)
            return a == b;

        var aBytes = System.Text.Encoding.UTF8.GetBytes(a);
        var bBytes = System.Text.Encoding.UTF8.GetBytes(b);

        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (name.Trim().Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters.", nameof(name));
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
    }
}

/// <summary>
/// Team permissions for authorization checks.
/// Story: ACF-007, ACF-009
/// </summary>
public enum TeamAccessRight
{
    /// <summary>Can view team content.</summary>
    ViewContent,

    /// <summary>Can edit team content.</summary>
    EditContent,

    /// <summary>Can assign tasks to members.</summary>
    AssignTasks,

    /// <summary>Can invite and remove members.</summary>
    InviteMembers,

    /// <summary>Can manage team settings.</summary>
    ManageTeamSettings,

    /// <summary>Can approve or request changes on content. Story: ACF-009</summary>
    ApproveContent
}
