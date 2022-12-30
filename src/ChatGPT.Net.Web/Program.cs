using ChatGPT.Net;
using ChatGPT.Net.DTO;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var clientConfigString = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
var clientConfig = JsonConvert.DeserializeObject<ChatGptClientConfig>(clientConfigString);

if (clientConfig is null) throw new Exception("Config is empty or incorrect");

var chatGpt = new ChatGpt(new ChatGptConfig
{
    UseCache = false
});

await chatGpt.WaitForReady();

var chatGptClient = await chatGpt.CreateClient(clientConfig);

app.MapGet("/", () => Results.Ok(new
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

    var id = "default";

    if(request.Query.ContainsKey("id")) id = request.Query["id"];

    var query = request.Query["q"].ToString();
    var response = await chatGptClient.Ask(query, id);

    return Results.Ok(new
    {
        Status = true,
        Response = response
    });
});

app.Run();