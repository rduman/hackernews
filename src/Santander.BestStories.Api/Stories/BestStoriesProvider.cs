namespace Santander.BestStories.Api.Stories;

public interface IBestStoriesProvider
{
    bool TryGetTopStories(int count, out IReadOnlyList<StoryDto> stories);

    void Publish(StoryDto[] stories);
}

public sealed class BestStoriesProvider : IBestStoriesProvider
{
    private static StoryDto[] _snapshot = [];
    public void Publish(StoryDto[] stories) => _snapshot = stories;

    public bool TryGetTopStories(int count, out IReadOnlyList<StoryDto> stories)
    {
        var snapshot = _snapshot;

        if (snapshot.Length == 0)
        {
            stories = [];
            return false;
        }

        stories = new ArraySegment<StoryDto>(snapshot, 0, Math.Clamp(count, 0, snapshot.Length));
        return true;
    }
}
