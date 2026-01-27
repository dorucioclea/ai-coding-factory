using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Approvals.Commands.ConfigureWorkflow;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Approvals;

/// <summary>
/// Unit tests for ConfigureWorkflowCommandHandler.
/// Story: ACF-009
/// </summary>
[Trait("Story", "ACF-009")]
public class ConfigureWorkflowCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<ILogger<ConfigureWorkflowCommandHandler>> _loggerMock;
    private readonly ConfigureWorkflowCommandHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();

    public ConfigureWorkflowCommandHandlerTests()
    {
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<ConfigureWorkflowCommandHandler>>();
        _handler = new ConfigureWorkflowCommandHandler(_teamRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleShouldEnableApproval()
    {
        // Arrange
        var team = Team.Create(_ownerId, "Test Team");
        SetupTeamRepository(team);

        var command = new ConfigureWorkflowCommand(_teamId, _ownerId, true, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RequiresApproval.Should().BeTrue();
        result.ApproverIds.Should().BeEmpty();
        result.TeamId.Should().Be(team.Id);

        _teamRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Once);
        _teamRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleShouldDisableApproval()
    {
        // Arrange
        var team = Team.Create(_ownerId, "Test Team");
        team.ConfigureWorkflow(true, null, _ownerId);
        SetupTeamRepository(team);

        var command = new ConfigureWorkflowCommand(_teamId, _ownerId, false, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.RequiresApproval.Should().BeFalse();
    }

    [Fact]
    public async Task HandleWithApproverIdsShouldSetApprovers()
    {
        // Arrange
        var team = Team.Create(_ownerId, "Test Team");
        var adminId = AddMemberToTeam(team, TeamRole.Admin);
        SetupTeamRepository(team);

        var command = new ConfigureWorkflowCommand(_teamId, _ownerId, true, new[] { adminId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.RequiresApproval.Should().BeTrue();
        result.ApproverIds.Should().ContainSingle().Which.Should().Be(adminId);
    }

    [Fact]
    public async Task HandleWithNonExistentTeamShouldThrow()
    {
        // Arrange
        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new ConfigureWorkflowCommand(_teamId, _ownerId, true, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamNotFoundException>();
    }

    [Fact]
    public async Task HandleWithUnauthorizedUserShouldThrow()
    {
        // Arrange
        var team = Team.Create(_ownerId, "Test Team");
        var viewerId = AddMemberToTeam(team, TeamRole.Viewer);
        SetupTeamRepository(team);

        var command = new ConfigureWorkflowCommand(_teamId, viewerId, true, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private void SetupTeamRepository(Team team)
    {
        _teamRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
    }

    private Guid AddMemberToTeam(Team team, TeamRole role)
    {
        var memberId = Guid.NewGuid();
        var email = $"member_{Guid.NewGuid():N}@example.com";
        var token = team.InviteMember(email, role, _ownerId);
        team.AcceptInvitation(token, memberId, email);
        return memberId;
    }
}
