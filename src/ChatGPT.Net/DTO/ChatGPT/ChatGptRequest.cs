using Newtonsoft.Json;

namespace ChatGPT.Net.DTO.ChatGPT;

public partial class ChatGptRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = "gpt-3.5-turbo";

    [JsonProperty("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonProperty("max_tokens")]
    public long MaxTokens { get; set; } = 256;

    [JsonProperty("n")]
    private long N { get; set; } = 1;

    [JsonProperty("stop")]
    public string[]? Stop { get; set; }

    [JsonProperty("top_p")]
    public double TopP { get; set; } = 0.9;
    [JsonProperty("presence_penalty")]
    public double PresencePenalty { get; set; } = 0.0;

    [JsonProperty("frequency_penalty")]
    public double FrequencyPenalty { get; set; } = 0.0;

    [JsonProperty("stream")]
    public bool Stream { get; set; } = false;

    [JsonProperty("messages")]
    public List<ChatGptMessage> Messages { get; set; } = new();
}