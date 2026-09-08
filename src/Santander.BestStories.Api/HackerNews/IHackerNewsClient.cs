namespace Santander.BestStories.Api.HackerNews;

public interface IHackerNewsClient
{
    Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken);

    Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken cancellationToken);
}
