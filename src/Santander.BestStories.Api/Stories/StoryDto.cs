using Santander.BestStories.Api.HackerNews;

namespace Santander.BestStories.Api.Stories;

public sealed record StoryDto(
    string Title,
    string? Uri,
    string PostedBy,
    DateTimeOffset Time,
    int Score,
    int CommentCount);

public static class StoryMapping
{
    public static StoryDto ToStoryDto(this HackerNewsItem item) => new(
        Title: item.Title ?? string.Empty,
        Uri: item.Url,
        PostedBy: item.By ?? string.Empty,
        Time: DateTimeOffset.FromUnixTimeSeconds(item.Time),
        Score: item.Score,
        CommentCount: item.Descendants ?? 0);
}
