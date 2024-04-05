using System.Text.Json;
using ContentAPI;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DBContext>();


builder.Services.AddHttpClient("SearchAPI")
    .AddPolicyHandler(GetCircuitBreakerPolicy());


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();


DBContext db = new();
db.Database.EnsureCreated();

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));
}

app.MapGet("/contents", async context =>
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var dbCo = services.GetRequiredService<DBContext>();

    var contents = await dbCo.Contents.ToListAsync();

    var json = JsonSerializer.Serialize(contents);
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(json);
});

app.MapGet("/searches", async context =>
{
    var searchApiUrl = "https://localhost:7215/searches";
    var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("SearchAPI");

    try
    {
        var response = await httpClient.GetAsync(searchApiUrl);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(responseContent);
        }
        else
        {
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync($"Failed to fetch data: {response.ReasonPhrase}");
        }
    }
    catch (BrokenCircuitException)
    {
        context.Response.StatusCode = 503;
        await context.Response.WriteAsync("Service geht nimma");
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"Error: {ex.Message}");
    }
});

app.MapControllers();

app.Run();
