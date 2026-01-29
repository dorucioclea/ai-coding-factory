using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.Commands.CreateSharedProject;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.SharedProjects;

/// <summary>
/// Unit tests for CreateSharedProjectCommandHandler.
/// Story: ACF-013
/// </summary>
[Trait("Story", "ACF-013")]
public class CreateSharedProjectCommandHandlerTests
{
    private readonly Mock<ISharedProjectRepository> _repositoryMock;
    private readonly Mock<ILogger<CreateSharedProjectCommandHandler>> _loggerMock;
    private readonly CreateSharedProjectCommandHandler _handler;

    public CreateSharedProjectCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISharedProjectRepository>();
        _loggerMock = new Mock<ILogger<CreateSharedProjectCommandHandler>>();
        _handler = new CreateSharedProjectCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateProject()
    {
        var command = new CreateSharedProjectCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Collab Project", "A test project");

        _repositoryMock.Setup(r => r.GetByCollaborationRequestIdAsync(
            command.CollaborationRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SharedProject?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Collab Project");
        result.MemberCount.Should().Be(2);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SharedProject>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingProject_ShouldReturnExisting()
    {
        var collaborationRequestId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var existingProject = SharedProject.Create(collaborationRequestId, senderId, recipientId, "Existing");

        _repositoryMock.Setup(r => r.GetByCollaborationRequestIdAsync(
            collaborationRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        var command = new CreateSharedProjectCommand(
            collaborationRequestId, senderId, recipientId, "New Name");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Existing");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SharedProject>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
