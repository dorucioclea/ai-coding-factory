using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.Commands.AcceptInvitation;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Teams;

/// <summary>
/// Unit tests for AcceptInvitationCommandHandler.
/// Story: ACF-007
/// </summary>
[Trait("Story", "ACF-007")]
public class AcceptInvitationCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _repositoryMock;
    private readonly Mock<ILogger<AcceptInvitationCommandHandler>> _loggerMock;
    private readonly AcceptInvitationCommandHandler _handler;

    public AcceptInvitationCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<AcceptInvitationCommandHandler>>();
        _handler = new AcceptInvitationCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleWithValidTokenShouldAddMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var newMemberId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");
        var token = team.InviteMember("newmember@example.com", TeamRole.Editor, ownerId);

        _repositoryMock.Setup(x => x.GetByInvitationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var userEmail = "newmember@example.com";
        var command = new AcceptInvitationCommand(token, newMemberId, userEmail);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(newMemberId);
        result.Role.Should().Be(TeamRole.Editor);

        _repositoryMock.Verify(x => x.UpdateAsync(team, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithInvalidTokenShouldThrow()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByInvitationTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var command = new AcceptInvitationCommand("invalid-token", Guid.NewGuid(), "user@example.com");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task HandleWhenAlreadyMemberShouldThrow()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var team = Team.Create(ownerId, "Test Team");
        var token = team.InviteMember("member@example.com", TeamRole.Editor, ownerId);

        _repositoryMock.Setup(x => x.GetByInvitationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Owner tries to accept with matching email (passes email check, fails on already member)
        var command = new AcceptInvitationCommand(token, ownerId, "member@example.com");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already a member*");
    }
}
