using System.Text.Json.Serialization;

namespace ChatGPT.Net.DTO;

public class SessionInfo
{
    [JsonPropertyName("sessionToken")]
    public string SessionToken { get; set; }

    [JsonPropertyName("auth")]
    public string Auth { get; set; }

    [JsonPropertyName("expires")]
    public DateTime Expires { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
}