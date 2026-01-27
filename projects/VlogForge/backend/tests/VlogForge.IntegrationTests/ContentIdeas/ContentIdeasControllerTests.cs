using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using VlogForge.Api.Controllers.Auth.Requests;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.ContentIdeas.DTOs;
using VlogForge.Domain.Entities;
using VlogForge.IntegrationTests.Fixtures;
using Xunit;

namespace VlogForge.IntegrationTests.ContentIdeas;

/// <summary>
/// Integration tests for ContentIdeasController.
/// Story: ACF-005
/// </summary>
[Trait("Story", "ACF-005")]
[Collection("Database")]
public class ContentIdeasControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ContentIdeasControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Helper Methods

    private async Task<AuthResponse> RegisterAndGetTokensAsync()
    {
        var email = $"content-test-{Guid.NewGuid():N}@example.com";
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

    private async Task<ContentIdeaResponse> CreateTestContentIdea(string title)
    {
        var request = new
        {
            Title = title,
            Notes = "Test notes",
            PlatformTags = new[] { "YouTube" }
        };

        var response = await _client.PostAsJsonAsync("/api/content", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions))!;
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateContentIdeaWithValidDataShouldReturnCreated()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var request = new
        {
            Title = "My First Video Idea",
            Notes = "This is a great idea for a video",
            PlatformTags = new[] { "YouTube", "TikTok" }
        };

        try
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/content", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var result = await response.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Title.Should().Be("My First Video Idea");
            result.Notes.Should().Be("This is a great idea for a video");
            result.Status.Should().Be(IdeaStatus.Idea);
            result.PlatformTags.Should().Contain("youtube");
            result.PlatformTags.Should().Contain("tiktok");
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task CreateContentIdeaUnauthenticatedShouldReturnUnauthorized()
    {
        // Arrange
        var request = new { Title = "Test", Notes = "Test notes" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/content", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateContentIdeaWithEmptyTitleShouldReturnBadRequest()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var request = new { Title = "", Notes = "Some notes" };

        try
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/content", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetAllShouldReturnUserIdeas()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            await CreateTestContentIdea("Idea 1");
            await CreateTestContentIdea("Idea 2");

            // Act
            var response = await _client.GetAsync("/api/content");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ContentIdeasListResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Items.Should().HaveCountGreaterOrEqualTo(2);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task GetAllWithStatusFilterShouldReturnFilteredIdeas()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var idea1 = await CreateTestContentIdea("Idea 1");
            await CreateTestContentIdea("Idea 2");

            // Move one to Draft
            await _client.PatchAsJsonAsync($"/api/content/{idea1.Id}/status", new { Status = IdeaStatus.Draft });

            // Act
            var response = await _client.GetAsync("/api/content?status=Draft");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ContentIdeasListResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Items.Should().OnlyContain(i => i.Status == IdeaStatus.Draft);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task GetByIdExistingIdeaShouldReturnIdea()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var created = await CreateTestContentIdea("Test Idea");

            // Act
            var response = await _client.GetAsync($"/api/content/{created.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Id.Should().Be(created.Id);
            result.Title.Should().Be("Test Idea");
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task GetByIdNonExistingIdeaShouldReturnNotFound()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            // Act
            var response = await _client.GetAsync($"/api/content/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateWithValidDataShouldReturnUpdatedIdea()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var created = await CreateTestContentIdea("Original Title");

            var updateRequest = new
            {
                Title = "Updated Title",
                Notes = "Updated notes",
                PlatformTags = new[] { "Instagram" }
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/content/{created.Id}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Title.Should().Be("Updated Title");
            result.Notes.Should().Be("Updated notes");
            result.PlatformTags.Should().Contain("instagram");
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task UpdateStatusToNextValidStatusShouldSucceed()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var created = await CreateTestContentIdea("Test Idea");

            // Act - Move from Idea to Draft
            var response = await _client.PatchAsJsonAsync($"/api/content/{created.Id}/status", new { Status = IdeaStatus.Draft });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ContentIdeaResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Status.Should().Be(IdeaStatus.Draft);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task UpdateStatusSkippingStepsShouldReturnBadRequest()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var created = await CreateTestContentIdea("Test Idea");

            // Act - Try to skip from Idea directly to Scheduled
            var response = await _client.PatchAsJsonAsync($"/api/content/{created.Id}/status", new { Status = IdeaStatus.Scheduled });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteExistingIdeaShouldReturnNoContent()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            var created = await CreateTestContentIdea("Test Idea");

            // Act
            var response = await _client.DeleteAsync($"/api/content/{created.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify it's not returned in list (soft deleted)
            var listResponse = await _client.GetAsync("/api/content");
            var list = await listResponse.Content.ReadFromJsonAsync<ContentIdeasListResponse>(JsonOptions);
            list!.Items.Should().NotContain(i => i.Id == created.Id);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task DeleteNonExistingIdeaShouldReturnNotFound()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            // Act
            var response = await _client.DeleteAsync($"/api/content/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    #endregion

    #region Cross-User Access Tests

    [Fact]
    public async Task GetByIdForOtherUserContentShouldReturnForbidden()
    {
        // Arrange - Create content with User A
        var authA = await RegisterAndGetTokensAsync();
        SetAuthHeader(authA.AccessToken);
        var created = await CreateTestContentIdea("User A's Idea");
        ClearAuthHeader();

        // Act - Try to access with User B
        var authB = await RegisterAndGetTokensAsync();
        SetAuthHeader(authB.AccessToken);

        try
        {
            var response = await _client.GetAsync($"/api/content/{created.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task UpdateOtherUserContentShouldReturnForbidden()
    {
        // Arrange - Create content with User A
        var authA = await RegisterAndGetTokensAsync();
        SetAuthHeader(authA.AccessToken);
        var created = await CreateTestContentIdea("User A's Idea");
        ClearAuthHeader();

        // Act - Try to update with User B
        var authB = await RegisterAndGetTokensAsync();
        SetAuthHeader(authB.AccessToken);

        try
        {
            var updateRequest = new { Title = "Hacked Title", Notes = "Hacked" };
            var response = await _client.PutAsJsonAsync($"/api/content/{created.Id}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task DeleteOtherUserContentShouldReturnForbidden()
    {
        // Arrange - Create content with User A
        var authA = await RegisterAndGetTokensAsync();
        SetAuthHeader(authA.AccessToken);
        var created = await CreateTestContentIdea("User A's Idea");
        ClearAuthHeader();

        // Act - Try to delete with User B
        var authB = await RegisterAndGetTokensAsync();
        SetAuthHeader(authB.AccessToken);

        try
        {
            var response = await _client.DeleteAsync($"/api/content/{created.Id}");

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
