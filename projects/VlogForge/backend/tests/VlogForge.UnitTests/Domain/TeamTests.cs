using FluentAssertions;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for Team aggregate root.
/// Story: ACF-007
/// </summary>
[Trait("Story", "ACF-007")]
public class TeamTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private const string TeamName = "Content Creators";
    private const string TeamDescription = "A team for content creation";

    #region Team Creation

    [Fact]
    public void CreateWithValidDataShouldReturnTeam()
    {
        // Act
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Assert
        team.Should().NotBeNull();
        team.Id.Should().NotBe(Guid.Empty);
        team.OwnerId.Should().Be(_ownerId);
        team.Name.Should().Be(TeamName);
        team.Description.Should().Be(TeamDescription);
        team.Members.Should().HaveCount(1);
        team.Members.First().UserId.Should().Be(_ownerId);
        team.Members.First().Role.Should().Be(TeamRole.Owner);
    }

    [Fact]
    public void CreateWithNullDescriptionShouldSucceed()
    {
        // Act
        var team = Team.Create(_ownerId, TeamName, null);

        // Assert
        team.Should().NotBeNull();
        team.Description.Should().BeNull();
    }

    [Fact]
    public void CreateShouldRaiseTeamCreatedEvent()
    {
        // Act
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Assert
        team.DomainEvents.Should().HaveCount(1);
        var evt = team.DomainEvents.First();
        evt.Should().BeOfType<VlogForge.Domain.Events.TeamCreatedEvent>();
    }

    [Fact]
    public void CreateWithEmptyOwnerIdShouldThrow()
    {
        // Act
        var act = () => Team.Create(Guid.Empty, TeamName, TeamDescription);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Owner ID*");
    }

    [Fact]
    public void CreateWithEmptyNameShouldThrow()
    {
        // Act
        var act = () => Team.Create(_ownerId, "", TeamDescription);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name*empty*");
    }

    [Fact]
    public void CreateWithNameExceedingMaxLengthShouldThrow()
    {
        // Arrange
        var longName = new string('a', Team.MaxNameLength + 1);

        // Act
        var act = () => Team.Create(_ownerId, longName, TeamDescription);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Name*{Team.MaxNameLength}*");
    }

    [Fact]
    public void CreateWithDescriptionExceedingMaxLengthShouldThrow()
    {
        // Arrange
        var longDescription = new string('a', Team.MaxDescriptionLength + 1);

        // Act
        var act = () => Team.Create(_ownerId, TeamName, longDescription);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Description*{Team.MaxDescriptionLength}*");
    }

    #endregion

    #region Team Update

    [Fact]
    public void UpdateWithValidDataShouldUpdateTeam()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var newName = "Updated Team Name";
        var newDescription = "Updated description";

        // Act
        team.Update(newName, newDescription);

        // Assert
        team.Name.Should().Be(newName);
        team.Description.Should().Be(newDescription);
    }

    [Fact]
    public void UpdateShouldIncrementVersion()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var initialVersion = team.Version;

        // Act
        team.Update("New Name", "New Description");

        // Assert
        team.Version.Should().Be(initialVersion + 1);
    }

    #endregion

    #region Member Invitation

    [Fact]
    public void InviteMemberAsOwnerShouldSucceed()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var inviteeEmail = "newmember@example.com";

        // Act
        var token = team.InviteMember(inviteeEmail, TeamRole.Editor, _ownerId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        team.Invitations.Should().HaveCount(1);
        var invitation = team.Invitations.First();
        invitation.Email.Should().Be(inviteeEmail);
        invitation.Role.Should().Be(TeamRole.Editor);
        invitation.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void InviteMemberShouldNormalizeEmail()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var inviteeEmail = "  NewMember@Example.COM  ";

        // Act
        team.InviteMember(inviteeEmail, TeamRole.Editor, _ownerId);

        // Assert
        team.Invitations.First().Email.Should().Be("newmember@example.com");
    }

    [Fact]
    public void InviteMemberAsOwnerRoleShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var act = () => team.InviteMember("test@example.com", TeamRole.Owner, _ownerId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot invite*Owner*");
    }

    [Fact]
    public void InviteMemberWithDuplicateEmailShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var email = "test@example.com";
        team.InviteMember(email, TeamRole.Editor, _ownerId);

        // Act
        var act = () => team.InviteMember(email, TeamRole.Viewer, _ownerId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*active invitation*");
    }

    [Fact]
    public void InviteMemberAsViewerShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var viewerId = Guid.NewGuid();
        var viewerEmail = "viewer@example.com";
        var viewerToken = team.InviteMember(viewerEmail, TeamRole.Viewer, _ownerId);
        team.AcceptInvitation(viewerToken, viewerId, viewerEmail);

        // Act
        var act = () => team.InviteMember("another@example.com", TeamRole.Viewer, viewerId);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*permission*invite*");
    }

    [Fact]
    public void InviteMemberAsAdminShouldSucceed()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var adminId = Guid.NewGuid();
        var adminEmail = "admin@example.com";
        var adminToken = team.InviteMember(adminEmail, TeamRole.Admin, _ownerId);
        team.AcceptInvitation(adminToken, adminId, adminEmail);

        // Act
        var token = team.InviteMember("another@example.com", TeamRole.Editor, adminId);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void InviteMemberWithEmptyEmailShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var act = () => team.InviteMember("", TeamRole.Editor, _ownerId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email*empty*");
    }

    #endregion

    #region Accept Invitation

    [Fact]
    public void AcceptInvitationWithValidTokenShouldAddMember()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var email = "newmember@example.com";
        var token = team.InviteMember(email, TeamRole.Editor, _ownerId);
        var newMemberId = Guid.NewGuid();

        // Act
        team.AcceptInvitation(token, newMemberId, email);

        // Assert
        team.Members.Should().HaveCount(2);
        var newMember = team.Members.First(m => m.UserId == newMemberId);
        newMember.Role.Should().Be(TeamRole.Editor);
        team.Invitations.First().IsAccepted.Should().BeTrue();
        team.Invitations.First().AcceptedByUserId.Should().Be(newMemberId);
    }

    [Fact]
    public void AcceptInvitationShouldRaiseTeamMemberJoinedEvent()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var email = "test@example.com";
        var token = team.InviteMember(email, TeamRole.Editor, _ownerId);
        team.ClearDomainEvents();

        // Act
        team.AcceptInvitation(token, Guid.NewGuid(), email);

        // Assert
        team.DomainEvents.Should().Contain(e =>
            e.GetType().Name == nameof(VlogForge.Domain.Events.TeamMemberJoinedEvent));
    }

    [Fact]
    public void AcceptInvitationWithInvalidTokenShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        team.InviteMember("test@example.com", TeamRole.Editor, _ownerId);

        // Act
        var act = () => team.AcceptInvitation("invalid-token", Guid.NewGuid(), "test@example.com");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invitation not found*");
    }

    [Fact]
    public void AcceptInvitationWithWrongEmailShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var token = team.InviteMember("invited@example.com", TeamRole.Editor, _ownerId);

        // Act - different email trying to accept
        var act = () => team.AcceptInvitation(token, Guid.NewGuid(), "different@example.com");

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*not sent to your email*");
    }

    [Fact]
    public void AcceptInvitationWhenAlreadyMemberShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var email = "member@example.com";
        var token = team.InviteMember(email, TeamRole.Editor, _ownerId);

        // Act - owner trying to accept with the invitation email will pass email check, but fail member check
        var act = () => team.AcceptInvitation(token, _ownerId, email);

        // Assert - Should fail on "already a member" after email check passes
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already a member*");
    }

    [Fact]
    public void AcceptInvitationWithEmptyUserIdShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var email = "test@example.com";
        var token = team.InviteMember(email, TeamRole.Editor, _ownerId);

        // Act
        var act = () => team.AcceptInvitation(token, Guid.Empty, email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*User ID*empty*");
    }

    #endregion

    #region Change Member Role

    [Fact]
    public void ChangeMemberRoleShouldUpdateRole()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var memberId = Guid.NewGuid();
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Viewer, _ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);

        // Act
        team.ChangeMemberRole(memberId, TeamRole.Editor, _ownerId);

        // Assert
        var member = team.GetMember(memberId);
        member.Should().NotBeNull();
        member!.Role.Should().Be(TeamRole.Editor);
    }

    [Fact]
    public void ChangeMemberRoleShouldRaiseEvent()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var memberId = Guid.NewGuid();
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Viewer, _ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);
        team.ClearDomainEvents();

        // Act
        team.ChangeMemberRole(memberId, TeamRole.Editor, _ownerId);

        // Assert
        team.DomainEvents.Should().Contain(e =>
            e.GetType().Name == nameof(VlogForge.Domain.Events.TeamMemberRoleChangedEvent));
    }

    [Fact]
    public void ChangeMemberRoleToOwnerShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var memberId = Guid.NewGuid();
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Admin, _ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);

        // Act
        var act = () => team.ChangeMemberRole(memberId, TeamRole.Owner, _ownerId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot assign Owner*");
    }

    [Fact]
    public void ChangeOwnerRoleShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var act = () => team.ChangeMemberRole(_ownerId, TeamRole.Admin, _ownerId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot change the owner's role*");
    }

    [Fact]
    public void ChangeMemberRoleWithoutPermissionShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var editorId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();

        var editorToken = team.InviteMember("editor@example.com", TeamRole.Editor, _ownerId);
        team.AcceptInvitation(editorToken, editorId, "editor@example.com");

        var viewerToken = team.InviteMember("viewer@example.com", TeamRole.Viewer, _ownerId);
        team.AcceptInvitation(viewerToken, viewerId, "viewer@example.com");

        // Act - editor trying to change role
        var act = () => team.ChangeMemberRole(viewerId, TeamRole.Editor, editorId);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*permission*");
    }

    [Fact]
    public void ChangeMemberRoleForNonExistentMemberShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var act = () => team.ChangeMemberRole(Guid.NewGuid(), TeamRole.Editor, _ownerId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Member not found*");
    }

    #endregion

    #region Remove Member

    [Fact]
    public void RemoveMemberAsOwnerShouldSucceed()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var memberId = Guid.NewGuid();
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Editor, _ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);

        // Act
        team.RemoveMember(memberId, _ownerId);

        // Assert
        team.Members.Should().HaveCount(1);
        team.IsMember(memberId).Should().BeFalse();
    }

    [Fact]
    public void RemoveMemberShouldRaiseEvent()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var memberId = Guid.NewGuid();
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Editor, _ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);
        team.ClearDomainEvents();

        // Act
        team.RemoveMember(memberId, _ownerId);

        // Assert
        team.DomainEvents.Should().Contain(e =>
            e.GetType().Name == nameof(VlogForge.Domain.Events.TeamMemberRemovedEvent));
    }

    [Fact]
    public void RemoveOwnerShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var act = () => team.RemoveMember(_ownerId, _ownerId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot remove the team owner*");
    }

    [Fact]
    public void MemberCanRemoveThemselves()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var memberId = Guid.NewGuid();
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Editor, _ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);

        // Act
        team.RemoveMember(memberId, memberId);

        // Assert
        team.IsMember(memberId).Should().BeFalse();
    }

    [Fact]
    public void RemoveMemberAsNonAdminShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var editorId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();

        var editorToken = team.InviteMember("editor@example.com", TeamRole.Editor, _ownerId);
        team.AcceptInvitation(editorToken, editorId, "editor@example.com");

        var viewerToken = team.InviteMember("viewer@example.com", TeamRole.Viewer, _ownerId);
        team.AcceptInvitation(viewerToken, viewerId, "viewer@example.com");

        // Act - editor trying to remove viewer (not self)
        var act = () => team.RemoveMember(viewerId, editorId);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*permission*remove*");
    }

    #endregion

    #region Transfer Ownership

    [Fact]
    public void TransferOwnershipShouldSucceed()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var newOwnerId = Guid.NewGuid();
        var newOwnerEmail = "newowner@example.com";
        var token = team.InviteMember(newOwnerEmail, TeamRole.Admin, _ownerId);
        team.AcceptInvitation(token, newOwnerId, newOwnerEmail);

        // Act
        team.TransferOwnership(newOwnerId, _ownerId);

        // Assert
        team.OwnerId.Should().Be(newOwnerId);
        team.GetMember(newOwnerId)!.Role.Should().Be(TeamRole.Owner);
        team.GetMember(_ownerId)!.Role.Should().Be(TeamRole.Admin);
    }

    [Fact]
    public void TransferOwnershipByNonOwnerShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var adminId = Guid.NewGuid();
        var adminEmail = "admin@example.com";
        var token = team.InviteMember(adminEmail, TeamRole.Admin, _ownerId);
        team.AcceptInvitation(token, adminId, adminEmail);

        // Act
        var act = () => team.TransferOwnership(adminId, adminId);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*Only the current owner*");
    }

    [Fact]
    public void TransferOwnershipToNonMemberShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var act = () => team.TransferOwnership(Guid.NewGuid(), _ownerId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be an existing member*");
    }

    #endregion

    #region Permissions

    [Theory]
    [InlineData(TeamRole.Owner, TeamAccessRight.ViewContent, true)]
    [InlineData(TeamRole.Owner, TeamAccessRight.EditContent, true)]
    [InlineData(TeamRole.Owner, TeamAccessRight.AssignTasks, true)]
    [InlineData(TeamRole.Owner, TeamAccessRight.InviteMembers, true)]
    [InlineData(TeamRole.Owner, TeamAccessRight.ManageTeamSettings, true)]
    [InlineData(TeamRole.Admin, TeamAccessRight.ViewContent, true)]
    [InlineData(TeamRole.Admin, TeamAccessRight.EditContent, true)]
    [InlineData(TeamRole.Admin, TeamAccessRight.AssignTasks, true)]
    [InlineData(TeamRole.Admin, TeamAccessRight.InviteMembers, true)]
    [InlineData(TeamRole.Admin, TeamAccessRight.ManageTeamSettings, true)]
    [InlineData(TeamRole.Editor, TeamAccessRight.ViewContent, true)]
    [InlineData(TeamRole.Editor, TeamAccessRight.EditContent, true)]
    [InlineData(TeamRole.Editor, TeamAccessRight.AssignTasks, true)]
    [InlineData(TeamRole.Editor, TeamAccessRight.InviteMembers, false)]
    [InlineData(TeamRole.Editor, TeamAccessRight.ManageTeamSettings, false)]
    [InlineData(TeamRole.Viewer, TeamAccessRight.ViewContent, true)]
    [InlineData(TeamRole.Viewer, TeamAccessRight.EditContent, false)]
    [InlineData(TeamRole.Viewer, TeamAccessRight.AssignTasks, false)]
    [InlineData(TeamRole.Viewer, TeamAccessRight.InviteMembers, false)]
    [InlineData(TeamRole.Viewer, TeamAccessRight.ManageTeamSettings, false)]
    public void HasPermissionShouldReturnCorrectResult(TeamRole role, TeamAccessRight permission, bool expected)
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);
        var memberId = Guid.NewGuid();

        if (role != TeamRole.Owner)
        {
            var email = $"member{role}@example.com";
            var token = team.InviteMember(email, role, _ownerId);
            team.AcceptInvitation(token, memberId, email);
        }
        else
        {
            memberId = _ownerId;
        }

        // Act
        var result = team.HasPermission(memberId, permission);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void HasPermissionForNonMemberShouldReturnFalse()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var result = team.HasPermission(Guid.NewGuid(), TeamAccessRight.ViewContent);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsMember

    [Fact]
    public void IsMemberShouldReturnTrueForMember()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var result = team.IsMember(_ownerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMemberShouldReturnFalseForNonMember()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName, TeamDescription);

        // Act
        var result = team.IsMember(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
