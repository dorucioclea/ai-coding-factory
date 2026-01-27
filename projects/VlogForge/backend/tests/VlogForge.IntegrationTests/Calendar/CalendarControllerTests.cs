using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using VlogForge.Api.Controllers.Auth.Requests;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Calendar.DTOs;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.IntegrationTests.Fixtures;
using Xunit;

namespace VlogForge.IntegrationTests.Calendar;

/// <summary>
/// Integration tests for CalendarController.
/// Story: ACF-006
/// </summary>
[Trait("Story", "ACF-006")]
[Collection("Database")]
public class CalendarControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CalendarControllerTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    #region Helper Methods

    private async Task<AuthResponse> RegisterAndGetTokensAsync()
    {
        var email = $"calendar-test-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest(email, "Password123!", "Test User");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return result!;
    }

    private void SetAuthHeader(string accessToken)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private void ClearAuthHeader()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<ContentIdeaResponse> CreateAndScheduleContentIdea(string title, DateTime scheduledDate)
    {
        var createRequest = new
        {
            Title = title,
            Notes = "Test notes",
            PlatformTags = new[] { "YouTube" }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/content", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);

        var scheduleRequest = new { ScheduledDate = scheduledDate };
        var scheduleResponse = await _client.PatchAsJsonAsync($"/api/content/{created!.Id}/schedule", scheduleRequest);
        scheduleResponse.EnsureSuccessStatusCode();

        return (await scheduleResponse.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions))!;
    }

    #endregion

    #region GetCalendar Tests

    [Fact]
    public async Task GetCalendarEmptyMonthShouldReturnEmptyCalendar()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            // Act
            var response = await _client.GetAsync("/api/calendar?month=2030-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CalendarResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Year.Should().Be(2030);
            result.Month.Should().Be(1);
            result.TotalItems.Should().Be(0);
            result.Days.Should().BeEmpty();
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task GetCalendarWithScheduledItemsShouldReturnItems()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var scheduledDate = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);
            await CreateAndScheduleContentIdea("Video 1", scheduledDate);
            await CreateAndScheduleContentIdea("Video 2", scheduledDate.AddHours(4));

            // Act
            var response = await _client.GetAsync("/api/calendar?month=2026-03");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CalendarResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Year.Should().Be(2026);
            result.Month.Should().Be(3);
            result.TotalItems.Should().Be(2);
            result.Days.Should().HaveCount(1);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task GetCalendarWithInvalidMonthFormatShouldReturnBadRequest()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            // Act
            var response = await _client.GetAsync("/api/calendar?month=invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task GetCalendarUnauthenticatedShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/calendar?month=2026-01");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region UpdateSchedule Tests

    [Fact]
    public async Task UpdateScheduleShouldSetScheduledDate()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var createRequest = new { Title = "Test Video", Notes = "Notes" };
            var createResponse = await _client.PostAsJsonAsync("/api/content", createRequest);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);

            var scheduledDate = DateTime.UtcNow.AddDays(7);
            var scheduleRequest = new { ScheduledDate = scheduledDate };

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/content/{created!.Id}/schedule", scheduleRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.ScheduledDate.Should().NotBeNull();
            result.ScheduledDate!.Value.Should().BeCloseTo(scheduledDate, TimeSpan.FromSeconds(1));
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task UpdateScheduleWithNullShouldClearScheduledDate()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var scheduled = await CreateAndScheduleContentIdea("Test Video", DateTime.UtcNow.AddDays(7));

            var clearRequest = new { ScheduledDate = (DateTime?)null };

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/content/{scheduled.Id}/schedule", clearRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.ScheduledDate.Should().BeNull();
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task UpdateScheduleNonExistingIdeaShouldReturnNotFound()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var scheduleRequest = new { ScheduledDate = DateTime.UtcNow.AddDays(7) };

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/content/{Guid.NewGuid()}/schedule", scheduleRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task UpdateScheduleOtherUserContentShouldReturnForbidden()
    {
        // Arrange - Create content with User A
        var authA = await RegisterAndGetTokensAsync();
        SetAuthHeader(authA.AccessToken);
        var createRequest = new { Title = "User A's Video", Notes = "Notes" };
        var createResponse = await _client.PostAsJsonAsync("/api/content", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);
        ClearAuthHeader();

        // Act - Try to schedule with User B
        var authB = await RegisterAndGetTokensAsync();
        SetAuthHeader(authB.AccessToken);

        try
        {
            var scheduleRequest = new { ScheduledDate = DateTime.UtcNow.AddDays(7) };
            var response = await _client.PatchAsJsonAsync($"/api/content/{created!.Id}/schedule", scheduleRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    #endregion
}
