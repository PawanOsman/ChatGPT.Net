using Newtonsoft.Json;

namespace ChatGPT.Net.DTO.ChatGPT;

public class ChatGptResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("object")]
    public string Object { get; set; }

    [JsonProperty("created")]
    public long Created { get; set; }

    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonProperty("usage")]
    public Usage Usage { get; set; }

    [JsonProperty("choices")]
    public List<Choice> Choices { get; set; }

    [JsonProperty("error")]
    public Error? Error { get; set; }
}

public class Choice
{
    [JsonProperty("message")]
    public ChatGptMessage Message { get; set; }

    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; }

    [JsonProperty("index")]
    public long Index { get; set; }
}

public class Usage
{
    [JsonProperty("prompt_tokens")]
    public long PromptTokens { get; set; }

    [JsonProperty("completion_tokens")]
    public long CompletionTokens { get; set; }

    [JsonProperty("total_tokens")]
    public long TotalTokens { get; set; }
}

public class Error
{
    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("param")]
    public object Param { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }
}