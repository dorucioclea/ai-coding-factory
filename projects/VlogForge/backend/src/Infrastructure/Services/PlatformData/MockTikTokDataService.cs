using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Entities;

namespace VlogForge.Infrastructure.Services.PlatformData;

/// <summary>
/// Mock TikTok data service for development and testing.
/// Generates realistic sample data that simulates API responses.
/// Story: ACF-004
/// </summary>
public sealed class MockTikTokDataService : IPlatformDataService
{
    private static readonly Random Random = new();

    public PlatformType SupportedPlatform => PlatformType.TikTok;

    public Task<PlatformMetricsData> FetchMetricsAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var seed = accessToken.GetHashCode();
        var rng = new Random(seed);

        var baseFollowers = rng.Next(10000, 1000000);
        var baseViews = baseFollowers * rng.Next(20, 100);

        var metrics = new PlatformMetricsData(
            FollowerCount: baseFollowers + Random.Next(-2000, 5000),
            TotalViews: baseViews + Random.Next(-50000, 100000),
            TotalLikes: baseViews / rng.Next(3, 10),
            TotalComments: baseViews / rng.Next(20, 80),
            TotalShares: baseViews / rng.Next(30, 100));

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
            "POV: when the beat drops \ud83d\udd25",
            "Reply to @user tutorial",
            "This trend is everything",
            "Wait for it...",
            "Stitch with @creator",
            "Life hack you NEED",
            "Trying viral recipe",
            "Get ready with me",
            "Before vs After",
            "Story time (part 1)",
            "Unpopular opinion but...",
            "Day in my life",
            "How I went viral",
            "Duet this!",
            "Things that just make sense"
        };

        for (var i = 0; i < Math.Min(limit, titles.Length); i++)
        {
            var videoId = $"tt_{Guid.NewGuid():N}"[..20];
            var views = rng.Next(50000, 5000000);
            var publishDate = DateTime.UtcNow.AddDays(-rng.Next(1, 30));

            content.Add(new ContentData(
                ContentId: videoId,
                Title: titles[i],
                ThumbnailUrl: $"https://p16-sign.tiktokcdn.com/obj/{videoId}",
                ContentUrl: $"https://www.tiktok.com/@user/video/{videoId}",
                PublishedAt: publishDate,
                ViewCount: views,
                LikeCount: views / rng.Next(3, 10),
                CommentCount: views / rng.Next(20, 80),
                ShareCount: views / rng.Next(30, 100)));
        }

        IReadOnlyList<ContentData> result = content.OrderByDescending(c => c.ViewCount).ToList();
        return Task.FromResult(result);
    }
}
