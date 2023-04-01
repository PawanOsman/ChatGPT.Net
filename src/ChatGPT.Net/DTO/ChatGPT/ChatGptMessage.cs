using Newtonsoft.Json;

namespace ChatGPT.Net.DTO.ChatGPT;

public class ChatGptMessage
{
    [JsonProperty("role")]
    public string Role { get; set; } = "user";

    [JsonProperty("content")]
    public string Content { get; set; }
}