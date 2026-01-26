using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VlogForge.Application;
using Xunit;

namespace VlogForge.UnitTests.Application;

[Trait("Story", "ACF-0001")]
public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServicesShouldRegisterMediatR()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var mediatorRegistered = services.Any(descriptor => descriptor.ServiceType == typeof(IMediator));

        // Assert
        mediatorRegistered.Should().BeTrue();
    }
}
