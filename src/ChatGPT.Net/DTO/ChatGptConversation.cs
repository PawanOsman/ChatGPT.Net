namespace ChatGPT.Net.DTO;

public class ChatGptConversation
{
    public string Id { get; set; }
    public string ConversationId { get; set; }
    public string ParentMessageId { get; set; } = Guid.NewGuid().ToString();
}