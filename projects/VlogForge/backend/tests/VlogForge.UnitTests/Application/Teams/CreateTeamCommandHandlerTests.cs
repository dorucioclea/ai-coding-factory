using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Teams.Commands.CreateTeam;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.Teams;

/// <summary>
/// Unit tests for CreateTeamCommandHandler.
/// Story: ACF-007
/// </summary>
[Trait("Story", "ACF-007")]
public class CreateTeamCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _repositoryMock;
    private readonly Mock<ILogger<CreateTeamCommandHandler>> _loggerMock;
    private readonly CreateTeamCommandHandler _handler;

    public CreateTeamCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<CreateTeamCommandHandler>>();
        _handler = new CreateTeamCommandHandler(_repositoryMock.Object, _loggerMock.Object);

        _repositoryMock.Setup(x => x.ExistsWithNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task HandleWithValidDataShouldCreateTeam()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new CreateTeamCommand(ownerId, "Content Creators", "A great team");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Content Creators");
        result.Description.Should().Be("A great team");
        result.OwnerId.Should().Be(ownerId);
        result.MemberCount.Should().Be(1); // Owner is automatically added

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWithDuplicateNameShouldThrow()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new CreateTeamCommand(ownerId, "Existing Team", null);

        _repositoryMock.Setup(x => x.ExistsWithNameAsync(ownerId, "Existing Team", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TeamNameAlreadyExistsException>();
    }

    [Fact]
    public async Task HandleShouldCallRepositoryWithCorrectTeam()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new CreateTeamCommand(ownerId, "Test Team", "Description");
        Team? capturedTeam = null;

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()))
            .Callback<Team, CancellationToken>((team, _) => capturedTeam = team)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTeam.Should().NotBeNull();
        capturedTeam!.Name.Should().Be("Test Team");
        capturedTeam.OwnerId.Should().Be(ownerId);
        capturedTeam.Members.Should().HaveCount(1);
        capturedTeam.Members.First().Role.Should().Be(TeamRole.Owner);
    }
}
