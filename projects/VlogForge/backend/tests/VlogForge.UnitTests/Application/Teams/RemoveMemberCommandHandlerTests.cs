using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.Commands.RemoveMember;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Teams;

/// <summary>
/// Unit tests for RemoveMemberCommandHandler.
/// Story: ACF-007
/// </summary>
[Trait("Story", "ACF-007")]
public class RemoveMemberCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _repositoryMock;
    private readonly Mock<ILogger<RemoveMemberCommandHandler>> _loggerMock;
    private readonly RemoveMemberCommandHandler _handler;

    public RemoveMemberCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<RemoveMemberCommandHandler>>();
        _handler = new RemoveMemberCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsOwnerShouldRemoveMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Editor, ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);

        _repositoryMock.Setup(x => x.GetByIdWithMembersAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new RemoveMemberCommand(team.Id, memberId, ownerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        team.IsMember(memberId).Should().BeFalse();

        _repositoryMock.Verify(x => x.UpdateAsync(team, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleMemberRemovingSelfShouldSucceed()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");
        var memberEmail = "member@example.com";
        var token = team.InviteMember(memberEmail, TeamRole.Editor, ownerId);
        team.AcceptInvitation(token, memberId, memberEmail);

        _repositoryMock.Setup(x => x.GetByIdWithMembersAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Member removes themselves
        var command = new RemoveMemberCommand(team.Id, memberId, memberId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        team.IsMember(memberId).Should().BeFalse();
    }

    [Fact]
    public async Task HandleWithTeamNotFoundShouldThrow()
    {
        // Arrange
        var teamId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdWithMembersAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new RemoveMemberCommand(teamId, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Team not found*");
    }

    [Fact]
    public async Task HandleRemovingOwnerShouldThrow()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");

        _repositoryMock.Setup(x => x.GetByIdWithMembersAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new RemoveMemberCommand(team.Id, ownerId, ownerId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot remove the team owner*");
    }

    [Fact]
    public async Task HandleAsNonAdminRemovingOtherShouldThrow()
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

        // Editor tries to remove viewer
        var command = new RemoveMemberCommand(team.Id, viewerId, editorId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
