using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace VlogForge.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainNamespace = "VlogForge.Domain";
    private const string ApplicationNamespace = "VlogForge.Application";
    private const string InfrastructureNamespace = "VlogForge.Infrastructure";
    private const string ApiNamespace = "VlogForge.Api";

    [Fact]
    public void DomainShouldNotHaveDependencyOnOtherProjects()
    {
        // Arrange
        var assembly = typeof(Domain.AssemblyReference).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        // Arrange
        var assembly = typeof(Application.AssemblyReference).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void HandlersShouldHaveDependencyOnDomain()
    {
        // Arrange
        var assembly = typeof(Application.AssemblyReference).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .HaveDependencyOn(DomainNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ControllersShouldHaveDependencyOnMediatR()
    {
        // Arrange
        var assembly = typeof(Api.AssemblyReference).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .HaveDependencyOn("MediatR")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEntitiesShouldBeSealed()
    {
        // Arrange
        var assembly = typeof(Domain.AssemblyReference).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("VlogForge.Domain.Entities")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
