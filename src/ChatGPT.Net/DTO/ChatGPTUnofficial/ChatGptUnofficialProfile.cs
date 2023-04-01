using Newtonsoft.Json;

namespace ChatGPT.Net.DTO.ChatGPTUnofficial;

public class ChatGptUnofficialProfile
{
    [JsonProperty("user")]
    public ChatGPTUnofficialUser User { get; set; }

    [JsonProperty("expires")]
    public DateTimeOffset Expires { get; set; }

    [JsonProperty("accessToken")]
    public string AccessToken { get; set; }
    [JsonProperty("error")]
    public string? Error { get; set; }
}

public class ChatGPTUnofficialUser
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("image")]
    public Uri Image { get; set; }

    [JsonProperty("picture")]
    public Uri Picture { get; set; }

    [JsonProperty("groups")]
    public List<string> Groups { get; set; }
}