# Best Stories API

Returns the best *n* Hacker News stories, ordered by score, highest first.

Built for the Santander backend coding test.

## Running it

Needs the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet run --project src/Santander.BestStories.Api
```

Swagger UI is at <http://localhost:5065/swagger>. The root URL redirects there.

```bash
curl "http://localhost:5065/api/v1/beststories?count=3"
```

```json
[
  {
    "title": "QBittorrent breaks out of sandbox to commit crimes",
    "uri": "https://beige.party/@intransitivelie/117057396732763183",
    "postedBy": "mraniki",
    "time": "2026-09-06T13:02:41+00:00",
    "score": 1276,
    "commentCount": 277
  }
]
```

## The endpoint

`GET /api/v1/beststories?count={n}`

| | |
| --- | --- |
| `count` | Optional, defaults to 10. Must be 1 or more. |
| `200` | Array of stories, highest score first. |
| `400` | `count` was below 1, or not a number. |
| `503` | The first few seconds after startup, before any data is loaded. |

## How it works

The brief asks the API to handle a lot of requests without overloading Hacker News. That is the hard
part, because Hacker News has no batch endpoint. Getting the best stories takes one call for the list
of IDs, then one call per story — about 200 calls in total.

Doing that on every request would mean 200 upstream calls for every incoming one. So a background
service does it once a minute instead, and stores the result:

```
StoryRefreshService  ──every 60s──►  Hacker News   (1 + ~200 calls)
        │
        │ stores a sorted array
        ▼
BestStoriesProvider
        │
        │ array slice, no network
        ▼
GET /api/v1/beststories  ◄──  any number of callers
```

Serving a request is then just reading from that array. Hacker News sees the same traffic whether we
handle one request a minute or ten thousand a second.

Fetching all 200 also solves a subtler problem. `beststories.json` comes back in Hacker News' own
ranking, which includes time decay — it is not score order. To return the true top *n* by score, you
need all of them to sort.

Three details worth knowing:

- The array is swapped, never edited, so readers always see a complete list.
- If a refresh fails, the previous data stays. Stale data beats no data.
- If every story fails to load, nothing is published. Otherwise a partial outage would become a total one.

## Configuration

Three settings under `HackerNews` in `appsettings.json`:

| Setting | Default | What it does |
| --- | --- | --- |
| `RefreshInterval` | 1 minute | How often data is refreshed. This sets the load on Hacker News. |
| `BaseAddress` | Hacker News v0 API | Where to fetch from. |
| `MaxConcurrentItemRequests` | 12 | How many stories are fetched at the same time. |

Development uses a 30 second interval.

## Assumptions

- **Top *n* by score means sorting all 200**, not sorting the first *n*. Hacker News' own order is not
  score order.
- **Data can be up to one refresh interval old.** Scores change constantly. This is the trade for not
  hammering the API.
- **`uri` can be null.** Ask HN posts have no URL. They are returned with `"uri": null` rather than
  dropped.
- **A missing `descendants` becomes `commentCount: 0`.** Hacker News omits the field instead of
  sending zero.
- **`title` is passed through as-is.** The docs describe it as HTML, so an entity could appear in it.
- **Only stories are returned.** No comments, jobs, polls, or deleted and dead items.
- **Asking for more than exist returns everything.** Hacker News returns 200 best stories today, but
  the docs do not promise that number, so nothing in the code assumes it.
- **Data is per-instance**, held in memory.
- **No authentication**, since the brief did not ask for any.

## What I would add with more time

- **Tests.** The mapping edge cases, the score ordering, and one test proving that many requests do
  not cause more upstream calls.
- **A shared cache.** Each instance keeps its own copy, so *N* instances mean *N* times the load on
  Hacker News. Redis plus a single refresher would fix that.
- **Smarter refreshing.** Hacker News has a `/v0/updates` endpoint listing recently changed items.
  Polling that, instead of refetching everything, would cut calls a lot.
- **Proper logging.** The refresh writes plain lines to the console. `ILogger` would give levels and
  structure.
- **Retries and a timeout on the HTTP client.** A failed call currently just skips that story, and the
  client uses the .NET default timeout of 100 seconds.
- **Config validation.** A `RefreshInterval` of zero would fail at runtime rather than at startup.
- **Output caching and rate limiting.** Neither is needed for the brief, but both would help under
  real load.
- **Load testing**, to put a number on the throughput claim.
