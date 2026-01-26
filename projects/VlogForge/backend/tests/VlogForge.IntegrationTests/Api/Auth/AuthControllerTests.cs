using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using VlogForge.Api.Controllers.Auth.Requests;
using VlogForge.Application.Auth.DTOs;
using VlogForge.IntegrationTests.Fixtures;
using Xunit;

namespace VlogForge.IntegrationTests.Api.Auth;

/// <summary>
/// Integration tests for AuthController.
/// Story: ACF-001
/// </summary>
[Trait("Story", "ACF-001")]
[Collection("Database")]
public class AuthControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuthControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Register Tests

    [Fact]
    public async Task RegisterWithValidDataShouldReturnTokens()
    {
        // Arrange
        var request = new RegisterRequest(
            $"test-{Guid.NewGuid():N}@example.com",
            "Password123!",
            "Test User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.DisplayName.Should().Be(request.DisplayName);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterWithExistingEmailShouldReturnConflict()
    {
        // Arrange - First register
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest(email, "Password123!", "Test User");
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Try to register again with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RegisterWithInvalidEmailShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            "invalid-email",
            "Password123!",
            "Test User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterWithWeakPasswordShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            $"test-{Guid.NewGuid():N}@example.com",
            "weak",
            "Test User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginWithValidCredentialsShouldReturnTokens()
    {
        // Arrange - First register
        var email = $"login-test-{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        var registerRequest = new RegisterRequest(email, password, "Test User");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act
        var loginRequest = new LoginRequest(email, password);
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginWithInvalidPasswordShouldReturnUnauthorized()
    {
        // Arrange - First register
        var email = $"login-fail-{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterRequest(email, "Password123!", "Test User");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act
        var loginRequest = new LoginRequest(email, "WrongPassword!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginWithNonExistentEmailShouldReturnUnauthorized()
    {
        // Act
        var loginRequest = new LoginRequest("nonexistent@example.com", "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshTokenWithValidTokenShouldReturnNewTokens()
    {
        // Arrange - Register to get tokens
        var email = $"refresh-test-{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterRequest(email, "Password123!", "Test User");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        // Act
        var refreshRequest = new RefreshTokenRequest(authResult!.RefreshToken);
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        // New tokens should be different (token rotation)
        result.RefreshToken.Should().NotBe(authResult.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenWithInvalidTokenShouldReturnUnauthorized()
    {
        // Act
        var refreshRequest = new RefreshTokenRequest("invalid-refresh-token");
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshTokenWithRevokedTokenShouldReturnUnauthorized()
    {
        // Arrange - Register to get tokens
        var email = $"revoked-test-{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterRequest(email, "Password123!", "Test User");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        // First refresh (this revokes the original token)
        var refreshRequest = new RefreshTokenRequest(authResult!.RefreshToken);
        await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Act - Try to use the old token again
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Forgot Password Tests

    [Fact]
    public async Task ForgotPasswordWithExistingEmailShouldReturnSuccess()
    {
        // Arrange - First register
        var email = $"forgot-test-{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterRequest(email, "Password123!", "Test User");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act
        var forgotRequest = new ForgotPasswordRequest(email);
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPasswordWithNonExistentEmailShouldStillReturnSuccess()
    {
        // This prevents user enumeration
        // Act
        var forgotRequest = new ForgotPasswordRequest("nonexistent@example.com");
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task LogoutWithValidTokenShouldReturnSuccess()
    {
        // Arrange - Register to get tokens
        var email = $"logout-test-{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterRequest(email, "Password123!", "Test User");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        // Add authorization header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.AccessToken);

        // Act
        var logoutRequest = new LogoutRequest(authResult.RefreshToken);
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Clean up
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task LogoutWithoutAuthorizationShouldReturnUnauthorized()
    {
        // Act
        var logoutRequest = new LogoutRequest("some-token");
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
