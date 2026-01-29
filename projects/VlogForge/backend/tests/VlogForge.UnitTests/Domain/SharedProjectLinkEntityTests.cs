using FluentAssertions;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for the SharedProjectLink entity.
/// Story: ACF-013
/// </summary>
[Trait("Story", "ACF-013")]
public class SharedProjectLinkEntityTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateLink()
    {
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var link = SharedProjectLink.Create(projectId, userId, "Script", "https://docs.google.com/doc/123", "Our script doc");

        link.Should().NotBeNull();
        link.SharedProjectId.Should().Be(projectId);
        link.AddedByUserId.Should().Be(userId);
        link.Title.Should().Be("Script");
        link.Url.Should().Be("https://docs.google.com/doc/123");
        link.Description.Should().Be("Our script doc");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ShouldThrow(string title)
    {
        var act = () => SharedProjectLink.Create(Guid.NewGuid(), Guid.NewGuid(), title, "https://example.com");
        act.Should().Throw<ArgumentException>().WithMessage("*title*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUrl_ShouldThrow(string url)
    {
        var act = () => SharedProjectLink.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", url);
        act.Should().Throw<ArgumentException>().WithMessage("*URL*");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("file:///path/to/file")]
    public void Create_WithInvalidUrl_ShouldThrow(string url)
    {
        var act = () => SharedProjectLink.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", url);
        act.Should().Throw<ArgumentException>().WithMessage("*HTTP*");
    }

    [Fact]
    public void Create_WithHttpUrl_ShouldSucceed()
    {
        var link = SharedProjectLink.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "http://example.com");
        link.Url.Should().Be("http://example.com");
    }
}
