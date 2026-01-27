using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for Team approval workflow configuration.
/// Story: ACF-009
/// </summary>
[Trait("Story", "ACF-009")]
public class TeamApprovalWorkflowTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private const string TeamName = "Content Creators";

    #region ConfigureWorkflow Tests

    [Fact]
    public void ConfigureWorkflowShouldEnableApproval()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        team.ClearDomainEvents();

        // Act
        team.ConfigureWorkflow(true, null, _ownerId);

        // Assert
        team.RequiresApproval.Should().BeTrue();
        team.ApproverIds.Should().BeEmpty();
    }

    [Fact]
    public void ConfigureWorkflowShouldDisableApproval()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        team.ConfigureWorkflow(true, null, _ownerId);
        team.ClearDomainEvents();

        // Act
        team.ConfigureWorkflow(false, null, _ownerId);

        // Assert
        team.RequiresApproval.Should().BeFalse();
    }

    [Fact]
    public void ConfigureWorkflowWithApproversShouldSetApproverIds()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var adminId = AddMemberToTeam(team, TeamRole.Admin);
        var editorId = AddMemberToTeam(team, TeamRole.Editor);
        var approverIds = new[] { adminId, editorId };
        team.ClearDomainEvents();

        // Act
        team.ConfigureWorkflow(true, approverIds, _ownerId);

        // Assert
        team.RequiresApproval.Should().BeTrue();
        team.ApproverIds.Should().HaveCount(2);
        team.ApproverIds.Should().Contain(adminId);
        team.ApproverIds.Should().Contain(editorId);
    }

    [Fact]
    public void ConfigureWorkflowShouldRaiseTeamWorkflowConfiguredEvent()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        team.ClearDomainEvents();

        // Act
        team.ConfigureWorkflow(true, null, _ownerId);

        // Assert
        team.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TeamWorkflowConfiguredEvent>();
        var evt = (TeamWorkflowConfiguredEvent)team.DomainEvents.First();
        evt.RequiresApproval.Should().BeTrue();
    }

    [Fact]
    public void ConfigureWorkflowShouldRemovePreviousApprovers()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var adminId = AddMemberToTeam(team, TeamRole.Admin);
        team.ConfigureWorkflow(true, new[] { adminId }, _ownerId);

        // Act
        team.ConfigureWorkflow(true, null, _ownerId);

        // Assert
        team.ApproverIds.Should().BeEmpty();
    }

    [Fact]
    public void ConfigureWorkflowWithNonMemberApproverShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var nonMemberId = Guid.NewGuid();

        // Act
        var act = () => team.ConfigureWorkflow(true, new[] { nonMemberId }, _ownerId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Approver must be a team member*");
    }

    [Fact]
    public void ConfigureWorkflowWithoutPermissionShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var viewerId = AddMemberToTeam(team, TeamRole.Viewer);

        // Act
        var act = () => team.ConfigureWorkflow(true, null, viewerId);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*permission*configure*");
    }

    [Fact]
    public void ConfigureWorkflowAsAdminShouldSucceed()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var adminId = AddMemberToTeam(team, TeamRole.Admin);

        // Act
        team.ConfigureWorkflow(true, null, adminId);

        // Assert
        team.RequiresApproval.Should().BeTrue();
    }

    [Fact]
    public void ConfigureWorkflowShouldFilterDuplicateApproverIds()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var adminId = AddMemberToTeam(team, TeamRole.Admin);
        var duplicateIds = new[] { adminId, adminId, adminId };

        // Act
        team.ConfigureWorkflow(true, duplicateIds, _ownerId);

        // Assert
        team.ApproverIds.Should().ContainSingle()
            .Which.Should().Be(adminId);
    }

    [Fact]
    public void ConfigureWorkflowShouldIgnoreEmptyGuids()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var adminId = AddMemberToTeam(team, TeamRole.Admin);
        var idsWithEmpty = new[] { adminId, Guid.Empty, Guid.Empty };

        // Act
        team.ConfigureWorkflow(true, idsWithEmpty, _ownerId);

        // Assert
        team.ApproverIds.Should().ContainSingle()
            .Which.Should().Be(adminId);
    }

    #endregion

    #region CanApproveContent Tests

    [Fact]
    public void CanApproveContentWhenApprovalNotRequiredShouldReturnFalse()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);

        // Act
        var result = team.CanApproveContent(_ownerId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanApproveContentAsOwnerWhenNoDesignatedApproversShouldReturnTrue()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        team.ConfigureWorkflow(true, null, _ownerId);

        // Act
        var result = team.CanApproveContent(_ownerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanApproveContentAsAdminWhenNoDesignatedApproversShouldReturnTrue()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var adminId = AddMemberToTeam(team, TeamRole.Admin);
        team.ConfigureWorkflow(true, null, _ownerId);

        // Act
        var result = team.CanApproveContent(adminId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanApproveContentAsEditorWhenNoDesignatedApproversShouldReturnFalse()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var editorId = AddMemberToTeam(team, TeamRole.Editor);
        team.ConfigureWorkflow(true, null, _ownerId);

        // Act
        var result = team.CanApproveContent(editorId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanApproveContentAsDesignatedApproverShouldReturnTrue()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var editorId = AddMemberToTeam(team, TeamRole.Editor);
        team.ConfigureWorkflow(true, new[] { editorId }, _ownerId);

        // Act
        var result = team.CanApproveContent(editorId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanApproveContentAsNonDesignatedApproverShouldReturnFalse()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var designatedId = AddMemberToTeam(team, TeamRole.Editor);
        var otherId = AddMemberToTeam(team, TeamRole.Admin);
        team.ConfigureWorkflow(true, new[] { designatedId }, _ownerId);

        // Act
        var result = team.CanApproveContent(otherId);

        // Assert
        result.Should().BeFalse(); // Admin is not in designated list
    }

    [Fact]
    public void CanApproveContentAsNonMemberShouldReturnFalse()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        team.ConfigureWorkflow(true, null, _ownerId);

        // Act
        var result = team.CanApproveContent(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasPermission ApproveContent Tests

    [Fact]
    public void HasPermissionApproveContentWhenCanApproveShouldReturnTrue()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        team.ConfigureWorkflow(true, null, _ownerId);

        // Act
        var result = team.HasPermission(_ownerId, TeamAccessRight.ApproveContent);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermissionApproveContentWhenCannotApproveShouldReturnFalse()
    {
        // Arrange
        var team = Team.Create(_ownerId, TeamName);
        var viewerId = AddMemberToTeam(team, TeamRole.Viewer);
        team.ConfigureWorkflow(true, null, _ownerId);

        // Act
        var result = team.HasPermission(viewerId, TeamAccessRight.ApproveContent);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private Guid AddMemberToTeam(Team team, TeamRole role)
    {
        var memberId = Guid.NewGuid();
        var email = $"member_{Guid.NewGuid():N}@example.com";
        var token = team.InviteMember(email, role, _ownerId);
        team.AcceptInvitation(token, memberId, email);
        return memberId;
    }

    #endregion
}
