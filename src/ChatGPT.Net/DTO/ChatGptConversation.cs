namespace ChatGPT.Net.DTO;

public class ChatGptConversation
{
    public string ConversationId { get; set; }
    public string ParentMessageId { get; set; } = Guid.NewGuid().ToString();
}