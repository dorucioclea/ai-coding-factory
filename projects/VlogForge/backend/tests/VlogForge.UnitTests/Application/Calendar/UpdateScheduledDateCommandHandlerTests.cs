using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VlogForge.Application.Calendar.Commands.UpdateScheduledDate;
using VlogForge.Application.Common.Exceptions;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Calendar;

/// <summary>
/// Unit tests for UpdateScheduledDateCommandHandler.
/// Story: ACF-006
/// </summary>
[Trait("Story", "ACF-006")]
public class UpdateScheduledDateCommandHandlerTests
{
    private readonly Mock<IContentItemRepository> _repositoryMock;
    private readonly Mock<ILogger<UpdateScheduledDateCommandHandler>> _loggerMock;
    private readonly UpdateScheduledDateCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _contentItemId = Guid.NewGuid();

    public UpdateScheduledDateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IContentItemRepository>();
        _loggerMock = new Mock<ILogger<UpdateScheduledDateCommandHandler>>();
        _handler = new UpdateScheduledDateCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleShouldUpdateScheduledDateWhenProvided()
    {
        // Arrange
        var item = ContentItem.Create(_userId, "Test Item", "Notes");
        var scheduledDate = DateTime.UtcNow.AddDays(7);
        var command = new UpdateScheduledDateCommand(_contentItemId, _userId, scheduledDate);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_contentItemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ScheduledDate.Should().NotBeNull();
        result.ScheduledDate!.Value.Should().BeCloseTo(scheduledDate, TimeSpan.FromSeconds(1));
        _repositoryMock.Verify(r => r.UpdateAsync(item, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleShouldClearScheduledDateWhenNull()
    {
        // Arrange
        var item = ContentItem.Create(_userId, "Test Item", "Notes");
        item.UpdateScheduledDate(DateTime.UtcNow.AddDays(7));
        var command = new UpdateScheduledDateCommand(_contentItemId, _userId, null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_contentItemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ScheduledDate.Should().BeNull();
        _repositoryMock.Verify(r => r.UpdateAsync(item, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleShouldThrowNotFoundExceptionWhenItemDoesNotExist()
    {
        // Arrange
        var command = new UpdateScheduledDateCommand(_contentItemId, _userId, DateTime.UtcNow.AddDays(7));

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_contentItemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleShouldThrowForbiddenAccessExceptionWhenUserDoesNotOwn()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var item = ContentItem.Create(differentUserId, "Test Item", "Notes");
        var command = new UpdateScheduledDateCommand(_contentItemId, _userId, DateTime.UtcNow.AddDays(7));

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_contentItemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

}
