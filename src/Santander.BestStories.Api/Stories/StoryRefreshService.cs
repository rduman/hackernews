using System.Collections.Concurrent;
using Santander.BestStories.Api.Configuration;
using Santander.BestStories.Api.HackerNews;

namespace Santander.BestStories.Api.Stories;

public sealed class StoryRefreshService(
    IHackerNewsClient client,
    IBestStoriesProvider provider,
    IHackerNewsSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(settings.RefreshInterval);

        do
        {
            await RefreshAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            var ids = await client.GetBestStoryIdsAsync(cancellationToken);
            var stories = await FetchStoriesAsync(ids, cancellationToken);

            if (stories.Count == 0)
            {
                Console.WriteLine($"Resolved no stories from {ids.Count} ids; keeping existing snapshot.");
                return;
            }

            provider.Publish([.. stories.OrderByDescending(story => story.Score)]);

            Console.WriteLine($"Refreshed {stories.Count} of {ids.Count} stories.");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine($"Story refresh failed; keeping existing snapshot. {ex.Message}");
        }
    }

    private async Task<IReadOnlyCollection<StoryDto>> FetchStoriesAsync(
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken)
    {
        var stories = new ConcurrentBag<StoryDto>();

        await Parallel.ForEachAsync(
            ids,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = settings.MaxConcurrentItemRequests,
                CancellationToken = cancellationToken
            },
            async (id, token) =>
            {
                try
                {
                    var item = await client.GetItemAsync(id, token);

                    if (item is not null && item.IsPublishableStory())
                    {
                        stories.Add(item.ToStoryDto());
                    }
                }
                catch (Exception ex) when (!token.IsCancellationRequested)
                {
                    Console.WriteLine($"Skipping story {id}: {ex.Message}");
                }
            });

        return stories;
    }
}
