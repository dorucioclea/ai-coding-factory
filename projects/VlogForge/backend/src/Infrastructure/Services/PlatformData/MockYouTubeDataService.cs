using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Services.PlatformData;

/// <summary>
/// Mock YouTube data service for development and testing.
/// Generates realistic sample data that simulates API responses.
/// Story: ACF-004
/// </summary>
public sealed class MockYouTubeDataService : IPlatformDataService
{
    private static readonly Random Random = new();

    public PlatformType SupportedPlatform => PlatformType.YouTube;

    public Task<PlatformMetricsData> FetchMetricsAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        // Generate consistent but varied metrics based on token hash
        var seed = accessToken.GetHashCode();
        var rng = new Random(seed);

        var baseFollowers = rng.Next(1000, 100000);
        var baseViews = baseFollowers * rng.Next(10, 50);

        var metrics = new PlatformMetricsData(
            FollowerCount: baseFollowers + Random.Next(-100, 200),
            TotalViews: baseViews + Random.Next(-1000, 5000),
            TotalLikes: baseViews / rng.Next(15, 30),
            TotalComments: baseViews / rng.Next(50, 150),
            TotalShares: baseViews / rng.Next(100, 300));

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
            "How I Started My YouTube Journey",
            "10 Tips for Better Video Quality",
            "My Morning Routine 2024",
            "Reacting to Your Comments",
            "Studio Tour - Behind the Scenes",
            "Q&A - You Asked, I Answer",
            "Day in My Life Vlog",
            "Best Camera Settings for YouTube",
            "Editing Tips for Beginners",
            "How to Grow Your Channel Fast",
            "My Equipment Setup Explained",
            "Responding to Hate Comments",
            "Collaboration Announcement!",
            "Big News About the Channel",
            "Thank You for 10K Subscribers!"
        };

        for (var i = 0; i < Math.Min(limit, titles.Length); i++)
        {
            var videoId = $"yt_{Guid.NewGuid():N}"[..20];
            var views = rng.Next(1000, 500000);
            var publishDate = DateTime.UtcNow.AddDays(-rng.Next(1, 90));

            content.Add(new ContentData(
                ContentId: videoId,
                Title: titles[i],
                ThumbnailUrl: $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg",
                ContentUrl: $"https://www.youtube.com/watch?v={videoId}",
                PublishedAt: publishDate,
                ViewCount: views,
                LikeCount: views / rng.Next(15, 30),
                CommentCount: views / rng.Next(50, 150),
                ShareCount: views / rng.Next(100, 300)));
        }

        IReadOnlyList<ContentData> result = content.OrderByDescending(c => c.ViewCount).ToList();
        return Task.FromResult(result);
    }
}
