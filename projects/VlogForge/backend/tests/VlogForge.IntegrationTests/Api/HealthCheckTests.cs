using System.Net;
using FluentAssertions;
using VlogForge.IntegrationTests.Fixtures;
using Xunit;

namespace VlogForge.IntegrationTests.Api;

[Trait("Story", "ACF-001")]
[Collection("Database")]
public class HealthCheckTests
{
    private readonly HttpClient _client;

    public HealthCheckTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpointReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthReadyEndpointReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthLiveEndpointReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
