using Newtonsoft.Json;

namespace ChatGPT.Net.DTO.ChatGPT;

public class ChatGptStreamChunkResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("object")]
    public string Object { get; set; }

    [JsonProperty("created")]
    public long Created { get; set; }

    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonProperty("choices")]
    public List<ChunkChoice> Choices { get; set; }
}

public class ChunkChoice
{
    [JsonProperty("delta")]
    public Delta Delta { get; set; }

    [JsonProperty("index")]
    public long Index { get; set; }

    [JsonProperty("finish_reason")]
    public object FinishReason { get; set; }
}

public class Delta
{
    [JsonProperty("content")]
    public string Content { get; set; }
}