using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.SharedProjects.Commands.AddProjectLink;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Exceptions;
using Xunit;

namespace VlogForge.UnitTests.Application.SharedProjects;

/// <summary>
/// Unit tests for AddProjectLinkCommandHandler.
/// Story: ACF-013
/// </summary>
[Trait("Story", "ACF-013")]
public class AddProjectLinkCommandHandlerTests
{
    private readonly Mock<ISharedProjectRepository> _repositoryMock;
    private readonly Mock<ILogger<AddProjectLinkCommandHandler>> _loggerMock;
    private readonly AddProjectLinkCommandHandler _handler;

    public AddProjectLinkCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISharedProjectRepository>();
        _loggerMock = new Mock<ILogger<AddProjectLinkCommandHandler>>();
        _handler = new AddProjectLinkCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldAddLink()
    {
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var project = SharedProject.Create(Guid.NewGuid(), senderId, recipientId, "Test Project");

        _repositoryMock.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new AddProjectLinkCommand(project.Id, senderId, "Script", "https://docs.google.com/doc/123");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Script");
        result.Url.Should().Be("https://docs.google.com/doc/123");
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProject_ShouldThrow()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SharedProject?)null);

        var command = new AddProjectLinkCommand(Guid.NewGuid(), Guid.NewGuid(), "Link", "https://example.com");
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<SharedProjectNotFoundException>();
    }
}
