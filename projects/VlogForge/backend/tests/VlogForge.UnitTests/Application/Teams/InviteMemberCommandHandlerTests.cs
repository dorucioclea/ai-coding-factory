using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.Commands.InviteMember;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Teams;

/// <summary>
/// Unit tests for InviteMemberCommandHandler.
/// Story: ACF-007
/// </summary>
[Trait("Story", "ACF-007")]
public class InviteMemberCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _repositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<InviteMemberCommandHandler>> _loggerMock;
    private readonly InviteMemberCommandHandler _handler;

    public InviteMemberCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITeamRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<InviteMemberCommandHandler>>();
        _handler = new InviteMemberCommandHandler(
            _repositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidDataShouldCreateInvitation()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");
        var teamId = team.Id;

        _repositoryMock.Setup(x => x.GetByIdWithInvitationsAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new InviteMemberCommand(teamId, ownerId, "newmember@example.com", TeamRole.Editor);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("newmember@example.com");
        result.Role.Should().Be(TeamRole.Editor);
        result.IsExpired.Should().BeFalse();

        _repositoryMock.Verify(x => x.UpdateAsync(team, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithTeamNotFoundShouldThrow()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdWithInvitationsAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new InviteMemberCommand(teamId, userId, "test@example.com", TeamRole.Editor);

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
        var viewerId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");

        // Add a viewer
        var viewerEmail = "viewer@example.com";
        var token = team.InviteMember(viewerEmail, TeamRole.Viewer, ownerId);
        team.AcceptInvitation(token, viewerId, viewerEmail);

        _repositoryMock.Setup(x => x.GetByIdWithInvitationsAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new InviteMemberCommand(team.Id, viewerId, "another@example.com", TeamRole.Editor);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task HandleShouldSendInvitationEmail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");

        _repositoryMock.Setup(x => x.GetByIdWithInvitationsAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new InviteMemberCommand(team.Id, ownerId, "invitee@example.com", TeamRole.Editor);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Give async fire-and-forget a moment
        await Task.Delay(100);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendTeamInvitationAsync("invitee@example.com", "Test Team", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
