using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.Commands.ChangeMemberRole;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Teams;

/// <summary>
/// Unit tests for ChangeMemberRoleCommandHandler.
/// Story: ACF-007
/// </summary>
[Trait("Story", "ACF-007")]
public class ChangeMemberRoleCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _repositoryMock;
    private readonly Mock<ILogger<ChangeMemberRoleCommandHandler>> _loggerMock;
    private readonly ChangeMemberRoleCommandHandler _handler;

    public ChangeMemberRoleCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<ChangeMemberRoleCommandHandler>>();
        _handler = new ChangeMemberRoleCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidDataShouldChangeRole()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Viewer, ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);

        _repositoryMock.Setup(x => x.GetByIdWithMembersAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new ChangeMemberRoleCommand(team.Id, memberId, TeamRole.Editor, ownerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(TeamRole.Editor);

        _repositoryMock.Verify(x => x.UpdateAsync(team, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithTeamNotFoundShouldThrow()
    {
        // Arrange
        var teamId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdWithMembersAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new ChangeMemberRoleCommand(teamId, Guid.NewGuid(), TeamRole.Editor, Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Team not found*");
    }

    [Fact]
    public async Task HandleAsNonAdminShouldThrow()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var editorId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");

        var editorEmail = "editor@example.com";
        var editorToken = team.InviteMember(editorEmail, TeamRole.Editor, ownerId);
        team.AcceptInvitation(editorToken, editorId, editorEmail);

        var viewerEmail = "viewer@example.com";
        var viewerToken = team.InviteMember(viewerEmail, TeamRole.Viewer, ownerId);
        team.AcceptInvitation(viewerToken, viewerId, viewerEmail);

        _repositoryMock.Setup(x => x.GetByIdWithMembersAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Editor tries to change viewer's role
        var command = new ChangeMemberRoleCommand(team.Id, viewerId, TeamRole.Editor, editorId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task HandleChangingOwnerRoleShouldThrow()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");

        _repositoryMock.Setup(x => x.GetByIdWithMembersAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new ChangeMemberRoleCommand(team.Id, ownerId, TeamRole.Admin, ownerId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot change the owner's role*");
    }
}
