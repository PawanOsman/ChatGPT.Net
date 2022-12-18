using Newtonsoft.Json;

namespace ChatGPT.Net.DTO
{
    public class Reply
    {
        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("conversation_id")]
        public string ConversationId { get; set; }
    }
}
