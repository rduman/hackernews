using Santander.BestStories.Api.Configuration;
using Santander.BestStories.Api.HackerNews;
using Santander.BestStories.Api.Middleware;
using Santander.BestStories.Api.Stories;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IHackerNewsSettings, HackerNewsSettings>();

var settings = new HackerNewsSettings(builder.Configuration);

builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>(client =>
{
    client.BaseAddress = new Uri(settings.BaseAddress);
});

builder.Services.AddSingleton<IBestStoriesProvider, BestStoriesProvider>();
builder.Services.AddHostedService<StoryRefreshService>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Best Stories API",
        Version = "v1",
        Description = "Returns the best Hacker News stories ordered by score, descending."
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(ui =>
{
    ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Best Stories API v1");
    ui.DocumentTitle = "Best Stories API";
});


app.MapControllers();

app.Run();
