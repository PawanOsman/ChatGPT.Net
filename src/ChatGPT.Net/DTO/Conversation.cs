using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ChatGPT.Net.DTO
{
    public class Conversation
    {
        [JsonProperty("action")]
        public string Action => "next";
        
        
        [JsonProperty("model")]
        public string Model => "text-davinci-002-render";

        [JsonProperty("parent_message_id")]
        public string ParentMessageId { get; set; }
        
        [JsonProperty("messages")]
        public Message[] Messages { get; set; }
    }

    public class Message 
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [JsonProperty("role")]
        public string Role { get; set; } = "user";
        
        [JsonProperty("content")]
        public Content Content { get; set; }
    }

    public class Content 
    {
        [JsonProperty("content_type")]
        public string ContentType => "text";
        
        [JsonProperty("parts")]
        public string[] Parts { get; set; }
    }
}
