using System.Net.Http.Json;
using System.Text.Json;

namespace Santander.BestStories.Api.HackerNews;

public sealed class HackerNewsClient(HttpClient httpClient) : IHackerNewsClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        var ids = await httpClient.GetFromJsonAsync<long[]>(
            "beststories.json", SerializerOptions, cancellationToken);

        return ids ?? [];
    }

    public async Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<HackerNewsItem>(
            $"item/{id}.json", SerializerOptions, cancellationToken);
    }
}
