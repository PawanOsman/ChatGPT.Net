using ChatGPT.Net;
using ChatGPT.Net.DTO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var chatGpt = new ChatGpt();
await chatGpt.WaitForReady();
var chatGptClient = await chatGpt.CreateClient(new ChatGptClientConfig
{
    SessionToken = "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIn0...."
});

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