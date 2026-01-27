using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using VlogForge.Api.Controllers.Auth.Requests;
using VlogForge.Api.Controllers.Profiles.Requests;
using VlogForge.Application.Auth.DTOs;
using VlogForge.Application.Profiles.DTOs;
using VlogForge.IntegrationTests.Fixtures;
using Xunit;

namespace VlogForge.IntegrationTests.Api.Profiles;

/// <summary>
/// Integration tests for ProfilesController.
/// Story: ACF-002
/// </summary>
[Trait("Story", "ACF-002")]
[Collection("Database")]
public class ProfilesControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ProfilesControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Helper Methods

    private async Task<AuthResponse> RegisterAndGetTokensAsync()
    {
        var email = $"profile-test-{Guid.NewGuid():N}@example.com";
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

    #endregion

    #region Create Profile Tests

    [Fact]
    public async Task CreateProfileWithValidDataShouldSucceed()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var username = $"user{Guid.NewGuid():N}"[..20];
        var request = new CreateProfileRequest
        {
            Username = username,
            DisplayName = "Test Creator",
            Bio = "I create awesome content!"
        };

        try
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/profiles/me", request);

            // Assert
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(HttpStatusCode.Created, $"Error: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<CreatorProfileResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Username.Should().Be(username.ToLowerInvariant());
            result.DisplayName.Should().Be("Test Creator");
            result.Bio.Should().Be("I create awesome content!");
            result.UserId.Should().Be(auth.UserId);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task CreateProfileWhenAlreadyExistsShouldReturnConflict()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var username = $"dup{Guid.NewGuid():N}"[..15];
        var request = new CreateProfileRequest
        {
            Username = username,
            DisplayName = "Test Creator"
        };

        try
        {
            // Create first profile
            var firstResponse = await _client.PostAsJsonAsync("/api/profiles/me", request);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act - Try to create again
            var secondResponse = await _client.PostAsJsonAsync("/api/profiles/me", new CreateProfileRequest
            {
                Username = $"diff{Guid.NewGuid():N}"[..15],
                DisplayName = "Different Name"
            });

            // Assert
            secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task CreateProfileWithDuplicateUsernameShouldReturnConflict()
    {
        // Arrange - First user creates profile
        var auth1 = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth1.AccessToken);

        var sharedUsername = $"shared{Guid.NewGuid():N}"[..15];
        var request1 = new CreateProfileRequest
        {
            Username = sharedUsername,
            DisplayName = "First User"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/profiles/me", request1);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        ClearAuthHeader();

        // Second user tries to use same username
        var auth2 = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth2.AccessToken);

        var request2 = new CreateProfileRequest
        {
            Username = sharedUsername,
            DisplayName = "Second User"
        };

        try
        {
            // Act
            var secondResponse = await _client.PostAsJsonAsync("/api/profiles/me", request2);

            // Assert
            secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task CreateProfileWithoutAuthShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateProfileRequest
        {
            Username = "testuser",
            DisplayName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/profiles/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Profile Tests

    [Fact]
    public async Task GetMyProfileShouldReturnProfile()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var username = $"myprof{Guid.NewGuid():N}"[..15];
        await _client.PostAsJsonAsync("/api/profiles/me", new CreateProfileRequest
        {
            Username = username,
            DisplayName = "My Profile"
        });

        try
        {
            // Act
            var response = await _client.GetAsync("/api/profiles/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CreatorProfileResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.Username.Should().Be(username.ToLowerInvariant());
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task GetMyProfileWhenNoProfileExistsShouldReturn404()
    {
        // Arrange - New user without profile
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        try
        {
            // Act
            var response = await _client.GetAsync("/api/profiles/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task GetProfileByUsernameShouldReturnPublicProfile()
    {
        // Arrange - Create a profile
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var username = $"public{Guid.NewGuid():N}"[..15];
        await _client.PostAsJsonAsync("/api/profiles/me", new CreateProfileRequest
        {
            Username = username,
            DisplayName = "Public Creator",
            Bio = "Public bio"
        });
        ClearAuthHeader();

        // Act - Get without auth (public endpoint)
        var response = await _client.GetAsync($"/api/profiles/{username}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PublicProfileResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Username.Should().Be(username.ToLowerInvariant());
        result.DisplayName.Should().Be("Public Creator");
        result.Bio.Should().Be("Public bio");
    }

    [Fact]
    public async Task GetProfileByNonExistentUsernameShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/profiles/nonexistentuser123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update Profile Tests

    [Fact]
    public async Task UpdateProfileShouldSucceed()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var username = $"update{Guid.NewGuid():N}"[..15];
        await _client.PostAsJsonAsync("/api/profiles/me", new CreateProfileRequest
        {
            Username = username,
            DisplayName = "Original Name"
        });

        var updateRequest = new UpdateProfileRequest
        {
            DisplayName = "Updated Name",
            Bio = "Updated bio",
            NicheTags = new[] { "gaming", "tech" },
            OpenToCollaborations = true,
            CollaborationPreferences = "Looking for gaming collabs"
        };

        try
        {
            // Act
            var response = await _client.PutAsJsonAsync("/api/profiles/me", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CreatorProfileResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.DisplayName.Should().Be("Updated Name");
            result.Bio.Should().Be("Updated bio");
            result.NicheTags.Should().HaveCount(2);
            result.OpenToCollaborations.Should().BeTrue();
            result.CollaborationPreferences.Should().Be("Looking for gaming collabs");
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    [Fact]
    public async Task UpdateProfileWithoutProfileShouldReturn404()
    {
        // Arrange - New user without profile
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var updateRequest = new UpdateProfileRequest
        {
            DisplayName = "Updated Name"
        };

        try
        {
            // Act
            var response = await _client.PutAsJsonAsync("/api/profiles/me", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    #endregion

    #region Collaboration Settings Tests

    [Fact]
    public async Task SetCollaborationSettingsShouldSucceed()
    {
        // Arrange
        var auth = await RegisterAndGetTokensAsync();
        SetAuthHeader(auth.AccessToken);

        var username = $"collab{Guid.NewGuid():N}"[..15];
        await _client.PostAsJsonAsync("/api/profiles/me", new CreateProfileRequest
        {
            Username = username,
            DisplayName = "Collab Test"
        });

        var settingsRequest = new SetCollaborationSettingsRequest
        {
            OpenToCollaborations = true,
            CollaborationPreferences = "Open to all collabs"
        };

        try
        {
            // Act
            var response = await _client.PutAsJsonAsync("/api/profiles/me/collaboration", settingsRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CreatorProfileResponse>(JsonOptions);
            result.Should().NotBeNull();
            result!.OpenToCollaborations.Should().BeTrue();
            result.CollaborationPreferences.Should().Be("Open to all collabs");
        }
        finally
        {
            ClearAuthHeader();
        }
    }

    #endregion
}
