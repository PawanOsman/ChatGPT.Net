using ChatGPT.Net;

// ChatGPT Official API
var bot = new ChatGpt("<API_KEY>");

var prompt = string.Empty;

while (true)
{
    Console.Write("You: ");
    prompt = Console.ReadLine();
    if (prompt is null) break;
    if (string.IsNullOrWhiteSpace(prompt)) break;
    if (prompt == "exit") break;
    Console.Write("ChatGPT: ");
    await bot.AskStream(Console.Write, prompt, "default");
    Console.WriteLine();
}