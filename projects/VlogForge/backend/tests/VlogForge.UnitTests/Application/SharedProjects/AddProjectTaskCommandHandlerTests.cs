using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.Commands.AddProjectTask;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.SharedProjects;

/// <summary>
/// Unit tests for AddProjectTaskCommandHandler.
/// Story: ACF-013
/// </summary>
[Trait("Story", "ACF-013")]
public class AddProjectTaskCommandHandlerTests
{
    private readonly Mock<ISharedProjectRepository> _repositoryMock;
    private readonly Mock<ILogger<AddProjectTaskCommandHandler>> _loggerMock;
    private readonly AddProjectTaskCommandHandler _handler;

    public AddProjectTaskCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISharedProjectRepository>();
        _loggerMock = new Mock<ILogger<AddProjectTaskCommandHandler>>();
        _handler = new AddProjectTaskCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldAddTask()
    {
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var project = SharedProject.Create(Guid.NewGuid(), senderId, recipientId, "Test Project");

        _repositoryMock.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new AddProjectTaskCommand(project.Id, senderId, "Film intro", "Description");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Film intro");
        _repositoryMock.Verify(r => r.UpdateAsync(project, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProject_ShouldThrow()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SharedProject?)null);

        var command = new AddProjectTaskCommand(Guid.NewGuid(), Guid.NewGuid(), "Task");
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<SharedProjectNotFoundException>();
    }
}
