using System.Text.Json.Serialization;

namespace ChatGPT.Net.DTO;

public class ChatGptResponse
{
    [JsonPropertyName("answer")]
    public string Answer { get; set; }
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; }
    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
}