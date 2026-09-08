namespace Santander.BestStories.Api.Configuration;

public sealed class HackerNewsSettings : IHackerNewsSettings
{
    public const string SectionName = "HackerNews";

    public HackerNewsSettings(IConfiguration configuration)
    {
        configuration.GetSection(SectionName).Bind(this);
    }

    public string BaseAddress { get; set; } = "https://hacker-news.firebaseio.com/v0/";

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(1);

    public int MaxConcurrentItemRequests { get; set; } = 12;
}
