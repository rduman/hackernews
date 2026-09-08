using Santander.BestStories.Api.Stories;
using Microsoft.AspNetCore.Mvc;

namespace Santander.BestStories.Api.Controllers;

[ApiController]
[Route("api/v1/beststories")]
public sealed class BestStoriesController(IBestStoriesProvider provider) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<IReadOnlyList<StoryDto>> GetBestStories([FromQuery] int count = 10)
    {
        if (count < 1)
        {
            ModelState.AddModelError("count", "'count' must be 1 or greater.");
            return ValidationProblem(ModelState);
        }

        if (!provider.TryGetTopStories(count, out var stories))
        {
            return Problem(
                title: "Stories are not available yet.",
                detail: "The service is still building its first snapshot from the Hacker News API. Retry shortly.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Ok(stories);
    }
}
