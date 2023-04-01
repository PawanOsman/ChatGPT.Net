using Newtonsoft.Json;

namespace ChatGPT.Net.DTO.ChatGPTUnofficial;


public class ChatGptUnofficialMessageResponse
{
    [JsonProperty("message")]
    public MessageClass Message { get; set; }

    [JsonProperty("conversation_id")]
    public string ConversationId { get; set; }

    [JsonProperty("error")]
    public object Error { get; set; }
}

public class MessageClass
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("author")]
    public ResponseAuthor Author { get; set; }

    [JsonProperty("create_time")]
    public double CreateTime { get; set; }

    [JsonProperty("update_time")]
    public object UpdateTime { get; set; }

    [JsonProperty("content")]
    public ResponseContent Content { get; set; }

    [JsonProperty("end_turn")]
    public object EndTurn { get; set; }

    [JsonProperty("weight")]
    public long Weight { get; set; }

    [JsonProperty("metadata")]
    public MessageMetadata Metadata { get; set; }

    [JsonProperty("recipient")]
    public string Recipient { get; set; }
}

public class ResponseAuthor
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("name")]
    public object Name { get; set; }

    [JsonProperty("metadata")]
    public AuthorMetadata Metadata { get; set; }
}

public class AuthorMetadata
{
}

public class ResponseContent
{
    [JsonProperty("content_type")]
    public string ContentType { get; set; }

    [JsonProperty("parts")]
    public List<string> Parts { get; set; }
}

public class MessageMetadata
{
    [JsonProperty("message_type")]
    public string MessageType { get; set; }

    [JsonProperty("model_slug")]
    public string ModelSlug { get; set; }
}