using FluentAssertions;
using Moq;
using VlogForge.Application.Calendar.Queries.GetCalendar;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Application.Calendar;

/// <summary>
/// Unit tests for GetCalendarQueryHandler.
/// Story: ACF-006
/// </summary>
[Trait("Story", "ACF-006")]
public class GetCalendarQueryHandlerTests
{
    private readonly Mock<IContentItemRepository> _repositoryMock;
    private readonly GetCalendarQueryHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetCalendarQueryHandlerTests()
    {
        _repositoryMock = new Mock<IContentItemRepository>();
        _handler = new GetCalendarQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleShouldReturnEmptyCalendarWhenNoScheduledItems()
    {
        // Arrange
        var query = new GetCalendarQuery(_userId, 2026, 1);
        _repositoryMock
            .Setup(r => r.GetScheduledForDateRangeAsync(
                _userId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentItem>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Year.Should().Be(2026);
        result.Month.Should().Be(1);
        result.TotalItems.Should().Be(0);
        result.Days.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleShouldGroupItemsByDay()
    {
        // Arrange
        var query = new GetCalendarQuery(_userId, 2026, 1);
        var item1 = CreateContentItemWithScheduledDate(new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc));
        var item2 = CreateContentItemWithScheduledDate(new DateTime(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc));
        var item3 = CreateContentItemWithScheduledDate(new DateTime(2026, 1, 20, 9, 0, 0, DateTimeKind.Utc));

        _repositoryMock
            .Setup(r => r.GetScheduledForDateRangeAsync(
                _userId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentItem> { item1, item2, item3 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalItems.Should().Be(3);
        result.Days.Should().HaveCount(2);
        result.Days[0].Date.Should().Be(new DateOnly(2026, 1, 15));
        result.Days[0].Items.Should().HaveCount(2);
        result.Days[1].Date.Should().Be(new DateOnly(2026, 1, 20));
        result.Days[1].Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleShouldCallRepositoryWithCorrectDateRange()
    {
        // Arrange
        var query = new GetCalendarQuery(_userId, 2026, 2);
        _repositoryMock
            .Setup(r => r.GetScheduledForDateRangeAsync(
                _userId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentItem>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetScheduledForDateRangeAsync(
            _userId,
            new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            It.Is<DateTime>(d => d.Month == 2 && d.Day == 28),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldOrderDaysByDate()
    {
        // Arrange
        var query = new GetCalendarQuery(_userId, 2026, 1);
        var item1 = CreateContentItemWithScheduledDate(new DateTime(2026, 1, 25, 10, 0, 0, DateTimeKind.Utc));
        var item2 = CreateContentItemWithScheduledDate(new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc));
        var item3 = CreateContentItemWithScheduledDate(new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc));

        _repositoryMock
            .Setup(r => r.GetScheduledForDateRangeAsync(
                _userId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentItem> { item1, item2, item3 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Days.Should().BeInAscendingOrder(d => d.Date);
    }

    private ContentItem CreateContentItemWithScheduledDate(DateTime scheduledDate)
    {
        var item = ContentItem.Create(_userId, $"Test Item {Guid.NewGuid()}", "Test notes");
        item.UpdateScheduledDate(scheduledDate);
        return item;
    }
}
