using System.Text.Json.Serialization;

namespace ChatGPT.Net.DTO;

public class ChatGptRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }
    [JsonPropertyName("parentId")]
    public string ParentId { get; set; }
    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; }
    [JsonPropertyName("auth")]
    public string Auth { get; set; }
}