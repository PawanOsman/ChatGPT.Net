namespace ChatGPT.Net.DTO;

public class ChatGptUnofficialOptions : ChatGptConfig
{
    public string BaseUrl { get; set; } = "https://api.pawan.krd";
    public string Model { get; set; } = "text-davinci-002-render-sha";
}