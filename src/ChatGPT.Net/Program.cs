using ChatGPT.Net.DTO;
using ChatGPT.Net.Session;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var jsonString = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));

var config = JsonConvert.DeserializeObject<ChatGptConfig>(jsonString);

var chatGpt = new ChatGpt(config);
await chatGpt.WaitForReady();

app.MapGet("/", async (HttpRequest request) => Results.Ok(new
{
    Status = true
}));

app.MapGet("/chat", async (HttpRequest request) =>
{
    if (!request.Query.ContainsKey("q"))
        return Results.Ok(new
        {
            Status = false,
            Response = "Query is empty"
        });

    var query = request.Query["q"].ToString();
    var response = await chatGpt.Ask(query);
    return Results.Ok(new
    {
        Status = true,
        Response = response
    });
});

app.Run();