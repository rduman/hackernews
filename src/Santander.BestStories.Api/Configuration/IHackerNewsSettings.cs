namespace Santander.BestStories.Api.Configuration;

public interface IHackerNewsSettings
{
    string BaseAddress { get; }

    TimeSpan RefreshInterval { get; }

    int MaxConcurrentItemRequests { get; }
}
