using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Services.PlatformData;

/// <summary>
/// Mock Instagram data service for development and testing.
/// Generates realistic sample data that simulates API responses.
/// Story: ACF-004
/// </summary>
public sealed class MockInstagramDataService : IPlatformDataService
{
    private static readonly Random Random = new();

    public PlatformType SupportedPlatform => PlatformType.Instagram;

    public Task<PlatformMetricsData> FetchMetricsAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var seed = accessToken.GetHashCode();
        var rng = new Random(seed);

        var baseFollowers = rng.Next(5000, 500000);
        var baseViews = baseFollowers * rng.Next(5, 20);

        var metrics = new PlatformMetricsData(
            FollowerCount: baseFollowers + Random.Next(-500, 1000),
            TotalViews: baseViews + Random.Next(-5000, 10000),
            TotalLikes: baseViews / rng.Next(5, 15),
            TotalComments: baseViews / rng.Next(30, 100),
            TotalShares: baseViews / rng.Next(50, 150));

        return Task.FromResult(metrics);
    }

    public Task<IReadOnlyList<ContentData>> FetchTopContentAsync(
        string accessToken,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var seed = accessToken.GetHashCode();
        var rng = new Random(seed);
        var content = new List<ContentData>();

        var titles = new[]
        {
            "Summer Vibes \u2600\ufe0f",
            "Coffee & Creativity",
            "Behind the scenes today",
            "New collection drop!",
            "Sunday self-care",
            "Travel memories \u2708\ufe0f",
            "OOTD - casual friday",
            "Sunset views",
            "Product unboxing",
            "Monday motivation",
            "Weekend adventures",
            "Home office setup",
            "Recipe of the day",
            "Fitness journey update",
            "Brand collaboration reveal"
        };

        for (var i = 0; i < Math.Min(limit, titles.Length); i++)
        {
            var postId = $"ig_{Guid.NewGuid():N}"[..20];
            var views = rng.Next(5000, 1000000);
            var publishDate = DateTime.UtcNow.AddDays(-rng.Next(1, 60));

            content.Add(new ContentData(
                ContentId: postId,
                Title: titles[i],
                ThumbnailUrl: $"https://instagram.com/p/{postId}/media",
                ContentUrl: $"https://www.instagram.com/p/{postId}/",
                PublishedAt: publishDate,
                ViewCount: views,
                LikeCount: views / rng.Next(5, 15),
                CommentCount: views / rng.Next(30, 100),
                ShareCount: views / rng.Next(50, 150)));
        }

        IReadOnlyList<ContentData> result = content.OrderByDescending(c => c.ViewCount).ToList();
        return Task.FromResult(result);
    }
}
