using Newtonsoft.Json;

namespace ChatGPT.Net.DTO;

public class ReplyMessage
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("role")]
    public string Role { get; set; }
    [JsonProperty("user")]
    public object User { get; set; }
    [JsonProperty("create_time")]
    public object CreateTime { get; set; }
    [JsonProperty("update_time")]
    public object UpdateTime { get; set; }
    [JsonProperty("content")]
    public Content Content { get; set; }
    [JsonProperty("end_turn")]
    public object EndTurn { get; set; }
    [JsonProperty("weight")]
    public double Weight { get; set; }
    [JsonProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; }
    [JsonProperty("recipient")]
    public string Recipient { get; set; }
}

public class ReplyMessageContent
{
    [JsonProperty("content_type")]
    public string ContentType { get; set; }
    [JsonProperty("parts")]
    public List<string> Parts { get; set; }
}

public class Reply
{
    [JsonProperty("message")]
    public Message Message { get; set; }
    [JsonProperty("conversation_id")]
    public string ConversationId { get; set; }
    [JsonProperty("error")]
    public object Error { get; set; }
}
